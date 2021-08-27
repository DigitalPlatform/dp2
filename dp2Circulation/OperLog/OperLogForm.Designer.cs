namespace dp2Circulation
{
    partial class OperLogForm
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

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperLogForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_logFileName = new System.Windows.Forms.TextBox();
            this.splitContainer_logRecords = new System.Windows.Forms.SplitContainer();
            this.listView_records = new DigitalPlatform.GUI.ListViewQU();
            this.columnHeader_filename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_operType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_operator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_operTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_seconds = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_attachment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel_record = new System.Windows.Forms.Panel();
            this.toolStrip_panelFixed = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_closeDownPanel = new System.Windows.Forms.ToolStripButton();
            this.tabControl_record = new System.Windows.Forms.TabControl();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.webBrowser_xml = new System.Windows.Forms.WebBrowser();
            this.tabPage_html = new System.Windows.Forms.TabPage();
            this.webBrowser_html = new System.Windows.Forms.WebBrowser();
            this.button_loadFromSingleFile = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_selectFile = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_filter = new System.Windows.Forms.TextBox();
            this.button_getTodayFilename = new System.Windows.Forms.Button();
            this.button_getSingleLogFilename = new System.Windows.Forms.Button();
            this.tabPage_selectFiles = new System.Windows.Forms.TabPage();
            this.comboBox_quickSetFilenames = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button_loadFilenams = new System.Windows.Forms.Button();
            this.button_loadLogRecords = new System.Windows.Forms.Button();
            this.textBox_filenames = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.tabPage_logRecords = new System.Windows.Forms.TabPage();
            this.tabPage_repair = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_repair_verify = new System.Windows.Forms.Button();
            this.button_repair_findVerifyFolderName = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_repair_verifyFolderName = new System.Windows.Forms.TextBox();
            this.button_repair_repair = new System.Windows.Forms.Button();
            this.button_repair_findTargetFilename = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_repair_targetFilename = new System.Windows.Forms.TextBox();
            this.button_repair_findSourceFilename = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_repair_sourceFilename = new System.Windows.Forms.TextBox();
            this.tabPage_file = new System.Windows.Forms.TabPage();
            this.listView_file = new System.Windows.Forms.ListView();
            this.columnHeader_file_index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_file_size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_file_operation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_file_operator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip_file = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_file_load = new System.Windows.Forms.ToolStripButton();
            this.tabPage_restore = new System.Windows.Forms.TabPage();
            this.splitContainer_recover = new System.Windows.Forms.SplitContainer();
            this.listView_restoreList = new DigitalPlatform.GUI.ListViewQU();
            this.columnHeader_restoreList_no = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_restoreList_recPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_restoreList_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_restoreList_history = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listView_recover_history = new DigitalPlatform.GUI.ListViewQU();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_logRecords)).BeginInit();
            this.splitContainer_logRecords.Panel1.SuspendLayout();
            this.splitContainer_logRecords.Panel2.SuspendLayout();
            this.splitContainer_logRecords.SuspendLayout();
            this.panel_record.SuspendLayout();
            this.toolStrip_panelFixed.SuspendLayout();
            this.tabControl_record.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_selectFile.SuspendLayout();
            this.tabPage_selectFiles.SuspendLayout();
            this.tabPage_logRecords.SuspendLayout();
            this.tabPage_repair.SuspendLayout();
            this.tabPage_file.SuspendLayout();
            this.toolStrip_file.SuspendLayout();
            this.tabPage_restore.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_recover)).BeginInit();
            this.splitContainer_recover.Panel1.SuspendLayout();
            this.splitContainer_recover.Panel2.SuspendLayout();
            this.splitContainer_recover.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "日志文件名(&F):";
            // 
            // textBox_logFileName
            // 
            this.textBox_logFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_logFileName.Location = new System.Drawing.Point(169, 18);
            this.textBox_logFileName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_logFileName.Name = "textBox_logFileName";
            this.textBox_logFileName.Size = new System.Drawing.Size(332, 31);
            this.textBox_logFileName.TabIndex = 1;
            // 
            // splitContainer_logRecords
            // 
            this.splitContainer_logRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_logRecords.Location = new System.Drawing.Point(4, 4);
            this.splitContainer_logRecords.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer_logRecords.Name = "splitContainer_logRecords";
            this.splitContainer_logRecords.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_logRecords.Panel1
            // 
            this.splitContainer_logRecords.Panel1.Controls.Add(this.listView_records);
            // 
            // splitContainer_logRecords.Panel2
            // 
            this.splitContainer_logRecords.Panel2.Controls.Add(this.panel_record);
            this.splitContainer_logRecords.Size = new System.Drawing.Size(673, 410);
            this.splitContainer_logRecords.SplitterDistance = 191;
            this.splitContainer_logRecords.SplitterWidth = 5;
            this.splitContainer_logRecords.TabIndex = 2;
            // 
            // listView_records
            // 
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_filename,
            this.columnHeader_index,
            this.columnHeader_libraryCode,
            this.columnHeader_operType,
            this.columnHeader_operator,
            this.columnHeader_operTime,
            this.columnHeader_seconds,
            this.columnHeader_attachment,
            this.columnHeader_comment});
            this.listView_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(0, 0);
            this.listView_records.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listView_records.Name = "listView_records";
            this.listView_records.Size = new System.Drawing.Size(673, 191);
            this.listView_records.TabIndex = 0;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            this.listView_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_records_ColumnClick);
            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            this.listView_records.DoubleClick += new System.EventHandler(this.listView_records_DoubleClick);
            this.listView_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_records_MouseUp);
            // 
            // columnHeader_filename
            // 
            this.columnHeader_filename.Text = "文件名";
            this.columnHeader_filename.Width = 150;
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "序号";
            this.columnHeader_index.Width = 100;
            // 
            // columnHeader_libraryCode
            // 
            this.columnHeader_libraryCode.Text = "馆代码";
            this.columnHeader_libraryCode.Width = 127;
            // 
            // columnHeader_operType
            // 
            this.columnHeader_operType.Text = "操作类型";
            this.columnHeader_operType.Width = 200;
            // 
            // columnHeader_operator
            // 
            this.columnHeader_operator.Text = "操作者";
            this.columnHeader_operator.Width = 200;
            // 
            // columnHeader_operTime
            // 
            this.columnHeader_operTime.Text = "操作时间";
            this.columnHeader_operTime.Width = 200;
            // 
            // columnHeader_seconds
            // 
            this.columnHeader_seconds.Text = "耗时(秒)";
            this.columnHeader_seconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader_attachment
            // 
            this.columnHeader_attachment.Text = "附件";
            this.columnHeader_attachment.Width = 87;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "附注";
            this.columnHeader_comment.Width = 160;
            // 
            // panel_record
            // 
            this.panel_record.Controls.Add(this.toolStrip_panelFixed);
            this.panel_record.Controls.Add(this.tabControl_record);
            this.panel_record.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_record.Location = new System.Drawing.Point(0, 0);
            this.panel_record.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_record.Name = "panel_record";
            this.panel_record.Size = new System.Drawing.Size(673, 214);
            this.panel_record.TabIndex = 2;
            // 
            // toolStrip_panelFixed
            // 
            this.toolStrip_panelFixed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_panelFixed.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_panelFixed.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_panelFixed.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip_panelFixed.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_closeDownPanel});
            this.toolStrip_panelFixed.Location = new System.Drawing.Point(628, 0);
            this.toolStrip_panelFixed.Name = "toolStrip_panelFixed";
            this.toolStrip_panelFixed.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip_panelFixed.Size = new System.Drawing.Size(46, 25);
            this.toolStrip_panelFixed.TabIndex = 4;
            this.toolStrip_panelFixed.Text = "toolStrip1";
            // 
            // toolStripButton_closeDownPanel
            // 
            this.toolStripButton_closeDownPanel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_closeDownPanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_closeDownPanel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_closeDownPanel.Image")));
            this.toolStripButton_closeDownPanel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_closeDownPanel.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_closeDownPanel.Name = "toolStripButton_closeDownPanel";
            this.toolStripButton_closeDownPanel.Size = new System.Drawing.Size(40, 19);
            this.toolStripButton_closeDownPanel.Text = "隐藏面板";
            this.toolStripButton_closeDownPanel.Click += new System.EventHandler(this.toolStripButton_closeDownPanel_Click);
            // 
            // tabControl_record
            // 
            this.tabControl_record.Controls.Add(this.tabPage_xml);
            this.tabControl_record.Controls.Add(this.tabPage_html);
            this.tabControl_record.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_record.Location = new System.Drawing.Point(0, 0);
            this.tabControl_record.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabControl_record.Name = "tabControl_record";
            this.tabControl_record.SelectedIndex = 0;
            this.tabControl_record.Size = new System.Drawing.Size(673, 214);
            this.tabControl_record.TabIndex = 1;
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.Controls.Add(this.webBrowser_xml);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 31);
            this.tabPage_xml.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_xml.Size = new System.Drawing.Size(665, 179);
            this.tabPage_xml.TabIndex = 0;
            this.tabPage_xml.Text = "XML";
            this.tabPage_xml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_xml
            // 
            this.webBrowser_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_xml.Location = new System.Drawing.Point(6, 5);
            this.webBrowser_xml.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.webBrowser_xml.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser_xml.Name = "webBrowser_xml";
            this.webBrowser_xml.Size = new System.Drawing.Size(653, 169);
            this.webBrowser_xml.TabIndex = 0;
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser_html);
            this.tabPage_html.Location = new System.Drawing.Point(4, 31);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_html.Size = new System.Drawing.Size(659, 173);
            this.tabPage_html.TabIndex = 1;
            this.tabPage_html.Text = "详细";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // webBrowser_html
            // 
            this.webBrowser_html.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_html.Location = new System.Drawing.Point(6, 5);
            this.webBrowser_html.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser_html.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_html.Name = "webBrowser_html";
            this.webBrowser_html.Size = new System.Drawing.Size(647, 163);
            this.webBrowser_html.TabIndex = 0;
            // 
            // button_loadFromSingleFile
            // 
            this.button_loadFromSingleFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadFromSingleFile.Location = new System.Drawing.Point(7, 368);
            this.button_loadFromSingleFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_loadFromSingleFile.Name = "button_loadFromSingleFile";
            this.button_loadFromSingleFile.Size = new System.Drawing.Size(664, 38);
            this.button_loadFromSingleFile.TabIndex = 3;
            this.button_loadFromSingleFile.Text = "装载日志记录(&L) >>";
            this.button_loadFromSingleFile.UseVisualStyleBackColor = true;
            this.button_loadFromSingleFile.Click += new System.EventHandler(this.button_loadFromSingleFile_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_selectFile);
            this.tabControl_main.Controls.Add(this.tabPage_selectFiles);
            this.tabControl_main.Controls.Add(this.tabPage_logRecords);
            this.tabControl_main.Controls.Add(this.tabPage_repair);
            this.tabControl_main.Controls.Add(this.tabPage_file);
            this.tabControl_main.Controls.Add(this.tabPage_restore);
            this.tabControl_main.Location = new System.Drawing.Point(17, 18);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(689, 453);
            this.tabControl_main.TabIndex = 4;
            // 
            // tabPage_selectFile
            // 
            this.tabPage_selectFile.Controls.Add(this.label7);
            this.tabPage_selectFile.Controls.Add(this.textBox_filter);
            this.tabPage_selectFile.Controls.Add(this.button_getTodayFilename);
            this.tabPage_selectFile.Controls.Add(this.button_getSingleLogFilename);
            this.tabPage_selectFile.Controls.Add(this.label1);
            this.tabPage_selectFile.Controls.Add(this.button_loadFromSingleFile);
            this.tabPage_selectFile.Controls.Add(this.textBox_logFileName);
            this.tabPage_selectFile.Location = new System.Drawing.Point(4, 31);
            this.tabPage_selectFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_selectFile.Name = "tabPage_selectFile";
            this.tabPage_selectFile.Size = new System.Drawing.Size(681, 418);
            this.tabPage_selectFile.TabIndex = 2;
            this.tabPage_selectFile.Text = "从单个日志文件装载";
            this.tabPage_selectFile.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 66);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(138, 21);
            this.label7.TabIndex = 5;
            this.label7.Text = "日志类型(&F):";
            this.label7.Visible = false;
            // 
            // textBox_filter
            // 
            this.textBox_filter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_filter.Location = new System.Drawing.Point(169, 61);
            this.textBox_filter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_filter.Name = "textBox_filter";
            this.textBox_filter.Size = new System.Drawing.Size(332, 31);
            this.textBox_filter.TabIndex = 6;
            this.textBox_filter.Visible = false;
            // 
            // button_getTodayFilename
            // 
            this.button_getTodayFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getTodayFilename.Location = new System.Drawing.Point(579, 18);
            this.button_getTodayFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_getTodayFilename.Name = "button_getTodayFilename";
            this.button_getTodayFilename.Size = new System.Drawing.Size(92, 38);
            this.button_getTodayFilename.TabIndex = 4;
            this.button_getTodayFilename.Text = "今天";
            this.button_getTodayFilename.UseVisualStyleBackColor = true;
            this.button_getTodayFilename.Click += new System.EventHandler(this.button_getTodayFilename_Click);
            // 
            // button_getSingleLogFilename
            // 
            this.button_getSingleLogFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getSingleLogFilename.Location = new System.Drawing.Point(512, 18);
            this.button_getSingleLogFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_getSingleLogFilename.Name = "button_getSingleLogFilename";
            this.button_getSingleLogFilename.Size = new System.Drawing.Size(61, 38);
            this.button_getSingleLogFilename.TabIndex = 2;
            this.button_getSingleLogFilename.Text = "...";
            this.button_getSingleLogFilename.UseVisualStyleBackColor = true;
            this.button_getSingleLogFilename.Click += new System.EventHandler(this.button_getSingleLogFilename_Click);
            // 
            // tabPage_selectFiles
            // 
            this.tabPage_selectFiles.Controls.Add(this.comboBox_quickSetFilenames);
            this.tabPage_selectFiles.Controls.Add(this.label6);
            this.tabPage_selectFiles.Controls.Add(this.label5);
            this.tabPage_selectFiles.Controls.Add(this.button_loadFilenams);
            this.tabPage_selectFiles.Controls.Add(this.button_loadLogRecords);
            this.tabPage_selectFiles.Controls.Add(this.textBox_filenames);
            this.tabPage_selectFiles.Location = new System.Drawing.Point(4, 31);
            this.tabPage_selectFiles.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_selectFiles.Name = "tabPage_selectFiles";
            this.tabPage_selectFiles.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_selectFiles.Size = new System.Drawing.Size(681, 418);
            this.tabPage_selectFiles.TabIndex = 0;
            this.tabPage_selectFiles.Text = "从多个日志文件装载";
            this.tabPage_selectFiles.UseVisualStyleBackColor = true;
            // 
            // comboBox_quickSetFilenames
            // 
            this.comboBox_quickSetFilenames.FormattingEnabled = true;
            this.comboBox_quickSetFilenames.Items.AddRange(new object[] {
            "本周",
            "本月",
            "本年",
            "最近 7 天",
            "最近 30 天",
            "最近 31 天",
            "最近 365 天",
            "最近 2 年",
            "最近 3 年",
            "最近 4 年",
            "最近 5 年",
            "最近 6 年",
            "最近 7 年",
            "最近 8 年",
            "最近 9 年",
            "最近 10 年"});
            this.comboBox_quickSetFilenames.Location = new System.Drawing.Point(163, 52);
            this.comboBox_quickSetFilenames.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_quickSetFilenames.Name = "comboBox_quickSetFilenames";
            this.comboBox_quickSetFilenames.Size = new System.Drawing.Size(264, 29);
            this.comboBox_quickSetFilenames.TabIndex = 5;
            this.comboBox_quickSetFilenames.SelectedIndexChanged += new System.EventHandler(this.comboBox_quickSetFilenames_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 58);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(138, 21);
            this.label6.TabIndex = 4;
            this.label6.Text = "快速获得(&Q):";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 102);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(287, 21);
            this.label5.TabIndex = 3;
            this.label5.Text = "日志文件名 [每行一个] (&F):";
            // 
            // button_loadFilenams
            // 
            this.button_loadFilenams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadFilenams.Location = new System.Drawing.Point(7, 9);
            this.button_loadFilenams.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_loadFilenams.Name = "button_loadFilenams";
            this.button_loadFilenams.Size = new System.Drawing.Size(664, 38);
            this.button_loadFilenams.TabIndex = 2;
            this.button_loadFilenams.Text = "获得日志文件名...";
            this.button_loadFilenams.UseVisualStyleBackColor = true;
            this.button_loadFilenams.Click += new System.EventHandler(this.button_loadFilenams_Click);
            // 
            // button_loadLogRecords
            // 
            this.button_loadLogRecords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadLogRecords.Enabled = false;
            this.button_loadLogRecords.Location = new System.Drawing.Point(7, 368);
            this.button_loadLogRecords.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_loadLogRecords.Name = "button_loadLogRecords";
            this.button_loadLogRecords.Size = new System.Drawing.Size(664, 38);
            this.button_loadLogRecords.TabIndex = 1;
            this.button_loadLogRecords.Text = "装载日志记录(&L) >>";
            this.button_loadLogRecords.UseVisualStyleBackColor = true;
            this.button_loadLogRecords.Click += new System.EventHandler(this.button_loadLogRecords_Click);
            // 
            // textBox_filenames
            // 
            this.textBox_filenames.AcceptsReturn = true;
            this.textBox_filenames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_filenames.HideSelection = false;
            this.textBox_filenames.Location = new System.Drawing.Point(7, 126);
            this.textBox_filenames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_filenames.MaxLength = 0;
            this.textBox_filenames.Multiline = true;
            this.textBox_filenames.Name = "textBox_filenames";
            this.textBox_filenames.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_filenames.Size = new System.Drawing.Size(660, 230);
            this.textBox_filenames.TabIndex = 0;
            this.textBox_filenames.TextChanged += new System.EventHandler(this.textBox_filenames_TextChanged);
            // 
            // tabPage_logRecords
            // 
            this.tabPage_logRecords.Controls.Add(this.splitContainer_logRecords);
            this.tabPage_logRecords.Location = new System.Drawing.Point(4, 31);
            this.tabPage_logRecords.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_logRecords.Name = "tabPage_logRecords";
            this.tabPage_logRecords.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_logRecords.Size = new System.Drawing.Size(681, 418);
            this.tabPage_logRecords.TabIndex = 1;
            this.tabPage_logRecords.Text = "日志记录";
            this.tabPage_logRecords.UseVisualStyleBackColor = true;
            // 
            // tabPage_repair
            // 
            this.tabPage_repair.Controls.Add(this.groupBox1);
            this.tabPage_repair.Controls.Add(this.button_repair_verify);
            this.tabPage_repair.Controls.Add(this.button_repair_findVerifyFolderName);
            this.tabPage_repair.Controls.Add(this.label4);
            this.tabPage_repair.Controls.Add(this.textBox_repair_verifyFolderName);
            this.tabPage_repair.Controls.Add(this.button_repair_repair);
            this.tabPage_repair.Controls.Add(this.button_repair_findTargetFilename);
            this.tabPage_repair.Controls.Add(this.label3);
            this.tabPage_repair.Controls.Add(this.textBox_repair_targetFilename);
            this.tabPage_repair.Controls.Add(this.button_repair_findSourceFilename);
            this.tabPage_repair.Controls.Add(this.label2);
            this.tabPage_repair.Controls.Add(this.textBox_repair_sourceFilename);
            this.tabPage_repair.Location = new System.Drawing.Point(4, 31);
            this.tabPage_repair.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_repair.Name = "tabPage_repair";
            this.tabPage_repair.Size = new System.Drawing.Size(681, 418);
            this.tabPage_repair.TabIndex = 3;
            this.tabPage_repair.Text = "修复";
            this.tabPage_repair.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(15, 215);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(651, 2);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            // 
            // button_repair_verify
            // 
            this.button_repair_verify.Location = new System.Drawing.Point(260, 315);
            this.button_repair_verify.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_repair_verify.Name = "button_repair_verify";
            this.button_repair_verify.Size = new System.Drawing.Size(194, 38);
            this.button_repair_verify.TabIndex = 13;
            this.button_repair_verify.Text = "验证(&V)";
            this.button_repair_verify.UseVisualStyleBackColor = true;
            this.button_repair_verify.Click += new System.EventHandler(this.button_repair_verify_Click);
            // 
            // button_repair_findVerifyFolderName
            // 
            this.button_repair_findVerifyFolderName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_repair_findVerifyFolderName.Location = new System.Drawing.Point(603, 248);
            this.button_repair_findVerifyFolderName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_repair_findVerifyFolderName.Name = "button_repair_findVerifyFolderName";
            this.button_repair_findVerifyFolderName.Size = new System.Drawing.Size(61, 38);
            this.button_repair_findVerifyFolderName.TabIndex = 12;
            this.button_repair_findVerifyFolderName.Text = "...";
            this.button_repair_findVerifyFolderName.UseVisualStyleBackColor = true;
            this.button_repair_findVerifyFolderName.Click += new System.EventHandler(this.button_repir_findVerifyFolderName_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 254);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(201, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "日志文件目录名(&D):";
            // 
            // textBox_repair_verifyFolderName
            // 
            this.textBox_repair_verifyFolderName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_repair_verifyFolderName.Location = new System.Drawing.Point(260, 248);
            this.textBox_repair_verifyFolderName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_repair_verifyFolderName.Name = "textBox_repair_verifyFolderName";
            this.textBox_repair_verifyFolderName.Size = new System.Drawing.Size(332, 31);
            this.textBox_repair_verifyFolderName.TabIndex = 11;
            // 
            // button_repair_repair
            // 
            this.button_repair_repair.Location = new System.Drawing.Point(260, 147);
            this.button_repair_repair.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_repair_repair.Name = "button_repair_repair";
            this.button_repair_repair.Size = new System.Drawing.Size(194, 38);
            this.button_repair_repair.TabIndex = 9;
            this.button_repair_repair.Text = "修复(&R)";
            this.button_repair_repair.UseVisualStyleBackColor = true;
            this.button_repair_repair.Click += new System.EventHandler(this.button_repair_repair_Click);
            // 
            // button_repair_findTargetFilename
            // 
            this.button_repair_findTargetFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_repair_findTargetFilename.Location = new System.Drawing.Point(603, 66);
            this.button_repair_findTargetFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_repair_findTargetFilename.Name = "button_repair_findTargetFilename";
            this.button_repair_findTargetFilename.Size = new System.Drawing.Size(61, 38);
            this.button_repair_findTargetFilename.TabIndex = 8;
            this.button_repair_findTargetFilename.Text = "...";
            this.button_repair_findTargetFilename.UseVisualStyleBackColor = true;
            this.button_repair_findTargetFilename.Click += new System.EventHandler(this.button_repair_findTargetFilename_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 70);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(201, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "目标日志文件名(&F):";
            // 
            // textBox_repair_targetFilename
            // 
            this.textBox_repair_targetFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_repair_targetFilename.Location = new System.Drawing.Point(260, 66);
            this.textBox_repair_targetFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_repair_targetFilename.Name = "textBox_repair_targetFilename";
            this.textBox_repair_targetFilename.Size = new System.Drawing.Size(332, 31);
            this.textBox_repair_targetFilename.TabIndex = 7;
            // 
            // button_repair_findSourceFilename
            // 
            this.button_repair_findSourceFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_repair_findSourceFilename.Location = new System.Drawing.Point(603, 23);
            this.button_repair_findSourceFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_repair_findSourceFilename.Name = "button_repair_findSourceFilename";
            this.button_repair_findSourceFilename.Size = new System.Drawing.Size(61, 38);
            this.button_repair_findSourceFilename.TabIndex = 5;
            this.button_repair_findSourceFilename.Text = "...";
            this.button_repair_findSourceFilename.UseVisualStyleBackColor = true;
            this.button_repair_findSourceFilename.Click += new System.EventHandler(this.button_repair_findSourceFilename_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 26);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "源日志文件名(&F):";
            // 
            // textBox_repair_sourceFilename
            // 
            this.textBox_repair_sourceFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_repair_sourceFilename.Location = new System.Drawing.Point(260, 23);
            this.textBox_repair_sourceFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_repair_sourceFilename.Name = "textBox_repair_sourceFilename";
            this.textBox_repair_sourceFilename.Size = new System.Drawing.Size(332, 31);
            this.textBox_repair_sourceFilename.TabIndex = 4;
            // 
            // tabPage_file
            // 
            this.tabPage_file.Controls.Add(this.listView_file);
            this.tabPage_file.Controls.Add(this.toolStrip_file);
            this.tabPage_file.Location = new System.Drawing.Point(4, 31);
            this.tabPage_file.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_file.Name = "tabPage_file";
            this.tabPage_file.Size = new System.Drawing.Size(681, 418);
            this.tabPage_file.TabIndex = 4;
            this.tabPage_file.Text = "物理文件";
            this.tabPage_file.UseVisualStyleBackColor = true;
            // 
            // listView_file
            // 
            this.listView_file.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_file_index,
            this.columnHeader_file_size,
            this.columnHeader_file_operation,
            this.columnHeader_file_operator});
            this.listView_file.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_file.FullRowSelect = true;
            this.listView_file.HideSelection = false;
            this.listView_file.Location = new System.Drawing.Point(0, 0);
            this.listView_file.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.listView_file.Name = "listView_file";
            this.listView_file.Size = new System.Drawing.Size(681, 380);
            this.listView_file.TabIndex = 1;
            this.listView_file.UseCompatibleStateImageBehavior = false;
            this.listView_file.View = System.Windows.Forms.View.Details;
            this.listView_file.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_file_ColumnClick);
            // 
            // columnHeader_file_index
            // 
            this.columnHeader_file_index.Text = "序号";
            // 
            // columnHeader_file_size
            // 
            this.columnHeader_file_size.Text = "尺寸";
            this.columnHeader_file_size.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_file_size.Width = 100;
            // 
            // columnHeader_file_operation
            // 
            this.columnHeader_file_operation.Text = "操作类型";
            this.columnHeader_file_operation.Width = 100;
            // 
            // columnHeader_file_operator
            // 
            this.columnHeader_file_operator.Text = "操作者";
            this.columnHeader_file_operator.Width = 100;
            // 
            // toolStrip_file
            // 
            this.toolStrip_file.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_file.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip_file.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_file_load});
            this.toolStrip_file.Location = new System.Drawing.Point(0, 380);
            this.toolStrip_file.Name = "toolStrip_file";
            this.toolStrip_file.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip_file.Size = new System.Drawing.Size(681, 38);
            this.toolStrip_file.TabIndex = 0;
            this.toolStrip_file.Text = "toolStrip1";
            // 
            // toolStripButton_file_load
            // 
            this.toolStripButton_file_load.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_file_load.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_file_load.Image")));
            this.toolStripButton_file_load.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_file_load.Name = "toolStripButton_file_load";
            this.toolStripButton_file_load.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_file_load.Text = "装载";
            this.toolStripButton_file_load.Click += new System.EventHandler(this.toolStripButton_file_load_Click);
            // 
            // tabPage_restore
            // 
            this.tabPage_restore.Controls.Add(this.splitContainer_recover);
            this.tabPage_restore.Location = new System.Drawing.Point(4, 31);
            this.tabPage_restore.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_restore.Name = "tabPage_restore";
            this.tabPage_restore.Size = new System.Drawing.Size(681, 418);
            this.tabPage_restore.TabIndex = 5;
            this.tabPage_restore.Text = "恢复记录";
            this.tabPage_restore.UseVisualStyleBackColor = true;
            // 
            // splitContainer_recover
            // 
            this.splitContainer_recover.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_recover.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_recover.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.splitContainer_recover.Name = "splitContainer_recover";
            this.splitContainer_recover.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_recover.Panel1
            // 
            this.splitContainer_recover.Panel1.Controls.Add(this.listView_restoreList);
            // 
            // splitContainer_recover.Panel2
            // 
            this.splitContainer_recover.Panel2.Controls.Add(this.listView_recover_history);
            this.splitContainer_recover.Size = new System.Drawing.Size(681, 418);
            this.splitContainer_recover.SplitterDistance = 206;
            this.splitContainer_recover.SplitterWidth = 14;
            this.splitContainer_recover.TabIndex = 3;
            // 
            // listView_restoreList
            // 
            this.listView_restoreList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_restoreList_no,
            this.columnHeader_restoreList_recPath,
            this.columnHeader_restoreList_summary,
            this.columnHeader_restoreList_history});
            this.listView_restoreList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_restoreList.FullRowSelect = true;
            this.listView_restoreList.HideSelection = false;
            this.listView_restoreList.Location = new System.Drawing.Point(0, 0);
            this.listView_restoreList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.listView_restoreList.Name = "listView_restoreList";
            this.listView_restoreList.Size = new System.Drawing.Size(681, 206);
            this.listView_restoreList.TabIndex = 2;
            this.listView_restoreList.UseCompatibleStateImageBehavior = false;
            this.listView_restoreList.View = System.Windows.Forms.View.Details;
            this.listView_restoreList.SelectedIndexChanged += new System.EventHandler(this.listView_restoreList_SelectedIndexChanged);
            this.listView_restoreList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_restoreList_MouseUp);
            // 
            // columnHeader_restoreList_no
            // 
            this.columnHeader_restoreList_no.Text = "序号";
            // 
            // columnHeader_restoreList_recPath
            // 
            this.columnHeader_restoreList_recPath.Text = "记录路径";
            this.columnHeader_restoreList_recPath.Width = 100;
            // 
            // columnHeader_restoreList_summary
            // 
            this.columnHeader_restoreList_summary.Text = "摘要";
            this.columnHeader_restoreList_summary.Width = 100;
            // 
            // columnHeader_restoreList_history
            // 
            this.columnHeader_restoreList_history.Text = "操作历史";
            this.columnHeader_restoreList_history.Width = 100;
            // 
            // listView_recover_history
            // 
            this.listView_recover_history.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listView_recover_history.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_recover_history.FullRowSelect = true;
            this.listView_recover_history.HideSelection = false;
            this.listView_recover_history.Location = new System.Drawing.Point(0, 0);
            this.listView_recover_history.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listView_recover_history.Name = "listView_recover_history";
            this.listView_recover_history.Size = new System.Drawing.Size(681, 198);
            this.listView_recover_history.TabIndex = 1;
            this.listView_recover_history.UseCompatibleStateImageBehavior = false;
            this.listView_recover_history.View = System.Windows.Forms.View.Details;
            this.listView_recover_history.SelectedIndexChanged += new System.EventHandler(this.listView_recover_history_SelectedIndexChanged);
            this.listView_recover_history.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_recover_history_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "文件名";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "序号";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "馆代码";
            this.columnHeader3.Width = 127;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "操作类型";
            this.columnHeader4.Width = 200;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "操作者";
            this.columnHeader5.Width = 200;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "操作时间";
            this.columnHeader6.Width = 200;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "耗时(秒)";
            this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "附件";
            this.columnHeader8.Width = 87;
            // 
            // OperLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 486);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "OperLogForm";
            this.ShowInTaskbar = false;
            this.Text = "日志窗";
            this.Activated += new System.EventHandler(this.OperLogForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OperLogForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OperLogForm_FormClosed);
            this.Load += new System.EventHandler(this.OperLogForm_Load);
            this.splitContainer_logRecords.Panel1.ResumeLayout(false);
            this.splitContainer_logRecords.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_logRecords)).EndInit();
            this.splitContainer_logRecords.ResumeLayout(false);
            this.panel_record.ResumeLayout(false);
            this.panel_record.PerformLayout();
            this.toolStrip_panelFixed.ResumeLayout(false);
            this.toolStrip_panelFixed.PerformLayout();
            this.tabControl_record.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.tabPage_html.ResumeLayout(false);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_selectFile.ResumeLayout(false);
            this.tabPage_selectFile.PerformLayout();
            this.tabPage_selectFiles.ResumeLayout(false);
            this.tabPage_selectFiles.PerformLayout();
            this.tabPage_logRecords.ResumeLayout(false);
            this.tabPage_repair.ResumeLayout(false);
            this.tabPage_repair.PerformLayout();
            this.tabPage_file.ResumeLayout(false);
            this.tabPage_file.PerformLayout();
            this.toolStrip_file.ResumeLayout(false);
            this.toolStrip_file.PerformLayout();
            this.tabPage_restore.ResumeLayout(false);
            this.splitContainer_recover.Panel1.ResumeLayout(false);
            this.splitContainer_recover.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_recover)).EndInit();
            this.splitContainer_recover.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_logFileName;
        private System.Windows.Forms.SplitContainer splitContainer_logRecords;
        private DigitalPlatform.GUI.ListViewQU listView_records;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_operType;
        private System.Windows.Forms.WebBrowser webBrowser_xml;
        private System.Windows.Forms.ColumnHeader columnHeader_attachment;
        private System.Windows.Forms.Button button_loadFromSingleFile;
        private System.Windows.Forms.ColumnHeader columnHeader_operator;
        private System.Windows.Forms.ColumnHeader columnHeader_operTime;
        private System.Windows.Forms.ColumnHeader columnHeader_filename;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_selectFiles;
        private System.Windows.Forms.TabPage tabPage_logRecords;
        private DigitalPlatform.GUI.NoHasSelTextBox textBox_filenames;
        private System.Windows.Forms.Button button_loadLogRecords;
        private System.Windows.Forms.Button button_loadFilenams;
        private System.Windows.Forms.TabPage tabPage_selectFile;
        private System.Windows.Forms.Button button_getSingleLogFilename;
        private System.Windows.Forms.TabPage tabPage_repair;
        private System.Windows.Forms.Button button_repair_findTargetFilename;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_repair_targetFilename;
        private System.Windows.Forms.Button button_repair_findSourceFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_repair_sourceFilename;
        private System.Windows.Forms.Button button_repair_repair;
        private System.Windows.Forms.Button button_repair_findVerifyFolderName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_repair_verifyFolderName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_repair_verify;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryCode;
        private System.Windows.Forms.Button button_getTodayFilename;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_quickSetFilenames;
        private System.Windows.Forms.TabControl tabControl_record;
        private System.Windows.Forms.TabPage tabPage_xml;
        private System.Windows.Forms.TabPage tabPage_html;
        private System.Windows.Forms.WebBrowser webBrowser_html;
        private System.Windows.Forms.Panel panel_record;
        private System.Windows.Forms.ToolStrip toolStrip_panelFixed;
        private System.Windows.Forms.ToolStripButton toolStripButton_closeDownPanel;
        private System.Windows.Forms.ColumnHeader columnHeader_seconds;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_filter;
        private System.Windows.Forms.TabPage tabPage_file;
        private System.Windows.Forms.ListView listView_file;
        private System.Windows.Forms.ToolStrip toolStrip_file;
        private System.Windows.Forms.ColumnHeader columnHeader_file_index;
        private System.Windows.Forms.ColumnHeader columnHeader_file_size;
        private System.Windows.Forms.ColumnHeader columnHeader_file_operation;
        private System.Windows.Forms.ToolStripButton toolStripButton_file_load;
        private System.Windows.Forms.ColumnHeader columnHeader_file_operator;
        private System.Windows.Forms.TabPage tabPage_restore;
        private DigitalPlatform.GUI.ListViewQU listView_restoreList;
        private System.Windows.Forms.ColumnHeader columnHeader_restoreList_no;
        private System.Windows.Forms.ColumnHeader columnHeader_restoreList_recPath;
        private System.Windows.Forms.ColumnHeader columnHeader_restoreList_summary;
        private System.Windows.Forms.ColumnHeader columnHeader_restoreList_history;
        private System.Windows.Forms.SplitContainer splitContainer_recover;
        private DigitalPlatform.GUI.ListViewQU listView_recover_history;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
    }
}