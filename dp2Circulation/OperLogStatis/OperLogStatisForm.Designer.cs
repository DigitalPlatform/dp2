namespace dp2Circulation
{
    partial class OperLogStatisForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperLogStatisForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_selectProject = new System.Windows.Forms.TabPage();
            this.button_option = new System.Windows.Forms.Button();
            this.comboBox_projectName = new System.Windows.Forms.ComboBox();
            this.button_defaultProject_1 = new System.Windows.Forms.Button();
            this.button_getProjectName = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_timeRange = new System.Windows.Forms.TabPage();
            this.comboBox_quickSetFilenames = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dateControl_end = new DigitalPlatform.CommonControl.DateControl();
            this.dateControl_start = new DigitalPlatform.CommonControl.DateControl();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox_tableStyle = new System.Windows.Forms.GroupBox();
            this.checkBox_perDayTable = new System.Windows.Forms.CheckBox();
            this.checkBox_startToEndTable = new System.Windows.Forms.CheckBox();
            this.checkBox_perMonthTable = new System.Windows.Forms.CheckBox();
            this.checkBox_perYearTable = new System.Windows.Forms.CheckBox();
            this.tabPage_runStatis = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar_files = new System.Windows.Forms.ProgressBar();
            this.webBrowser1_running = new System.Windows.Forms.WebBrowser();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print = new System.Windows.Forms.Button();
            this.tabPage_management = new System.Windows.Forms.TabPage();
            this.button_next = new System.Windows.Forms.Button();
            this.button_projectManage = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_selectProject.SuspendLayout();
            this.tabPage_timeRange.SuspendLayout();
            this.groupBox_tableStyle.SuspendLayout();
            this.tabPage_runStatis.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_selectProject);
            this.tabControl_main.Controls.Add(this.tabPage_timeRange);
            this.tabControl_main.Controls.Add(this.tabPage_runStatis);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Controls.Add(this.tabPage_management);
            this.tabControl_main.Location = new System.Drawing.Point(0, 18);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(707, 378);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_selectProject
            // 
            this.tabPage_selectProject.Controls.Add(this.button_option);
            this.tabPage_selectProject.Controls.Add(this.comboBox_projectName);
            this.tabPage_selectProject.Controls.Add(this.button_defaultProject_1);
            this.tabPage_selectProject.Controls.Add(this.button_getProjectName);
            this.tabPage_selectProject.Controls.Add(this.label3);
            this.tabPage_selectProject.Location = new System.Drawing.Point(4, 31);
            this.tabPage_selectProject.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_selectProject.Name = "tabPage_selectProject";
            this.tabPage_selectProject.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_selectProject.Size = new System.Drawing.Size(699, 343);
            this.tabPage_selectProject.TabIndex = 1;
            this.tabPage_selectProject.Text = " 选定方案 ";
            this.tabPage_selectProject.UseVisualStyleBackColor = true;
            // 
            // button_option
            // 
            this.button_option.Location = new System.Drawing.Point(149, 53);
            this.button_option.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_option.Name = "button_option";
            this.button_option.Size = new System.Drawing.Size(183, 40);
            this.button_option.TabIndex = 4;
            this.button_option.Text = "选项 ...";
            this.button_option.UseVisualStyleBackColor = true;
            this.button_option.Click += new System.EventHandler(this.button_option_Click);
            // 
            // comboBox_projectName
            // 
            this.comboBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_projectName.FormattingEnabled = true;
            this.comboBox_projectName.Items.AddRange(new object[] {
            "#典藏移交清单",
            "#出纳流水"});
            this.comboBox_projectName.Location = new System.Drawing.Point(149, 16);
            this.comboBox_projectName.Name = "comboBox_projectName";
            this.comboBox_projectName.Size = new System.Drawing.Size(474, 29);
            this.comboBox_projectName.TabIndex = 1;
            // 
            // button_defaultProject_1
            // 
            this.button_defaultProject_1.Location = new System.Drawing.Point(149, 128);
            this.button_defaultProject_1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_defaultProject_1.Name = "button_defaultProject_1";
            this.button_defaultProject_1.Size = new System.Drawing.Size(183, 40);
            this.button_defaultProject_1.TabIndex = 3;
            this.button_defaultProject_1.Text = "内置方案 #1";
            this.button_defaultProject_1.UseVisualStyleBackColor = true;
            this.button_defaultProject_1.Visible = false;
            this.button_defaultProject_1.Click += new System.EventHandler(this.button_defaultProject_1_Click);
            // 
            // button_getProjectName
            // 
            this.button_getProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getProjectName.Location = new System.Drawing.Point(630, 16);
            this.button_getProjectName.Margin = new System.Windows.Forms.Padding(4);
            this.button_getProjectName.Name = "button_getProjectName";
            this.button_getProjectName.Size = new System.Drawing.Size(59, 38);
            this.button_getProjectName.TabIndex = 2;
            this.button_getProjectName.Text = "...";
            this.button_getProjectName.UseVisualStyleBackColor = true;
            this.button_getProjectName.Click += new System.EventHandler(this.button_getProjectName_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 19);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "方案名(&P):";
            // 
            // tabPage_timeRange
            // 
            this.tabPage_timeRange.AutoScroll = true;
            this.tabPage_timeRange.Controls.Add(this.comboBox_quickSetFilenames);
            this.tabPage_timeRange.Controls.Add(this.label6);
            this.tabPage_timeRange.Controls.Add(this.dateControl_end);
            this.tabPage_timeRange.Controls.Add(this.dateControl_start);
            this.tabPage_timeRange.Controls.Add(this.label2);
            this.tabPage_timeRange.Controls.Add(this.label1);
            this.tabPage_timeRange.Controls.Add(this.groupBox_tableStyle);
            this.tabPage_timeRange.Location = new System.Drawing.Point(4, 31);
            this.tabPage_timeRange.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_timeRange.Name = "tabPage_timeRange";
            this.tabPage_timeRange.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_timeRange.Size = new System.Drawing.Size(699, 343);
            this.tabPage_timeRange.TabIndex = 0;
            this.tabPage_timeRange.Text = " 设定时间范围 ";
            this.tabPage_timeRange.UseVisualStyleBackColor = true;
            // 
            // comboBox_quickSetFilenames
            // 
            this.comboBox_quickSetFilenames.FormattingEnabled = true;
            this.comboBox_quickSetFilenames.Items.AddRange(new object[] {
            "今天",
            "本周",
            "本月",
            "本年",
            "最近 7 天",
            "最近 30 天",
            "最近 31 天",
            "最近 365 天",
            "最近 10 年"});
            this.comboBox_quickSetFilenames.Location = new System.Drawing.Point(264, 124);
            this.comboBox_quickSetFilenames.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_quickSetFilenames.Name = "comboBox_quickSetFilenames";
            this.comboBox_quickSetFilenames.Size = new System.Drawing.Size(200, 29);
            this.comboBox_quickSetFilenames.TabIndex = 10;
            this.comboBox_quickSetFilenames.SelectedIndexChanged += new System.EventHandler(this.comboBox_quickSetFilenames_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 130);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(222, 21);
            this.label6.TabIndex = 9;
            this.label6.Text = "快速设定时间范围(&Q):";
            // 
            // dateControl_end
            // 
            this.dateControl_end.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_end.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_end.Location = new System.Drawing.Point(264, 74);
            this.dateControl_end.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_end.Name = "dateControl_end";
            this.dateControl_end.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_end.Size = new System.Drawing.Size(204, 32);
            this.dateControl_end.TabIndex = 3;
            this.dateControl_end.Value = new System.DateTime(((long)(0)));
            // 
            // dateControl_start
            // 
            this.dateControl_start.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_start.Location = new System.Drawing.Point(266, 24);
            this.dateControl_start.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_start.Name = "dateControl_start";
            this.dateControl_start.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_start.Size = new System.Drawing.Size(202, 32);
            this.dateControl_start.TabIndex = 2;
            this.dateControl_start.Value = new System.DateTime(((long)(0)));
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 74);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 21);
            this.label2.TabIndex = 1;
            this.label2.Text = "结束日(&E):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "起始日(&S):";
            // 
            // groupBox_tableStyle
            // 
            this.groupBox_tableStyle.Controls.Add(this.checkBox_perDayTable);
            this.groupBox_tableStyle.Controls.Add(this.checkBox_startToEndTable);
            this.groupBox_tableStyle.Controls.Add(this.checkBox_perMonthTable);
            this.groupBox_tableStyle.Controls.Add(this.checkBox_perYearTable);
            this.groupBox_tableStyle.Location = new System.Drawing.Point(18, 196);
            this.groupBox_tableStyle.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_tableStyle.Name = "groupBox_tableStyle";
            this.groupBox_tableStyle.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_tableStyle.Size = new System.Drawing.Size(411, 187);
            this.groupBox_tableStyle.TabIndex = 8;
            this.groupBox_tableStyle.TabStop = false;
            this.groupBox_tableStyle.Text = " 如何输出报表 ";
            this.groupBox_tableStyle.Visible = false;
            // 
            // checkBox_perDayTable
            // 
            this.checkBox_perDayTable.AutoSize = true;
            this.checkBox_perDayTable.Location = new System.Drawing.Point(147, 138);
            this.checkBox_perDayTable.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_perDayTable.Name = "checkBox_perDayTable";
            this.checkBox_perDayTable.Size = new System.Drawing.Size(132, 25);
            this.checkBox_perDayTable.TabIndex = 7;
            this.checkBox_perDayTable.Text = "每一日(&D)";
            this.checkBox_perDayTable.UseVisualStyleBackColor = true;
            // 
            // checkBox_startToEndTable
            // 
            this.checkBox_startToEndTable.AutoSize = true;
            this.checkBox_startToEndTable.Location = new System.Drawing.Point(147, 33);
            this.checkBox_startToEndTable.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_startToEndTable.Name = "checkBox_startToEndTable";
            this.checkBox_startToEndTable.Size = new System.Drawing.Size(153, 25);
            this.checkBox_startToEndTable.TabIndex = 4;
            this.checkBox_startToEndTable.Text = "起止范围(&A)";
            this.checkBox_startToEndTable.UseVisualStyleBackColor = true;
            // 
            // checkBox_perMonthTable
            // 
            this.checkBox_perMonthTable.AutoSize = true;
            this.checkBox_perMonthTable.Location = new System.Drawing.Point(147, 103);
            this.checkBox_perMonthTable.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_perMonthTable.Name = "checkBox_perMonthTable";
            this.checkBox_perMonthTable.Size = new System.Drawing.Size(132, 25);
            this.checkBox_perMonthTable.TabIndex = 6;
            this.checkBox_perMonthTable.Text = "每一月(&M)";
            this.checkBox_perMonthTable.UseVisualStyleBackColor = true;
            // 
            // checkBox_perYearTable
            // 
            this.checkBox_perYearTable.AutoSize = true;
            this.checkBox_perYearTable.Location = new System.Drawing.Point(147, 68);
            this.checkBox_perYearTable.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_perYearTable.Name = "checkBox_perYearTable";
            this.checkBox_perYearTable.Size = new System.Drawing.Size(132, 25);
            this.checkBox_perYearTable.TabIndex = 5;
            this.checkBox_perYearTable.Text = "每一年(&Y)";
            this.checkBox_perYearTable.UseVisualStyleBackColor = true;
            // 
            // tabPage_runStatis
            // 
            this.tabPage_runStatis.Controls.Add(this.tableLayoutPanel1);
            this.tabPage_runStatis.Location = new System.Drawing.Point(4, 31);
            this.tabPage_runStatis.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_runStatis.Name = "tabPage_runStatis";
            this.tabPage_runStatis.Size = new System.Drawing.Size(699, 343);
            this.tabPage_runStatis.TabIndex = 3;
            this.tabPage_runStatis.Text = " 执行运算 ";
            this.tabPage_runStatis.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.progressBar_files, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.webBrowser1_running, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.progressBar_records, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 14, 0, 7);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(699, 343);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // progressBar_files
            // 
            this.progressBar_files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar_files.Location = new System.Drawing.Point(4, 279);
            this.progressBar_files.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.progressBar_files.Name = "progressBar_files";
            this.progressBar_files.Size = new System.Drawing.Size(691, 18);
            this.progressBar_files.TabIndex = 2;
            this.progressBar_files.Visible = false;
            // 
            // webBrowser1_running
            // 
            this.webBrowser1_running.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1_running.Location = new System.Drawing.Point(4, 18);
            this.webBrowser1_running.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1_running.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser1_running.Name = "webBrowser1_running";
            this.webBrowser1_running.Size = new System.Drawing.Size(691, 250);
            this.webBrowser1_running.TabIndex = 0;
            // 
            // progressBar_records
            // 
            this.progressBar_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar_records.Location = new System.Drawing.Point(4, 311);
            this.progressBar_records.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.progressBar_records.Name = "progressBar_records";
            this.progressBar_records.Size = new System.Drawing.Size(691, 18);
            this.progressBar_records.TabIndex = 1;
            this.progressBar_records.Visible = false;
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 31);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(699, 343);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = " 打印结果 ";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print
            // 
            this.button_print.Location = new System.Drawing.Point(6, 24);
            this.button_print.Margin = new System.Windows.Forms.Padding(4);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(293, 38);
            this.button_print.TabIndex = 0;
            this.button_print.Text = "打印统计结果(&P)";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // tabPage_management
            // 
            this.tabPage_management.Location = new System.Drawing.Point(4, 31);
            this.tabPage_management.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_management.Name = "tabPage_management";
            this.tabPage_management.Size = new System.Drawing.Size(699, 343);
            this.tabPage_management.TabIndex = 4;
            this.tabPage_management.Text = "管理";
            this.tabPage_management.UseVisualStyleBackColor = true;
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(568, 402);
            this.button_next.Margin = new System.Windows.Forms.Padding(4);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(139, 38);
            this.button_next.TabIndex = 2;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // button_projectManage
            // 
            this.button_projectManage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_projectManage.Location = new System.Drawing.Point(0, 402);
            this.button_projectManage.Margin = new System.Windows.Forms.Padding(4);
            this.button_projectManage.Name = "button_projectManage";
            this.button_projectManage.Size = new System.Drawing.Size(172, 38);
            this.button_projectManage.TabIndex = 1;
            this.button_projectManage.Text = "方案管理(&M)...";
            this.button_projectManage.UseVisualStyleBackColor = true;
            this.button_projectManage.Click += new System.EventHandler(this.button_projectManage_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(9, 47);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(109, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(9, 77);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(109, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // OperLogStatisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 458);
            this.Controls.Add(this.button_projectManage);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OperLogStatisForm";
            this.ShowInTaskbar = false;
            this.Text = "日志统计窗";
            this.Activated += new System.EventHandler(this.OperLogStatisForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OperLogStatisForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OperLogStatisForm_FormClosed);
            this.Load += new System.EventHandler(this.OperLogStatisForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_selectProject.ResumeLayout(false);
            this.tabPage_selectProject.PerformLayout();
            this.tabPage_timeRange.ResumeLayout(false);
            this.tabPage_timeRange.PerformLayout();
            this.groupBox_tableStyle.ResumeLayout(false);
            this.groupBox_tableStyle.PerformLayout();
            this.tabPage_runStatis.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabPage_print.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_timeRange;
        private System.Windows.Forms.TabPage tabPage_selectProject;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.DateControl dateControl_start;
        private DigitalPlatform.CommonControl.DateControl dateControl_end;
        private System.Windows.Forms.TabPage tabPage_runStatis;
        private System.Windows.Forms.WebBrowser webBrowser1_running;
        private System.Windows.Forms.ProgressBar progressBar_records;
        private System.Windows.Forms.Button button_projectManage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_getProjectName;
        private System.Windows.Forms.ProgressBar progressBar_files;
        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.CheckBox checkBox_startToEndTable;
        private System.Windows.Forms.CheckBox checkBox_perMonthTable;
        private System.Windows.Forms.CheckBox checkBox_perYearTable;
        private System.Windows.Forms.CheckBox checkBox_perDayTable;
        private System.Windows.Forms.GroupBox groupBox_tableStyle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox comboBox_quickSetFilenames;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_defaultProject_1;
        private System.Windows.Forms.TabPage tabPage_management;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox comboBox_projectName;
        private System.Windows.Forms.Button button_option;
    }
}