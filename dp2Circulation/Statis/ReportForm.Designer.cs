namespace dp2Circulation
{
    partial class ReportForm
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

            if (this._counting != null)
                this._counting.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.button_start_uploadReport = new System.Windows.Forms.Button();
            this.button_start_dailyReport = new System.Windows.Forms.Button();
            this.checkBox_start_enableFirst = new System.Windows.Forms.CheckBox();
            this.button_start_dailyReplication = new System.Windows.Forms.Button();
            this.button_start_createLocalStorage = new System.Windows.Forms.Button();
            this.comboBox_start_uploadMethod = new System.Windows.Forms.ComboBox();
            this.tabPage_libraryConfig = new System.Windows.Forms.TabPage();
            this.listView_libraryConfig = new System.Windows.Forms.ListView();
            this.columnHeader_libraryConfig_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_libraryConfig = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip_libraryConfig = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_libraryConfig_save = new System.Windows.Forms.ToolStripButton();
            this.tabPage_query = new System.Windows.Forms.TabPage();
            this.toolStrip_query = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_query_do = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_query = new System.Windows.Forms.SplitContainer();
            this.textBox_query_command = new System.Windows.Forms.TextBox();
            this.listView_query_results = new DigitalPlatform.GUI.ListViewQU();
            this.tabPage_option = new System.Windows.Forms.TabPage();
            this.checkBox_option_deleteReportFileAfterUpload = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_printHtml = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_setReportStartDay = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_convertFormat = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_openReportFolder = new System.Windows.Forms.ToolStripButton();
            this.timer_qu = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            this.tabPage_libraryConfig.SuspendLayout();
            this.toolStrip_libraryConfig.SuspendLayout();
            this.tabPage_query.SuspendLayout();
            this.toolStrip_query.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_query)).BeginInit();
            this.splitContainer_query.Panel1.SuspendLayout();
            this.splitContainer_query.Panel2.SuspendLayout();
            this.splitContainer_query.SuspendLayout();
            this.tabPage_option.SuspendLayout();
            this.toolStrip_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_start);
            this.tabControl1.Controls.Add(this.tabPage_libraryConfig);
            this.tabControl1.Controls.Add(this.tabPage_query);
            this.tabControl1.Controls.Add(this.tabPage_option);
            this.tabControl1.Location = new System.Drawing.Point(0, 12);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(431, 288);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_start
            // 
            this.tabPage_start.AutoScroll = true;
            this.tabPage_start.Controls.Add(this.label1);
            this.tabPage_start.Controls.Add(this.button_start_uploadReport);
            this.tabPage_start.Controls.Add(this.button_start_dailyReport);
            this.tabPage_start.Controls.Add(this.checkBox_start_enableFirst);
            this.tabPage_start.Controls.Add(this.button_start_dailyReplication);
            this.tabPage_start.Controls.Add(this.button_start_createLocalStorage);
            this.tabPage_start.Controls.Add(this.comboBox_start_uploadMethod);
            this.tabPage_start.Location = new System.Drawing.Point(4, 22);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_start.Size = new System.Drawing.Size(423, 262);
            this.tabPage_start.TabIndex = 0;
            this.tabPage_start.Text = "开始";
            this.tabPage_start.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 173);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "上传方式:";
            // 
            // button_start_uploadReport
            // 
            this.button_start_uploadReport.Enabled = false;
            this.button_start_uploadReport.Location = new System.Drawing.Point(7, 188);
            this.button_start_uploadReport.Name = "button_start_uploadReport";
            this.button_start_uploadReport.Size = new System.Drawing.Size(284, 23);
            this.button_start_uploadReport.TabIndex = 4;
            this.button_start_uploadReport.Text = "上传报表";
            this.button_start_uploadReport.UseVisualStyleBackColor = true;
            this.button_start_uploadReport.Click += new System.EventHandler(this.button_start_uploadReport_Click);
            // 
            // button_start_dailyReport
            // 
            this.button_start_dailyReport.Location = new System.Drawing.Point(7, 116);
            this.button_start_dailyReport.Name = "button_start_dailyReport";
            this.button_start_dailyReport.Size = new System.Drawing.Size(284, 23);
            this.button_start_dailyReport.TabIndex = 3;
            this.button_start_dailyReport.Text = "每日报表";
            this.button_start_dailyReport.UseVisualStyleBackColor = true;
            this.button_start_dailyReport.Click += new System.EventHandler(this.button_start_dailyReport_Click);
            // 
            // checkBox_start_enableFirst
            // 
            this.checkBox_start_enableFirst.AutoSize = true;
            this.checkBox_start_enableFirst.Location = new System.Drawing.Point(297, 34);
            this.checkBox_start_enableFirst.Name = "checkBox_start_enableFirst";
            this.checkBox_start_enableFirst.Size = new System.Drawing.Size(96, 16);
            this.checkBox_start_enableFirst.TabIndex = 2;
            this.checkBox_start_enableFirst.Text = "从头再次执行";
            this.checkBox_start_enableFirst.UseVisualStyleBackColor = true;
            this.checkBox_start_enableFirst.CheckedChanged += new System.EventHandler(this.checkBox_start_enableFirst_CheckedChanged);
            // 
            // button_start_dailyReplication
            // 
            this.button_start_dailyReplication.Location = new System.Drawing.Point(7, 59);
            this.button_start_dailyReplication.Name = "button_start_dailyReplication";
            this.button_start_dailyReplication.Size = new System.Drawing.Size(284, 23);
            this.button_start_dailyReplication.TabIndex = 1;
            this.button_start_dailyReplication.Text = "每日同步";
            this.button_start_dailyReplication.UseVisualStyleBackColor = true;
            this.button_start_dailyReplication.Click += new System.EventHandler(this.button_start_dailyReplication_Click);
            // 
            // button_start_createLocalStorage
            // 
            this.button_start_createLocalStorage.Location = new System.Drawing.Point(7, 30);
            this.button_start_createLocalStorage.Name = "button_start_createLocalStorage";
            this.button_start_createLocalStorage.Size = new System.Drawing.Size(284, 23);
            this.button_start_createLocalStorage.TabIndex = 0;
            this.button_start_createLocalStorage.Text = "首次创建本地存储";
            this.button_start_createLocalStorage.UseVisualStyleBackColor = true;
            this.button_start_createLocalStorage.Click += new System.EventHandler(this.button_start_createLocalStorage_Click);
            // 
            // comboBox_start_uploadMethod
            // 
            this.comboBox_start_uploadMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_start_uploadMethod.FormattingEnabled = true;
            this.comboBox_start_uploadMethod.Items.AddRange(new object[] {
            "dp2Library",
            "FTP"});
            this.comboBox_start_uploadMethod.Location = new System.Drawing.Point(73, 169);
            this.comboBox_start_uploadMethod.Name = "comboBox_start_uploadMethod";
            this.comboBox_start_uploadMethod.Size = new System.Drawing.Size(218, 20);
            this.comboBox_start_uploadMethod.TabIndex = 5;
            // 
            // tabPage_libraryConfig
            // 
            this.tabPage_libraryConfig.Controls.Add(this.listView_libraryConfig);
            this.tabPage_libraryConfig.Controls.Add(this.toolStrip_libraryConfig);
            this.tabPage_libraryConfig.Location = new System.Drawing.Point(4, 22);
            this.tabPage_libraryConfig.Name = "tabPage_libraryConfig";
            this.tabPage_libraryConfig.Size = new System.Drawing.Size(423, 262);
            this.tabPage_libraryConfig.TabIndex = 2;
            this.tabPage_libraryConfig.Text = "报表配置";
            this.tabPage_libraryConfig.UseVisualStyleBackColor = true;
            // 
            // listView_libraryConfig
            // 
            this.listView_libraryConfig.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_libraryConfig_libraryCode,
            this.columnHeader_libraryConfig});
            this.listView_libraryConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_libraryConfig.FullRowSelect = true;
            this.listView_libraryConfig.HideSelection = false;
            this.listView_libraryConfig.Location = new System.Drawing.Point(0, 0);
            this.listView_libraryConfig.Name = "listView_libraryConfig";
            this.listView_libraryConfig.Size = new System.Drawing.Size(423, 262);
            this.listView_libraryConfig.TabIndex = 1;
            this.listView_libraryConfig.UseCompatibleStateImageBehavior = false;
            this.listView_libraryConfig.View = System.Windows.Forms.View.Details;
            this.listView_libraryConfig.DoubleClick += new System.EventHandler(this.listView_libraryConfig_DoubleClick);
            this.listView_libraryConfig.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_libraryConfig_MouseUp);
            // 
            // columnHeader_libraryConfig_libraryCode
            // 
            this.columnHeader_libraryConfig_libraryCode.Text = "馆代码";
            this.columnHeader_libraryConfig_libraryCode.Width = 100;
            // 
            // columnHeader_libraryConfig
            // 
            this.columnHeader_libraryConfig.Text = "摘要";
            this.columnHeader_libraryConfig.Width = 200;
            // 
            // toolStrip_libraryConfig
            // 
            this.toolStrip_libraryConfig.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_libraryConfig_save});
            this.toolStrip_libraryConfig.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_libraryConfig.Name = "toolStrip_libraryConfig";
            this.toolStrip_libraryConfig.Size = new System.Drawing.Size(399, 25);
            this.toolStrip_libraryConfig.TabIndex = 0;
            this.toolStrip_libraryConfig.Text = "toolStrip2";
            this.toolStrip_libraryConfig.Visible = false;
            // 
            // toolStripButton_libraryConfig_save
            // 
            this.toolStripButton_libraryConfig_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_libraryConfig_save.Enabled = false;
            this.toolStripButton_libraryConfig_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_libraryConfig_save.Image")));
            this.toolStripButton_libraryConfig_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_libraryConfig_save.Name = "toolStripButton_libraryConfig_save";
            this.toolStripButton_libraryConfig_save.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_libraryConfig_save.Text = "保存";
            this.toolStripButton_libraryConfig_save.Click += new System.EventHandler(this.toolStripButton_libraryConfig_save_Click);
            // 
            // tabPage_query
            // 
            this.tabPage_query.Controls.Add(this.toolStrip_query);
            this.tabPage_query.Controls.Add(this.splitContainer_query);
            this.tabPage_query.Location = new System.Drawing.Point(4, 22);
            this.tabPage_query.Name = "tabPage_query";
            this.tabPage_query.Size = new System.Drawing.Size(423, 262);
            this.tabPage_query.TabIndex = 3;
            this.tabPage_query.Text = "查询分析";
            this.tabPage_query.UseVisualStyleBackColor = true;
            // 
            // toolStrip_query
            // 
            this.toolStrip_query.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_query_do});
            this.toolStrip_query.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_query.Name = "toolStrip_query";
            this.toolStrip_query.Size = new System.Drawing.Size(423, 25);
            this.toolStrip_query.TabIndex = 1;
            this.toolStrip_query.Text = "toolStrip2";
            // 
            // toolStripButton_query_do
            // 
            this.toolStripButton_query_do.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_query_do.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_query_do.Image")));
            this.toolStripButton_query_do.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_query_do.Name = "toolStripButton_query_do";
            this.toolStripButton_query_do.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_query_do.Text = "执行";
            this.toolStripButton_query_do.Click += new System.EventHandler(this.toolStripButton_query_do_Click);
            // 
            // splitContainer_query
            // 
            this.splitContainer_query.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_query.Location = new System.Drawing.Point(0, 28);
            this.splitContainer_query.Name = "splitContainer_query";
            this.splitContainer_query.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_query.Panel1
            // 
            this.splitContainer_query.Panel1.Controls.Add(this.textBox_query_command);
            // 
            // splitContainer_query.Panel2
            // 
            this.splitContainer_query.Panel2.Controls.Add(this.listView_query_results);
            this.splitContainer_query.Size = new System.Drawing.Size(420, 234);
            this.splitContainer_query.SplitterDistance = 58;
            this.splitContainer_query.SplitterWidth = 8;
            this.splitContainer_query.TabIndex = 0;
            // 
            // textBox_query_command
            // 
            this.textBox_query_command.AcceptsReturn = true;
            this.textBox_query_command.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_query_command.HideSelection = false;
            this.textBox_query_command.Location = new System.Drawing.Point(0, 0);
            this.textBox_query_command.Multiline = true;
            this.textBox_query_command.Name = "textBox_query_command";
            this.textBox_query_command.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_query_command.Size = new System.Drawing.Size(420, 58);
            this.textBox_query_command.TabIndex = 0;
            // 
            // listView_query_results
            // 
            this.listView_query_results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_query_results.FullRowSelect = true;
            this.listView_query_results.HideSelection = false;
            this.listView_query_results.Location = new System.Drawing.Point(0, 0);
            this.listView_query_results.Name = "listView_query_results";
            this.listView_query_results.Size = new System.Drawing.Size(420, 168);
            this.listView_query_results.TabIndex = 0;
            this.listView_query_results.UseCompatibleStateImageBehavior = false;
            this.listView_query_results.View = System.Windows.Forms.View.Details;
            this.listView_query_results.SelectedIndexChanged += new System.EventHandler(this.listView_query_results_SelectedIndexChanged);
            this.listView_query_results.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_query_results_MouseUp);
            // 
            // tabPage_option
            // 
            this.tabPage_option.Controls.Add(this.checkBox_option_deleteReportFileAfterUpload);
            this.tabPage_option.Location = new System.Drawing.Point(4, 22);
            this.tabPage_option.Name = "tabPage_option";
            this.tabPage_option.Size = new System.Drawing.Size(423, 262);
            this.tabPage_option.TabIndex = 4;
            this.tabPage_option.Text = "参数";
            this.tabPage_option.UseVisualStyleBackColor = true;
            this.tabPage_option.Enter += new System.EventHandler(this.tabPage_option_Enter);
            this.tabPage_option.Leave += new System.EventHandler(this.tabPage_option_Leave);
            // 
            // checkBox_option_deleteReportFileAfterUpload
            // 
            this.checkBox_option_deleteReportFileAfterUpload.AutoSize = true;
            this.checkBox_option_deleteReportFileAfterUpload.Location = new System.Drawing.Point(9, 20);
            this.checkBox_option_deleteReportFileAfterUpload.Name = "checkBox_option_deleteReportFileAfterUpload";
            this.checkBox_option_deleteReportFileAfterUpload.Size = new System.Drawing.Size(222, 16);
            this.checkBox_option_deleteReportFileAfterUpload.TabIndex = 0;
            this.checkBox_option_deleteReportFileAfterUpload.Text = "上载成功后自动删除本地报表文件(&D)";
            this.checkBox_option_deleteReportFileAfterUpload.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(365, 214);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "管理";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_printHtml,
            this.toolStripButton_setReportStartDay,
            this.toolStripButton_convertFormat,
            this.toolStripButton_openReportFolder});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 303);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Size = new System.Drawing.Size(431, 25);
            this.toolStrip_main.TabIndex = 1;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolStripButton_printHtml
            // 
            this.toolStripButton_printHtml.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_printHtml.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_printHtml.Image")));
            this.toolStripButton_printHtml.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_printHtml.Name = "toolStripButton_printHtml";
            this.toolStripButton_printHtml.Size = new System.Drawing.Size(74, 22);
            this.toolStripButton_printHtml.Text = "打印 HTML";
            this.toolStripButton_printHtml.Visible = false;
            this.toolStripButton_printHtml.Click += new System.EventHandler(this.toolStripButton_printHtml_Click);
            // 
            // toolStripButton_setReportStartDay
            // 
            this.toolStripButton_setReportStartDay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_setReportStartDay.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_setReportStartDay.Image")));
            this.toolStripButton_setReportStartDay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_setReportStartDay.Name = "toolStripButton_setReportStartDay";
            this.toolStripButton_setReportStartDay.Size = new System.Drawing.Size(132, 22);
            this.toolStripButton_setReportStartDay.Text = "设置报表创建起始日期";
            this.toolStripButton_setReportStartDay.Click += new System.EventHandler(this.toolStripButton_setReportStartDay_Click);
            // 
            // toolStripButton_convertFormat
            // 
            this.toolStripButton_convertFormat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_convertFormat.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_convertFormat.Image")));
            this.toolStripButton_convertFormat.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_convertFormat.Name = "toolStripButton_convertFormat";
            this.toolStripButton_convertFormat.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_convertFormat.Text = "格式转换";
            this.toolStripButton_convertFormat.ToolTipText = "把 .rml 格式转换为其它格式";
            this.toolStripButton_convertFormat.Click += new System.EventHandler(this.toolStripButton_convertFormat_Click);
            // 
            // toolStripButton_openReportFolder
            // 
            this.toolStripButton_openReportFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_openReportFolder.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_openReportFolder.Image")));
            this.toolStripButton_openReportFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_openReportFolder.Name = "toolStripButton_openReportFolder";
            this.toolStripButton_openReportFolder.Size = new System.Drawing.Size(96, 22);
            this.toolStripButton_openReportFolder.Text = "打开报表文件夹";
            this.toolStripButton_openReportFolder.Click += new System.EventHandler(this.toolStripButton_openReportFolder_Click);
            // 
            // timer_qu
            // 
            this.timer_qu.Interval = 1000;
            this.timer_qu.Tick += new System.EventHandler(this.timer_qu_Tick);
            // 
            // ReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 328);
            this.Controls.Add(this.toolStrip_main);
            this.Controls.Add(this.tabControl1);
            this.Name = "ReportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "报表";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReportForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReportForm_FormClosed);
            this.Load += new System.EventHandler(this.ReportForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.tabPage_start.PerformLayout();
            this.tabPage_libraryConfig.ResumeLayout(false);
            this.tabPage_libraryConfig.PerformLayout();
            this.toolStrip_libraryConfig.ResumeLayout(false);
            this.toolStrip_libraryConfig.PerformLayout();
            this.tabPage_query.ResumeLayout(false);
            this.tabPage_query.PerformLayout();
            this.toolStrip_query.ResumeLayout(false);
            this.toolStrip_query.PerformLayout();
            this.splitContainer_query.Panel1.ResumeLayout(false);
            this.splitContainer_query.Panel1.PerformLayout();
            this.splitContainer_query.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_query)).EndInit();
            this.splitContainer_query.ResumeLayout(false);
            this.tabPage_option.ResumeLayout(false);
            this.tabPage_option.PerformLayout();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripButton toolStripButton_printHtml;
        private System.Windows.Forms.TabPage tabPage_libraryConfig;
        private System.Windows.Forms.ToolStrip toolStrip_libraryConfig;
        private System.Windows.Forms.ListView listView_libraryConfig;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryConfig_libraryCode;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryConfig;
        private System.Windows.Forms.ToolStripButton toolStripButton_libraryConfig_save;
        private System.Windows.Forms.Button button_start_createLocalStorage;
        private System.Windows.Forms.Button button_start_dailyReplication;
        private System.Windows.Forms.TabPage tabPage_query;
        private System.Windows.Forms.SplitContainer splitContainer_query;
        private System.Windows.Forms.TextBox textBox_query_command;
        private DigitalPlatform.GUI.ListViewQU listView_query_results;
        private System.Windows.Forms.ToolStrip toolStrip_query;
        private System.Windows.Forms.ToolStripButton toolStripButton_query_do;
        private System.Windows.Forms.CheckBox checkBox_start_enableFirst;
        private System.Windows.Forms.Button button_start_dailyReport;
        private System.Windows.Forms.ToolStripButton toolStripButton_setReportStartDay;
        private System.Windows.Forms.Button button_start_uploadReport;
        private System.Windows.Forms.Timer timer_qu;
        private System.Windows.Forms.ComboBox comboBox_start_uploadMethod;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton toolStripButton_convertFormat;
        private System.Windows.Forms.TabPage tabPage_option;
        private System.Windows.Forms.CheckBox checkBox_option_deleteReportFileAfterUpload;
        private System.Windows.Forms.ToolStripButton toolStripButton_openReportFolder;
    }
}