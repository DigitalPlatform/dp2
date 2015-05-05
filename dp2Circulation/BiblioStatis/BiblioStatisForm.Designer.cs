namespace dp2Circulation
{
    partial class BiblioStatisForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiblioStatisForm));
            this.button_projectManage = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_source = new System.Windows.Forms.TabPage();
            this.radioButton_inputStyle_recPaths = new System.Windows.Forms.RadioButton();
            this.textBox_inputStyle_recPaths = new System.Windows.Forms.TextBox();
            this.tabComboBox_inputBatchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButton_inputStyle_recPathFile = new System.Windows.Forms.RadioButton();
            this.button_findInputRecPathFilename = new System.Windows.Forms.Button();
            this.textBox_inputRecPathFilename = new System.Windows.Forms.TextBox();
            this.radioButton_inputStyle_biblioDatabase = new System.Windows.Forms.RadioButton();
            this.comboBox_inputBiblioDbName = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage_filter = new System.Windows.Forms.TabPage();
            this.tabPage_selectProject = new System.Windows.Forms.TabPage();
            this.button_getProjectName = new System.Windows.Forms.Button();
            this.textBox_projectName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_runStatis = new System.Windows.Forms.TabPage();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.webBrowser1_running = new System.Windows.Forms.WebBrowser();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print = new System.Windows.Forms.Button();
            this.tableLayoutPanel_running = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl_main.SuspendLayout();
            this.tabPage_source.SuspendLayout();
            this.tabPage_selectProject.SuspendLayout();
            this.tabPage_runStatis.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.tableLayoutPanel_running.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_projectManage
            // 
            this.button_projectManage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_projectManage.Location = new System.Drawing.Point(0, 316);
            this.button_projectManage.Margin = new System.Windows.Forms.Padding(2);
            this.button_projectManage.Name = "button_projectManage";
            this.button_projectManage.Size = new System.Drawing.Size(94, 22);
            this.button_projectManage.TabIndex = 11;
            this.button_projectManage.Text = "方案管理(&M)...";
            this.button_projectManage.UseVisualStyleBackColor = true;
            this.button_projectManage.Click += new System.EventHandler(this.button_projectManage_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(334, 317);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(76, 22);
            this.button_next.TabIndex = 10;
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
            this.tabControl_main.Size = new System.Drawing.Size(410, 302);
            this.tabControl_main.TabIndex = 9;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_source
            // 
            this.tabPage_source.AutoScroll = true;
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_recPaths);
            this.tabPage_source.Controls.Add(this.textBox_inputStyle_recPaths);
            this.tabPage_source.Controls.Add(this.tabComboBox_inputBatchNo);
            this.tabPage_source.Controls.Add(this.label4);
            this.tabPage_source.Controls.Add(this.label2);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_recPathFile);
            this.tabPage_source.Controls.Add(this.button_findInputRecPathFilename);
            this.tabPage_source.Controls.Add(this.textBox_inputRecPathFilename);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_biblioDatabase);
            this.tabPage_source.Controls.Add(this.comboBox_inputBiblioDbName);
            this.tabPage_source.Controls.Add(this.label6);
            this.tabPage_source.Location = new System.Drawing.Point(4, 22);
            this.tabPage_source.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Size = new System.Drawing.Size(402, 276);
            this.tabPage_source.TabIndex = 4;
            this.tabPage_source.Text = "数据来源";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // radioButton_inputStyle_recPaths
            // 
            this.radioButton_inputStyle_recPaths.AutoSize = true;
            this.radioButton_inputStyle_recPaths.Location = new System.Drawing.Point(10, 172);
            this.radioButton_inputStyle_recPaths.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_inputStyle_recPaths.Name = "radioButton_inputStyle_recPaths";
            this.radioButton_inputStyle_recPaths.Size = new System.Drawing.Size(161, 16);
            this.radioButton_inputStyle_recPaths.TabIndex = 9;
            this.radioButton_inputStyle_recPaths.Text = "记录路径 [每行一个] (&P)";
            this.radioButton_inputStyle_recPaths.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_recPaths.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_recPaths_CheckedChanged);
            // 
            // textBox_inputStyle_recPaths
            // 
            this.textBox_inputStyle_recPaths.Enabled = false;
            this.textBox_inputStyle_recPaths.Location = new System.Drawing.Point(35, 192);
            this.textBox_inputStyle_recPaths.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_inputStyle_recPaths.MaxLength = 0;
            this.textBox_inputStyle_recPaths.Multiline = true;
            this.textBox_inputStyle_recPaths.Name = "textBox_inputStyle_recPaths";
            this.textBox_inputStyle_recPaths.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_inputStyle_recPaths.Size = new System.Drawing.Size(230, 100);
            this.textBox_inputStyle_recPaths.TabIndex = 10;
            this.textBox_inputStyle_recPaths.WordWrap = false;
            // 
            // tabComboBox_inputBatchNo
            // 
            this.tabComboBox_inputBatchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_inputBatchNo.FormattingEnabled = true;
            this.tabComboBox_inputBatchNo.LeftFontStyle = System.Drawing.FontStyle.Bold;
            this.tabComboBox_inputBatchNo.Location = new System.Drawing.Point(125, 94);
            this.tabComboBox_inputBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.tabComboBox_inputBatchNo.Name = "tabComboBox_inputBatchNo";
            this.tabComboBox_inputBatchNo.RightFontStyle = System.Drawing.FontStyle.Italic;
            this.tabComboBox_inputBatchNo.Size = new System.Drawing.Size(140, 22);
            this.tabComboBox_inputBatchNo.TabIndex = 5;
            this.tabComboBox_inputBatchNo.DropDown += new System.EventHandler(this.tabComboBox_inputBatchNo_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(123, 117);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "(为空则表示全部)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 97);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "编目批次号(&B):";
            // 
            // radioButton_inputStyle_recPathFile
            // 
            this.radioButton_inputStyle_recPathFile.AutoSize = true;
            this.radioButton_inputStyle_recPathFile.Location = new System.Drawing.Point(10, 13);
            this.radioButton_inputStyle_recPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_inputStyle_recPathFile.Name = "radioButton_inputStyle_recPathFile";
            this.radioButton_inputStyle_recPathFile.Size = new System.Drawing.Size(113, 16);
            this.radioButton_inputStyle_recPathFile.TabIndex = 0;
            this.radioButton_inputStyle_recPathFile.Text = "记录路径文件(&F)";
            this.radioButton_inputStyle_recPathFile.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_recPathFile.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_recPathFile_CheckedChanged);
            // 
            // button_findInputRecPathFilename
            // 
            this.button_findInputRecPathFilename.Enabled = false;
            this.button_findInputRecPathFilename.Location = new System.Drawing.Point(270, 33);
            this.button_findInputRecPathFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_findInputRecPathFilename.Name = "button_findInputRecPathFilename";
            this.button_findInputRecPathFilename.Size = new System.Drawing.Size(32, 22);
            this.button_findInputRecPathFilename.TabIndex = 2;
            this.button_findInputRecPathFilename.Text = "...";
            this.button_findInputRecPathFilename.UseVisualStyleBackColor = true;
            this.button_findInputRecPathFilename.Click += new System.EventHandler(this.button_findInputRecPathFilename_Click);
            // 
            // textBox_inputRecPathFilename
            // 
            this.textBox_inputRecPathFilename.Enabled = false;
            this.textBox_inputRecPathFilename.Location = new System.Drawing.Point(36, 33);
            this.textBox_inputRecPathFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_inputRecPathFilename.Name = "textBox_inputRecPathFilename";
            this.textBox_inputRecPathFilename.Size = new System.Drawing.Size(230, 21);
            this.textBox_inputRecPathFilename.TabIndex = 1;
            // 
            // radioButton_inputStyle_biblioDatabase
            // 
            this.radioButton_inputStyle_biblioDatabase.AutoSize = true;
            this.radioButton_inputStyle_biblioDatabase.Checked = true;
            this.radioButton_inputStyle_biblioDatabase.Location = new System.Drawing.Point(10, 67);
            this.radioButton_inputStyle_biblioDatabase.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_inputStyle_biblioDatabase.Name = "radioButton_inputStyle_biblioDatabase";
            this.radioButton_inputStyle_biblioDatabase.Size = new System.Drawing.Size(131, 16);
            this.radioButton_inputStyle_biblioDatabase.TabIndex = 3;
            this.radioButton_inputStyle_biblioDatabase.TabStop = true;
            this.radioButton_inputStyle_biblioDatabase.Text = "整个库 / 批次号(&D)";
            this.radioButton_inputStyle_biblioDatabase.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_biblioDatabase.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_readerDatabase_CheckedChanged);
            // 
            // comboBox_inputBiblioDbName
            // 
            this.comboBox_inputBiblioDbName.FormattingEnabled = true;
            this.comboBox_inputBiblioDbName.Location = new System.Drawing.Point(125, 139);
            this.comboBox_inputBiblioDbName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_inputBiblioDbName.Name = "comboBox_inputBiblioDbName";
            this.comboBox_inputBiblioDbName.Size = new System.Drawing.Size(140, 20);
            this.comboBox_inputBiblioDbName.TabIndex = 8;
            this.comboBox_inputBiblioDbName.DropDown += new System.EventHandler(this.comboBox_inputBiblioDbName_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(34, 141);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "书目库名(&B):";
            // 
            // tabPage_filter
            // 
            this.tabPage_filter.Location = new System.Drawing.Point(4, 22);
            this.tabPage_filter.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_filter.Name = "tabPage_filter";
            this.tabPage_filter.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_filter.Size = new System.Drawing.Size(402, 276);
            this.tabPage_filter.TabIndex = 0;
            this.tabPage_filter.Text = " 筛选特性 ";
            this.tabPage_filter.UseVisualStyleBackColor = true;
            // 
            // tabPage_selectProject
            // 
            this.tabPage_selectProject.Controls.Add(this.button_getProjectName);
            this.tabPage_selectProject.Controls.Add(this.textBox_projectName);
            this.tabPage_selectProject.Controls.Add(this.label3);
            this.tabPage_selectProject.Location = new System.Drawing.Point(4, 22);
            this.tabPage_selectProject.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_selectProject.Name = "tabPage_selectProject";
            this.tabPage_selectProject.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_selectProject.Size = new System.Drawing.Size(402, 276);
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
            this.tabPage_runStatis.Controls.Add(this.tableLayoutPanel_running);
            this.tabPage_runStatis.Location = new System.Drawing.Point(4, 22);
            this.tabPage_runStatis.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_runStatis.Name = "tabPage_runStatis";
            this.tabPage_runStatis.Size = new System.Drawing.Size(402, 276);
            this.tabPage_runStatis.TabIndex = 3;
            this.tabPage_runStatis.Text = " 执行统计 ";
            this.tabPage_runStatis.UseVisualStyleBackColor = true;
            // 
            // progressBar_records
            // 
            this.progressBar_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar_records.Location = new System.Drawing.Point(2, 263);
            this.progressBar_records.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar_records.Name = "progressBar_records";
            this.progressBar_records.Size = new System.Drawing.Size(398, 11);
            this.progressBar_records.TabIndex = 1;
            // 
            // webBrowser1_running
            // 
            this.webBrowser1_running.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1_running.Location = new System.Drawing.Point(2, 2);
            this.webBrowser1_running.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser1_running.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser1_running.Name = "webBrowser1_running";
            this.webBrowser1_running.Size = new System.Drawing.Size(398, 257);
            this.webBrowser1_running.TabIndex = 0;
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 22);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(402, 276);
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
            // tableLayoutPanel_running
            // 
            this.tableLayoutPanel_running.ColumnCount = 1;
            this.tableLayoutPanel_running.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_running.Controls.Add(this.webBrowser1_running, 0, 0);
            this.tableLayoutPanel_running.Controls.Add(this.progressBar_records, 0, 1);
            this.tableLayoutPanel_running.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_running.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_running.Name = "tableLayoutPanel_running";
            this.tableLayoutPanel_running.RowCount = 2;
            this.tableLayoutPanel_running.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_running.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_running.Size = new System.Drawing.Size(402, 276);
            this.tableLayoutPanel_running.TabIndex = 2;
            // 
            // BiblioStatisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 349);
            this.Controls.Add(this.button_projectManage);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BiblioStatisForm";
            this.Text = "书目统计窗";
            this.Activated += new System.EventHandler(this.BiblioStatisForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BiblioStatisForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BiblioStatisForm_FormClosed);
            this.Load += new System.EventHandler(this.BiblioStatisForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_source.ResumeLayout(false);
            this.tabPage_source.PerformLayout();
            this.tabPage_selectProject.ResumeLayout(false);
            this.tabPage_selectProject.PerformLayout();
            this.tabPage_runStatis.ResumeLayout(false);
            this.tabPage_print.ResumeLayout(false);
            this.tableLayoutPanel_running.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_projectManage;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_source;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_recPathFile;
        private System.Windows.Forms.Button button_findInputRecPathFilename;
        private System.Windows.Forms.TextBox textBox_inputRecPathFilename;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_biblioDatabase;
        private System.Windows.Forms.ComboBox comboBox_inputBiblioDbName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabPage_filter;
        private System.Windows.Forms.TabPage tabPage_selectProject;
        private System.Windows.Forms.Button button_getProjectName;
        private System.Windows.Forms.TextBox textBox_projectName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_runStatis;
        private System.Windows.Forms.ProgressBar progressBar_records;
        private System.Windows.Forms.WebBrowser webBrowser1_running;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_print;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_inputBatchNo;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_recPaths;
        private System.Windows.Forms.TextBox textBox_inputStyle_recPaths;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_running;
    }
}