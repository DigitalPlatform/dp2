namespace dp2Circulation
{
    partial class ItemStatisForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemStatisForm));
            this.button_projectManage = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_source = new System.Windows.Forms.TabPage();
            this.tabComboBox_inputBatchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButton_inputStyle_recPathFile = new System.Windows.Forms.RadioButton();
            this.button_findInputRecPathFilename = new System.Windows.Forms.Button();
            this.textBox_inputRecPathFilename = new System.Windows.Forms.TextBox();
            this.radioButton_inputStyle_readerDatabase = new System.Windows.Forms.RadioButton();
            this.radioButton_inputStyle_barcodeFile = new System.Windows.Forms.RadioButton();
            this.comboBox_inputItemDbName = new System.Windows.Forms.ComboBox();
            this.label_inputItemDbName = new System.Windows.Forms.Label();
            this.button_findInputBarcodeFilename = new System.Windows.Forms.Button();
            this.textBox_inputBarcodeFilename = new System.Windows.Forms.TextBox();
            this.tabPage_filter = new System.Windows.Forms.TabPage();
            this.textBox_itemTypes = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_locationNames = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_selectProject = new System.Windows.Forms.TabPage();
            this.button_getProjectName = new System.Windows.Forms.Button();
            this.textBox_projectName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_runStatis = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_runStatis = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser1_running = new System.Windows.Forms.WebBrowser();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print = new System.Windows.Forms.Button();
            this.checkBox_selectProject_outputDebugInfo = new System.Windows.Forms.CheckBox();
            this.checkBox_runStatis_outputDebugInfo = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_source.SuspendLayout();
            this.tabPage_filter.SuspendLayout();
            this.tabPage_selectProject.SuspendLayout();
            this.tabPage_runStatis.SuspendLayout();
            this.tableLayoutPanel_runStatis.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_projectManage
            // 
            this.button_projectManage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_projectManage.Location = new System.Drawing.Point(0, 279);
            this.button_projectManage.Margin = new System.Windows.Forms.Padding(2);
            this.button_projectManage.Name = "button_projectManage";
            this.button_projectManage.Size = new System.Drawing.Size(94, 22);
            this.button_projectManage.TabIndex = 8;
            this.button_projectManage.Text = "方案管理(&M)...";
            this.button_projectManage.UseVisualStyleBackColor = true;
            this.button_projectManage.Click += new System.EventHandler(this.button_projectManage_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(290, 279);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(76, 22);
            this.button_next.TabIndex = 7;
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
            this.tabControl_main.Controls.Add(this.tabPage_filter);
            this.tabControl_main.Controls.Add(this.tabPage_selectProject);
            this.tabControl_main.Controls.Add(this.tabPage_runStatis);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(0, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(366, 264);
            this.tabControl_main.TabIndex = 6;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_source
            // 
            this.tabPage_source.Controls.Add(this.tabComboBox_inputBatchNo);
            this.tabPage_source.Controls.Add(this.label4);
            this.tabPage_source.Controls.Add(this.label2);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_recPathFile);
            this.tabPage_source.Controls.Add(this.button_findInputRecPathFilename);
            this.tabPage_source.Controls.Add(this.textBox_inputRecPathFilename);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_readerDatabase);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_barcodeFile);
            this.tabPage_source.Controls.Add(this.comboBox_inputItemDbName);
            this.tabPage_source.Controls.Add(this.label_inputItemDbName);
            this.tabPage_source.Controls.Add(this.button_findInputBarcodeFilename);
            this.tabPage_source.Controls.Add(this.textBox_inputBarcodeFilename);
            this.tabPage_source.Location = new System.Drawing.Point(4, 22);
            this.tabPage_source.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Size = new System.Drawing.Size(358, 238);
            this.tabPage_source.TabIndex = 4;
            this.tabPage_source.Text = "数据来源";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // tabComboBox_inputBatchNo
            // 
            this.tabComboBox_inputBatchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_inputBatchNo.FormattingEnabled = true;
            this.tabComboBox_inputBatchNo.LeftFontStyle = System.Drawing.FontStyle.Bold;
            this.tabComboBox_inputBatchNo.Location = new System.Drawing.Point(125, 145);
            this.tabComboBox_inputBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.tabComboBox_inputBatchNo.Name = "tabComboBox_inputBatchNo";
            this.tabComboBox_inputBatchNo.RightFontStyle = System.Drawing.FontStyle.Italic;
            this.tabComboBox_inputBatchNo.Size = new System.Drawing.Size(140, 22);
            this.tabComboBox_inputBatchNo.TabIndex = 8;
            this.tabComboBox_inputBatchNo.DropDown += new System.EventHandler(this.tabComboBox_inputBatchNo_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(123, 168);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "(为空则表示全部)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 148);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "批次号(&B):";
            // 
            // radioButton_inputStyle_recPathFile
            // 
            this.radioButton_inputStyle_recPathFile.AutoSize = true;
            this.radioButton_inputStyle_recPathFile.Location = new System.Drawing.Point(10, 64);
            this.radioButton_inputStyle_recPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_inputStyle_recPathFile.Name = "radioButton_inputStyle_recPathFile";
            this.radioButton_inputStyle_recPathFile.Size = new System.Drawing.Size(113, 16);
            this.radioButton_inputStyle_recPathFile.TabIndex = 3;
            this.radioButton_inputStyle_recPathFile.Text = "记录路径文件(&P)";
            this.radioButton_inputStyle_recPathFile.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_recPathFile.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_recPathFile_CheckedChanged);
            // 
            // button_findInputRecPathFilename
            // 
            this.button_findInputRecPathFilename.Enabled = false;
            this.button_findInputRecPathFilename.Location = new System.Drawing.Point(270, 84);
            this.button_findInputRecPathFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_findInputRecPathFilename.Name = "button_findInputRecPathFilename";
            this.button_findInputRecPathFilename.Size = new System.Drawing.Size(32, 22);
            this.button_findInputRecPathFilename.TabIndex = 5;
            this.button_findInputRecPathFilename.Text = "...";
            this.button_findInputRecPathFilename.UseVisualStyleBackColor = true;
            this.button_findInputRecPathFilename.Click += new System.EventHandler(this.button_findInputRecPathFilename_Click);
            // 
            // textBox_inputRecPathFilename
            // 
            this.textBox_inputRecPathFilename.Enabled = false;
            this.textBox_inputRecPathFilename.Location = new System.Drawing.Point(36, 84);
            this.textBox_inputRecPathFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_inputRecPathFilename.Name = "textBox_inputRecPathFilename";
            this.textBox_inputRecPathFilename.Size = new System.Drawing.Size(230, 21);
            this.textBox_inputRecPathFilename.TabIndex = 4;
            // 
            // radioButton_inputStyle_readerDatabase
            // 
            this.radioButton_inputStyle_readerDatabase.AutoSize = true;
            this.radioButton_inputStyle_readerDatabase.Checked = true;
            this.radioButton_inputStyle_readerDatabase.Location = new System.Drawing.Point(10, 118);
            this.radioButton_inputStyle_readerDatabase.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_inputStyle_readerDatabase.Name = "radioButton_inputStyle_readerDatabase";
            this.radioButton_inputStyle_readerDatabase.Size = new System.Drawing.Size(131, 16);
            this.radioButton_inputStyle_readerDatabase.TabIndex = 6;
            this.radioButton_inputStyle_readerDatabase.TabStop = true;
            this.radioButton_inputStyle_readerDatabase.Text = "整个库 / 批次号(&D)";
            this.radioButton_inputStyle_readerDatabase.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_readerDatabase.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_readerDatabase_CheckedChanged);
            // 
            // radioButton_inputStyle_barcodeFile
            // 
            this.radioButton_inputStyle_barcodeFile.AutoSize = true;
            this.radioButton_inputStyle_barcodeFile.Location = new System.Drawing.Point(10, 10);
            this.radioButton_inputStyle_barcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_inputStyle_barcodeFile.Name = "radioButton_inputStyle_barcodeFile";
            this.radioButton_inputStyle_barcodeFile.Size = new System.Drawing.Size(113, 16);
            this.radioButton_inputStyle_barcodeFile.TabIndex = 0;
            this.radioButton_inputStyle_barcodeFile.Text = "册条码号文件(&B)";
            this.radioButton_inputStyle_barcodeFile.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_barcodeFile.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_barcodeFile_CheckedChanged);
            // 
            // comboBox_inputItemDbName
            // 
            this.comboBox_inputItemDbName.FormattingEnabled = true;
            this.comboBox_inputItemDbName.Location = new System.Drawing.Point(125, 190);
            this.comboBox_inputItemDbName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_inputItemDbName.Name = "comboBox_inputItemDbName";
            this.comboBox_inputItemDbName.Size = new System.Drawing.Size(140, 20);
            this.comboBox_inputItemDbName.TabIndex = 11;
            this.comboBox_inputItemDbName.DropDown += new System.EventHandler(this.comboBox_inputItemDbName_DropDown);
            // 
            // label_inputItemDbName
            // 
            this.label_inputItemDbName.AutoSize = true;
            this.label_inputItemDbName.Location = new System.Drawing.Point(34, 192);
            this.label_inputItemDbName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_inputItemDbName.Name = "label_inputItemDbName";
            this.label_inputItemDbName.Size = new System.Drawing.Size(77, 12);
            this.label_inputItemDbName.TabIndex = 10;
            this.label_inputItemDbName.Text = "实体库名(&I):";
            // 
            // button_findInputBarcodeFilename
            // 
            this.button_findInputBarcodeFilename.Enabled = false;
            this.button_findInputBarcodeFilename.Location = new System.Drawing.Point(270, 30);
            this.button_findInputBarcodeFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_findInputBarcodeFilename.Name = "button_findInputBarcodeFilename";
            this.button_findInputBarcodeFilename.Size = new System.Drawing.Size(32, 22);
            this.button_findInputBarcodeFilename.TabIndex = 2;
            this.button_findInputBarcodeFilename.Text = "...";
            this.button_findInputBarcodeFilename.UseVisualStyleBackColor = true;
            this.button_findInputBarcodeFilename.Click += new System.EventHandler(this.button_findInputBarcodeFilename_Click);
            // 
            // textBox_inputBarcodeFilename
            // 
            this.textBox_inputBarcodeFilename.Enabled = false;
            this.textBox_inputBarcodeFilename.Location = new System.Drawing.Point(36, 30);
            this.textBox_inputBarcodeFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_inputBarcodeFilename.Name = "textBox_inputBarcodeFilename";
            this.textBox_inputBarcodeFilename.Size = new System.Drawing.Size(230, 21);
            this.textBox_inputBarcodeFilename.TabIndex = 1;
            // 
            // tabPage_filter
            // 
            this.tabPage_filter.Controls.Add(this.textBox_itemTypes);
            this.tabPage_filter.Controls.Add(this.label5);
            this.tabPage_filter.Controls.Add(this.textBox_locationNames);
            this.tabPage_filter.Controls.Add(this.label1);
            this.tabPage_filter.Location = new System.Drawing.Point(4, 22);
            this.tabPage_filter.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_filter.Name = "tabPage_filter";
            this.tabPage_filter.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_filter.Size = new System.Drawing.Size(358, 238);
            this.tabPage_filter.TabIndex = 0;
            this.tabPage_filter.Text = " 筛选特性 ";
            this.tabPage_filter.UseVisualStyleBackColor = true;
            // 
            // textBox_itemTypes
            // 
            this.textBox_itemTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_itemTypes.Location = new System.Drawing.Point(109, 36);
            this.textBox_itemTypes.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_itemTypes.Name = "textBox_itemTypes";
            this.textBox_itemTypes.Size = new System.Drawing.Size(223, 21);
            this.textBox_itemTypes.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 38);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "册类型(&T):";
            // 
            // textBox_locationNames
            // 
            this.textBox_locationNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_locationNames.Location = new System.Drawing.Point(109, 11);
            this.textBox_locationNames.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_locationNames.Name = "textBox_locationNames";
            this.textBox_locationNames.Size = new System.Drawing.Size(223, 21);
            this.textBox_locationNames.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "馆藏地点(&L):";
            // 
            // tabPage_selectProject
            // 
            this.tabPage_selectProject.Controls.Add(this.checkBox_selectProject_outputDebugInfo);
            this.tabPage_selectProject.Controls.Add(this.button_getProjectName);
            this.tabPage_selectProject.Controls.Add(this.textBox_projectName);
            this.tabPage_selectProject.Controls.Add(this.label3);
            this.tabPage_selectProject.Location = new System.Drawing.Point(4, 22);
            this.tabPage_selectProject.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_selectProject.Name = "tabPage_selectProject";
            this.tabPage_selectProject.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_selectProject.Size = new System.Drawing.Size(358, 238);
            this.tabPage_selectProject.TabIndex = 1;
            this.tabPage_selectProject.Text = " 选定方案 ";
            this.tabPage_selectProject.UseVisualStyleBackColor = true;
            // 
            // button_getProjectName
            // 
            this.button_getProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getProjectName.Location = new System.Drawing.Point(305, 9);
            this.button_getProjectName.Margin = new System.Windows.Forms.Padding(2);
            this.button_getProjectName.Name = "button_getProjectName";
            this.button_getProjectName.Size = new System.Drawing.Size(32, 22);
            this.button_getProjectName.TabIndex = 2;
            this.button_getProjectName.Text = "...";
            this.button_getProjectName.UseVisualStyleBackColor = true;
            this.button_getProjectName.Click += new System.EventHandler(this.button_getProjectName_Click);
            // 
            // textBox_projectName
            // 
            this.textBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_projectName.Location = new System.Drawing.Point(81, 9);
            this.textBox_projectName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_projectName.Name = "textBox_projectName";
            this.textBox_projectName.Size = new System.Drawing.Size(221, 21);
            this.textBox_projectName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "方案名(&P):";
            // 
            // tabPage_runStatis
            // 
            this.tabPage_runStatis.Controls.Add(this.tableLayoutPanel_runStatis);
            this.tabPage_runStatis.Location = new System.Drawing.Point(4, 22);
            this.tabPage_runStatis.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_runStatis.Name = "tabPage_runStatis";
            this.tabPage_runStatis.Size = new System.Drawing.Size(358, 238);
            this.tabPage_runStatis.TabIndex = 3;
            this.tabPage_runStatis.Text = " 执行统计 ";
            this.tabPage_runStatis.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_runStatis
            // 
            this.tableLayoutPanel_runStatis.ColumnCount = 1;
            this.tableLayoutPanel_runStatis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_runStatis.Controls.Add(this.webBrowser1_running, 0, 0);
            this.tableLayoutPanel_runStatis.Controls.Add(this.progressBar_records, 0, 2);
            this.tableLayoutPanel_runStatis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_runStatis.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_runStatis.Name = "tableLayoutPanel_runStatis";
            this.tableLayoutPanel_runStatis.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.tableLayoutPanel_runStatis.RowCount = 3;
            this.tableLayoutPanel_runStatis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_runStatis.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_runStatis.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_runStatis.Size = new System.Drawing.Size(358, 238);
            this.tableLayoutPanel_runStatis.TabIndex = 2;
            // 
            // webBrowser1_running
            // 
            this.webBrowser1_running.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1_running.Location = new System.Drawing.Point(2, 10);
            this.webBrowser1_running.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser1_running.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser1_running.Name = "webBrowser1_running";
            this.webBrowser1_running.Size = new System.Drawing.Size(354, 203);
            this.webBrowser1_running.TabIndex = 0;
            // 
            // progressBar_records
            // 
            this.progressBar_records.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar_records.Location = new System.Drawing.Point(2, 217);
            this.progressBar_records.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar_records.Name = "progressBar_records";
            this.progressBar_records.Size = new System.Drawing.Size(354, 11);
            this.progressBar_records.TabIndex = 1;
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 22);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(358, 238);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = " 打印结果 ";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print
            // 
            this.button_print.Location = new System.Drawing.Point(3, 14);
            this.button_print.Margin = new System.Windows.Forms.Padding(2);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(160, 22);
            this.button_print.TabIndex = 0;
            this.button_print.Text = "打印统计结果(&P)";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // checkBox_selectProject_outputDebugInfo
            // 
            this.checkBox_selectProject_outputDebugInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_selectProject_outputDebugInfo.AutoSize = true;
            this.checkBox_selectProject_outputDebugInfo.Location = new System.Drawing.Point(8, 217);
            this.checkBox_selectProject_outputDebugInfo.Name = "checkBox_selectProject_outputDebugInfo";
            this.checkBox_selectProject_outputDebugInfo.Size = new System.Drawing.Size(114, 16);
            this.checkBox_selectProject_outputDebugInfo.TabIndex = 3;
            this.checkBox_selectProject_outputDebugInfo.Text = "输出调试信息(&O)";
            this.checkBox_selectProject_outputDebugInfo.UseVisualStyleBackColor = true;
            // 
            // checkBox_runStatis_outputDebugInfo
            // 
            this.checkBox_runStatis_outputDebugInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_runStatis_outputDebugInfo.AutoSize = true;
            this.checkBox_runStatis_outputDebugInfo.Location = new System.Drawing.Point(8, 217);
            this.checkBox_runStatis_outputDebugInfo.Name = "checkBox_runStatis_outputDebugInfo";
            this.checkBox_runStatis_outputDebugInfo.Size = new System.Drawing.Size(114, 16);
            this.checkBox_runStatis_outputDebugInfo.TabIndex = 3;
            this.checkBox_runStatis_outputDebugInfo.Text = "输出调试信息(&O)";
            this.checkBox_runStatis_outputDebugInfo.UseVisualStyleBackColor = true;
            // 
            // ItemStatisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 311);
            this.Controls.Add(this.button_projectManage);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ItemStatisForm";
            this.ShowInTaskbar = false;
            this.Text = "册统计窗";
            this.Activated += new System.EventHandler(this.ItemStatisForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ItemStatisForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ItemStatisForm_FormClosed);
            this.Load += new System.EventHandler(this.ItemStatisForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_source.ResumeLayout(false);
            this.tabPage_source.PerformLayout();
            this.tabPage_filter.ResumeLayout(false);
            this.tabPage_filter.PerformLayout();
            this.tabPage_selectProject.ResumeLayout(false);
            this.tabPage_selectProject.PerformLayout();
            this.tabPage_runStatis.ResumeLayout(false);
            this.tableLayoutPanel_runStatis.ResumeLayout(false);
            this.tabPage_print.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_projectManage;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_source;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_recPathFile;
        private System.Windows.Forms.Button button_findInputRecPathFilename;
        private System.Windows.Forms.TextBox textBox_inputRecPathFilename;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_readerDatabase;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_barcodeFile;
        private System.Windows.Forms.ComboBox comboBox_inputItemDbName;
        private System.Windows.Forms.Label label_inputItemDbName;
        private System.Windows.Forms.Button button_findInputBarcodeFilename;
        private System.Windows.Forms.TextBox textBox_inputBarcodeFilename;
        private System.Windows.Forms.TabPage tabPage_filter;
        private System.Windows.Forms.TextBox textBox_itemTypes;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_locationNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_selectProject;
        private System.Windows.Forms.Button button_getProjectName;
        private System.Windows.Forms.TextBox textBox_projectName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_runStatis;
        private System.Windows.Forms.ProgressBar progressBar_records;
        private System.Windows.Forms.WebBrowser webBrowser1_running;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_inputBatchNo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_runStatis;
        private System.Windows.Forms.CheckBox checkBox_selectProject_outputDebugInfo;
        private System.Windows.Forms.CheckBox checkBox_runStatis_outputDebugInfo;
    }
}