namespace dp2Circulation
{
    partial class AccountBookForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountBookForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_load = new System.Windows.Forms.TabPage();
            this.checkBox_load_fillBiblioSummary = new System.Windows.Forms.CheckBox();
            this.checkBox_load_fillOrderInfo = new System.Windows.Forms.CheckBox();
            this.button_load_loadFromRecPathFile = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_load_type = new System.Windows.Forms.ComboBox();
            this.button_load_loadFromBatchNo = new System.Windows.Forms.Button();
            this.button_load_loadFromBarcodeFile = new System.Windows.Forms.Button();
            this.tabPage_sort = new System.Windows.Forms.TabPage();
            this.comboBox_sort_sortStyle = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print_createNewScriptFile = new System.Windows.Forms.Button();
            this.button_print_runScript = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_print_optionWordXml = new System.Windows.Forms.Button();
            this.button_print_outputWordXmlFile = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_print_outputExcelFile = new System.Windows.Forms.Button();
            this.button_print_optionText = new System.Windows.Forms.Button();
            this.button_print_outputTextFile = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_print_printNormalList = new System.Windows.Forms.Button();
            this.button_print_optionHTML = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.listView_in = new DigitalPlatform.GUI.ListViewNF();
            this.imageList_lineType = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl_main.SuspendLayout();
            this.tabPage_load.SuspendLayout();
            this.tabPage_sort.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_load);
            this.tabControl_main.Controls.Add(this.tabPage_sort);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(2, 2);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(506, 141);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_load
            // 
            this.tabPage_load.AutoScroll = true;
            this.tabPage_load.Controls.Add(this.checkBox_load_fillBiblioSummary);
            this.tabPage_load.Controls.Add(this.checkBox_load_fillOrderInfo);
            this.tabPage_load.Controls.Add(this.button_load_loadFromRecPathFile);
            this.tabPage_load.Controls.Add(this.label4);
            this.tabPage_load.Controls.Add(this.comboBox_load_type);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBatchNo);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBarcodeFile);
            this.tabPage_load.Location = new System.Drawing.Point(4, 22);
            this.tabPage_load.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_load.Name = "tabPage_load";
            this.tabPage_load.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_load.Size = new System.Drawing.Size(498, 165);
            this.tabPage_load.TabIndex = 0;
            this.tabPage_load.Text = "装载";
            this.tabPage_load.UseVisualStyleBackColor = true;
            // 
            // checkBox_load_fillBiblioSummary
            // 
            this.checkBox_load_fillBiblioSummary.AutoSize = true;
            this.checkBox_load_fillBiblioSummary.Location = new System.Drawing.Point(6, 63);
            this.checkBox_load_fillBiblioSummary.Name = "checkBox_load_fillBiblioSummary";
            this.checkBox_load_fillBiblioSummary.Size = new System.Drawing.Size(114, 16);
            this.checkBox_load_fillBiblioSummary.TabIndex = 3;
            this.checkBox_load_fillBiblioSummary.Text = "包含书目摘要(&B)";
            this.checkBox_load_fillBiblioSummary.UseVisualStyleBackColor = true;
            // 
            // checkBox_load_fillOrderInfo
            // 
            this.checkBox_load_fillOrderInfo.AutoSize = true;
            this.checkBox_load_fillOrderInfo.Location = new System.Drawing.Point(5, 44);
            this.checkBox_load_fillOrderInfo.Name = "checkBox_load_fillOrderInfo";
            this.checkBox_load_fillOrderInfo.Size = new System.Drawing.Size(114, 16);
            this.checkBox_load_fillOrderInfo.TabIndex = 2;
            this.checkBox_load_fillOrderInfo.Text = "包含订购信息(&O)";
            this.checkBox_load_fillOrderInfo.UseVisualStyleBackColor = true;
            // 
            // button_load_loadFromRecPathFile
            // 
            this.button_load_loadFromRecPathFile.Location = new System.Drawing.Point(160, 32);
            this.button_load_loadFromRecPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromRecPathFile.Name = "button_load_loadFromRecPathFile";
            this.button_load_loadFromRecPathFile.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromRecPathFile.TabIndex = 5;
            this.button_load_loadFromRecPathFile.Text = "从记录路径文件装载(&R)...";
            this.button_load_loadFromRecPathFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromRecPathFile.Click += new System.EventHandler(this.button_load_loadFromRecPathFile_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 5);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "出版物类型(&T):";
            // 
            // comboBox_load_type
            // 
            this.comboBox_load_type.FormattingEnabled = true;
            this.comboBox_load_type.Items.AddRange(new object[] {
            "图书",
            "连续出版物"});
            this.comboBox_load_type.Location = new System.Drawing.Point(6, 19);
            this.comboBox_load_type.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_load_type.Name = "comboBox_load_type";
            this.comboBox_load_type.Size = new System.Drawing.Size(116, 20);
            this.comboBox_load_type.TabIndex = 1;
            this.comboBox_load_type.Text = "图书";
            // 
            // button_load_loadFromBatchNo
            // 
            this.button_load_loadFromBatchNo.Location = new System.Drawing.Point(160, 59);
            this.button_load_loadFromBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromBatchNo.Name = "button_load_loadFromBatchNo";
            this.button_load_loadFromBatchNo.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromBatchNo.TabIndex = 6;
            this.button_load_loadFromBatchNo.Text = "根据批次号检索装载(&B)...";
            this.button_load_loadFromBatchNo.UseVisualStyleBackColor = true;
            this.button_load_loadFromBatchNo.Click += new System.EventHandler(this.button_load_loadFromBatchNo_Click);
            // 
            // button_load_loadFromBarcodeFile
            // 
            this.button_load_loadFromBarcodeFile.Location = new System.Drawing.Point(160, 5);
            this.button_load_loadFromBarcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromBarcodeFile.Name = "button_load_loadFromBarcodeFile";
            this.button_load_loadFromBarcodeFile.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromBarcodeFile.TabIndex = 4;
            this.button_load_loadFromBarcodeFile.Text = "从条码号文件装载(&F)...";
            this.button_load_loadFromBarcodeFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromBarcodeFile.Click += new System.EventHandler(this.button_load_loadFromBarcodeFile_Click);
            // 
            // tabPage_sort
            // 
            this.tabPage_sort.Controls.Add(this.comboBox_sort_sortStyle);
            this.tabPage_sort.Controls.Add(this.label2);
            this.tabPage_sort.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sort.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_sort.Name = "tabPage_sort";
            this.tabPage_sort.Size = new System.Drawing.Size(498, 165);
            this.tabPage_sort.TabIndex = 3;
            this.tabPage_sort.Text = "排序";
            this.tabPage_sort.UseVisualStyleBackColor = true;
            // 
            // comboBox_sort_sortStyle
            // 
            this.comboBox_sort_sortStyle.FormattingEnabled = true;
            this.comboBox_sort_sortStyle.Items.AddRange(new object[] {
            "<无>",
            "册条码号",
            "登录号",
            "渠道",
            "经费来源"});
            this.comboBox_sort_sortStyle.Location = new System.Drawing.Point(98, 8);
            this.comboBox_sort_sortStyle.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_sort_sortStyle.Name = "comboBox_sort_sortStyle";
            this.comboBox_sort_sortStyle.Size = new System.Drawing.Size(184, 20);
            this.comboBox_sort_sortStyle.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "排序策略(&S):";
            // 
            // tabPage_print
            // 
            this.tabPage_print.AutoScroll = true;
            this.tabPage_print.Controls.Add(this.button_print_createNewScriptFile);
            this.tabPage_print.Controls.Add(this.button_print_runScript);
            this.tabPage_print.Controls.Add(this.groupBox3);
            this.tabPage_print.Controls.Add(this.groupBox2);
            this.tabPage_print.Controls.Add(this.groupBox1);
            this.tabPage_print.Location = new System.Drawing.Point(4, 22);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(498, 115);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = "打印";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print_createNewScriptFile
            // 
            this.button_print_createNewScriptFile.Location = new System.Drawing.Point(19, 122);
            this.button_print_createNewScriptFile.Name = "button_print_createNewScriptFile";
            this.button_print_createNewScriptFile.Size = new System.Drawing.Size(152, 23);
            this.button_print_createNewScriptFile.TabIndex = 4;
            this.button_print_createNewScriptFile.Text = "创建新的脚本文件(&C) ...";
            this.button_print_createNewScriptFile.UseVisualStyleBackColor = true;
            this.button_print_createNewScriptFile.Click += new System.EventHandler(this.button_print_createNewScriptFile_Click);
            // 
            // button_print_runScript
            // 
            this.button_print_runScript.Location = new System.Drawing.Point(19, 95);
            this.button_print_runScript.Name = "button_print_runScript";
            this.button_print_runScript.Size = new System.Drawing.Size(152, 23);
            this.button_print_runScript.TabIndex = 3;
            this.button_print_runScript.Text = "执行脚本(&S) ...";
            this.button_print_runScript.UseVisualStyleBackColor = true;
            this.button_print_runScript.Click += new System.EventHandler(this.button_print_runScript_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button_print_optionWordXml);
            this.groupBox3.Controls.Add(this.button_print_outputWordXmlFile);
            this.groupBox3.Location = new System.Drawing.Point(394, 3);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(180, 75);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = " WordML ";
            // 
            // button_print_optionWordXml
            // 
            this.button_print_optionWordXml.Location = new System.Drawing.Point(12, 43);
            this.button_print_optionWordXml.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_optionWordXml.Name = "button_print_optionWordXml";
            this.button_print_optionWordXml.Size = new System.Drawing.Size(152, 22);
            this.button_print_optionWordXml.TabIndex = 1;
            this.button_print_optionWordXml.Text = "输出配置(&C)...";
            this.button_print_optionWordXml.UseVisualStyleBackColor = true;
            this.button_print_optionWordXml.Click += new System.EventHandler(this.button_print_optionWordXml_Click);
            // 
            // button_print_outputWordXmlFile
            // 
            this.button_print_outputWordXmlFile.Location = new System.Drawing.Point(12, 16);
            this.button_print_outputWordXmlFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_outputWordXmlFile.Name = "button_print_outputWordXmlFile";
            this.button_print_outputWordXmlFile.Size = new System.Drawing.Size(152, 22);
            this.button_print_outputWordXmlFile.TabIndex = 0;
            this.button_print_outputWordXmlFile.Text = "输出到 WordML 文件(&X)...";
            this.button_print_outputWordXmlFile.UseVisualStyleBackColor = true;
            this.button_print_outputWordXmlFile.Click += new System.EventHandler(this.button_print_outputWordXmlFile_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_print_outputExcelFile);
            this.groupBox2.Controls.Add(this.button_print_optionText);
            this.groupBox2.Controls.Add(this.button_print_outputTextFile);
            this.groupBox2.Location = new System.Drawing.Point(200, 3);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(180, 97);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " 纯文本 ";
            // 
            // button_print_outputExcelFile
            // 
            this.button_print_outputExcelFile.Location = new System.Drawing.Point(12, 42);
            this.button_print_outputExcelFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_outputExcelFile.Name = "button_print_outputExcelFile";
            this.button_print_outputExcelFile.Size = new System.Drawing.Size(152, 22);
            this.button_print_outputExcelFile.TabIndex = 1;
            this.button_print_outputExcelFile.Text = "输出到 Excel 文件(&E)...";
            this.button_print_outputExcelFile.UseVisualStyleBackColor = true;
            this.button_print_outputExcelFile.Click += new System.EventHandler(this.button_print_outputExcelFile_Click);
            // 
            // button_print_optionText
            // 
            this.button_print_optionText.Location = new System.Drawing.Point(12, 71);
            this.button_print_optionText.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_optionText.Name = "button_print_optionText";
            this.button_print_optionText.Size = new System.Drawing.Size(152, 22);
            this.button_print_optionText.TabIndex = 2;
            this.button_print_optionText.Text = "输出配置(&N)...";
            this.button_print_optionText.UseVisualStyleBackColor = true;
            this.button_print_optionText.Click += new System.EventHandler(this.button_print_optionText_Click);
            // 
            // button_print_outputTextFile
            // 
            this.button_print_outputTextFile.Location = new System.Drawing.Point(12, 16);
            this.button_print_outputTextFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_outputTextFile.Name = "button_print_outputTextFile";
            this.button_print_outputTextFile.Size = new System.Drawing.Size(152, 22);
            this.button_print_outputTextFile.TabIndex = 0;
            this.button_print_outputTextFile.Text = "输出到文本文件(&T)...";
            this.button_print_outputTextFile.UseVisualStyleBackColor = true;
            this.button_print_outputTextFile.Click += new System.EventHandler(this.button_print_outputTextFile_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_print_printNormalList);
            this.groupBox1.Controls.Add(this.button_print_optionHTML);
            this.groupBox1.Location = new System.Drawing.Point(7, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(180, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " HTML ";
            // 
            // button_print_printNormalList
            // 
            this.button_print_printNormalList.Location = new System.Drawing.Point(12, 16);
            this.button_print_printNormalList.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_printNormalList.Name = "button_print_printNormalList";
            this.button_print_printNormalList.Size = new System.Drawing.Size(152, 22);
            this.button_print_printNormalList.TabIndex = 0;
            this.button_print_printNormalList.Text = "打印(&P)...";
            this.button_print_printNormalList.UseVisualStyleBackColor = true;
            this.button_print_printNormalList.Click += new System.EventHandler(this.button_print_printNormalList_Click);
            // 
            // button_print_optionHTML
            // 
            this.button_print_optionHTML.Location = new System.Drawing.Point(12, 43);
            this.button_print_optionHTML.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_optionHTML.Name = "button_print_optionHTML";
            this.button_print_optionHTML.Size = new System.Drawing.Size(152, 22);
            this.button_print_optionHTML.TabIndex = 1;
            this.button_print_optionHTML.Text = "打印配置(&O)...";
            this.button_print_optionHTML.UseVisualStyleBackColor = true;
            this.button_print_optionHTML.Click += new System.EventHandler(this.button_print_optionHTML_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(425, 147);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(83, 22);
            this.button_next.TabIndex = 1;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // listView_in
            // 
            this.listView_in.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_in.FullRowSelect = true;
            this.listView_in.HideSelection = false;
            this.listView_in.LargeImageList = this.imageList_lineType;
            this.listView_in.Location = new System.Drawing.Point(0, 0);
            this.listView_in.Margin = new System.Windows.Forms.Padding(2);
            this.listView_in.Name = "listView_in";
            this.listView_in.Size = new System.Drawing.Size(510, 132);
            this.listView_in.SmallImageList = this.imageList_lineType;
            this.listView_in.TabIndex = 2;
            this.listView_in.UseCompatibleStateImageBehavior = false;
            this.listView_in.View = System.Windows.Forms.View.Details;
            this.listView_in.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_in_ColumnClick);
            this.listView_in.SelectedIndexChanged += new System.EventHandler(this.listView_in_SelectedIndexChanged);
            this.listView_in.DoubleClick += new System.EventHandler(this.listView_in_DoubleClick);
            this.listView_in.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_in_MouseUp);
            // 
            // imageList_lineType
            // 
            this.imageList_lineType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_lineType.ImageStream")));
            this.imageList_lineType.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_lineType.Images.SetKeyName(0, "WarningHS.png");
            this.imageList_lineType.Images.SetKeyName(1, "Book_angleHS.png");
            this.imageList_lineType.Images.SetKeyName(2, "Book_openHS.png");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl_main);
            this.splitContainer1.Panel1.Controls.Add(this.button_next);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView_in);
            this.splitContainer1.Size = new System.Drawing.Size(510, 310);
            this.splitContainer1.SplitterDistance = 170;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 3;
            // 
            // AccountBookForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 310);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AccountBookForm";
            this.Text = "打印财产账";
            this.Activated += new System.EventHandler(this.AccountBookForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AccountBookForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AccountBookForm_FormClosed);
            this.Load += new System.EventHandler(this.AccountBookForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_load.ResumeLayout(false);
            this.tabPage_load.PerformLayout();
            this.tabPage_sort.ResumeLayout(false);
            this.tabPage_sort.PerformLayout();
            this.tabPage_print.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_load;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabPage tabPage_print;
        private DigitalPlatform.GUI.ListViewNF listView_in;
        private System.Windows.Forms.Button button_load_loadFromBatchNo;
        private System.Windows.Forms.Button button_load_loadFromBarcodeFile;
        private System.Windows.Forms.ImageList imageList_lineType;
        private System.Windows.Forms.Button button_print_optionHTML;
        private System.Windows.Forms.Button button_print_printNormalList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_load_type;
        private System.Windows.Forms.Button button_print_outputTextFile;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_print_optionText;
        private System.Windows.Forms.TabPage tabPage_sort;
        private System.Windows.Forms.ComboBox comboBox_sort_sortStyle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button_print_optionWordXml;
        private System.Windows.Forms.Button button_print_outputWordXmlFile;
        private System.Windows.Forms.Button button_load_loadFromRecPathFile;
        private System.Windows.Forms.CheckBox checkBox_load_fillOrderInfo;
        private System.Windows.Forms.CheckBox checkBox_load_fillBiblioSummary;
        private System.Windows.Forms.Button button_print_runScript;
        private System.Windows.Forms.Button button_print_createNewScriptFile;
        private System.Windows.Forms.Button button_print_outputExcelFile;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}