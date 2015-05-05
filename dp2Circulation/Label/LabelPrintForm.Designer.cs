namespace dp2Circulation
{
    partial class LabelPrintForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabelPrintForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_labelFile = new System.Windows.Forms.TabPage();
            this.textBox_labelFile_content = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_labelFile_findLabelFilename = new System.Windows.Forms.Button();
            this.textBox_labelFile_labelFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_itemRecords = new System.Windows.Forms.TabPage();
            this.splitContainer_itemRecords = new System.Windows.Forms.SplitContainer();
            this.listView_records = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label_message = new System.Windows.Forms.Label();
            this.textBox_errorInfo = new System.Windows.Forms.TextBox();
            this.button_findLabelDefFilename = new System.Windows.Forms.Button();
            this.textBox_labelDefFilename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_print = new System.Windows.Forms.Button();
            this.button_printPreview = new System.Windows.Forms.Button();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.checkBox_testingGrid = new System.Windows.Forms.CheckBox();
            this.button_editDefFile = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_labelFile.SuspendLayout();
            this.tabPage_itemRecords.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemRecords)).BeginInit();
            this.splitContainer_itemRecords.Panel1.SuspendLayout();
            this.splitContainer_itemRecords.Panel2.SuspendLayout();
            this.splitContainer_itemRecords.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_labelFile);
            this.tabControl_main.Controls.Add(this.tabPage_itemRecords);
            this.tabControl_main.Location = new System.Drawing.Point(0, 35);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(435, 241);
            this.tabControl_main.TabIndex = 4;
            // 
            // tabPage_labelFile
            // 
            this.tabPage_labelFile.Controls.Add(this.textBox_labelFile_content);
            this.tabPage_labelFile.Controls.Add(this.label2);
            this.tabPage_labelFile.Controls.Add(this.button_labelFile_findLabelFilename);
            this.tabPage_labelFile.Controls.Add(this.textBox_labelFile_labelFilename);
            this.tabPage_labelFile.Controls.Add(this.label1);
            this.tabPage_labelFile.Location = new System.Drawing.Point(4, 22);
            this.tabPage_labelFile.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_labelFile.Name = "tabPage_labelFile";
            this.tabPage_labelFile.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_labelFile.Size = new System.Drawing.Size(427, 215);
            this.tabPage_labelFile.TabIndex = 0;
            this.tabPage_labelFile.Text = "标签文件";
            this.tabPage_labelFile.UseVisualStyleBackColor = true;
            // 
            // textBox_labelFile_content
            // 
            this.textBox_labelFile_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_labelFile_content.Location = new System.Drawing.Point(0, 57);
            this.textBox_labelFile_content.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_labelFile_content.MaxLength = 0;
            this.textBox_labelFile_content.Multiline = true;
            this.textBox_labelFile_content.Name = "textBox_labelFile_content";
            this.textBox_labelFile_content.ReadOnly = true;
            this.textBox_labelFile_content.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_labelFile_content.Size = new System.Drawing.Size(430, 163);
            this.textBox_labelFile_content.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-2, 42);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "文件内容(&C):";
            // 
            // button_labelFile_findLabelFilename
            // 
            this.button_labelFile_findLabelFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_labelFile_findLabelFilename.Location = new System.Drawing.Point(391, 12);
            this.button_labelFile_findLabelFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_labelFile_findLabelFilename.Name = "button_labelFile_findLabelFilename";
            this.button_labelFile_findLabelFilename.Size = new System.Drawing.Size(38, 22);
            this.button_labelFile_findLabelFilename.TabIndex = 2;
            this.button_labelFile_findLabelFilename.Text = "...";
            this.button_labelFile_findLabelFilename.UseVisualStyleBackColor = true;
            this.button_labelFile_findLabelFilename.Click += new System.EventHandler(this.button_labelFile_findLabelFilename_Click);
            // 
            // textBox_labelFile_labelFilename
            // 
            this.textBox_labelFile_labelFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_labelFile_labelFilename.Location = new System.Drawing.Point(116, 12);
            this.textBox_labelFile_labelFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_labelFile_labelFilename.Name = "textBox_labelFile_labelFilename";
            this.textBox_labelFile_labelFilename.Size = new System.Drawing.Size(271, 21);
            this.textBox_labelFile_labelFilename.TabIndex = 1;
            this.textBox_labelFile_labelFilename.TextChanged += new System.EventHandler(this.textBox_labelFile_labelFilename_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "标签文件名(&L):";
            // 
            // tabPage_itemRecords
            // 
            this.tabPage_itemRecords.Controls.Add(this.splitContainer_itemRecords);
            this.tabPage_itemRecords.Location = new System.Drawing.Point(4, 22);
            this.tabPage_itemRecords.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_itemRecords.Name = "tabPage_itemRecords";
            this.tabPage_itemRecords.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_itemRecords.Size = new System.Drawing.Size(427, 215);
            this.tabPage_itemRecords.TabIndex = 1;
            this.tabPage_itemRecords.Text = "册记录";
            this.tabPage_itemRecords.UseVisualStyleBackColor = true;
            // 
            // splitContainer_itemRecords
            // 
            this.splitContainer_itemRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_itemRecords.Location = new System.Drawing.Point(2, 2);
            this.splitContainer_itemRecords.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_itemRecords.Name = "splitContainer_itemRecords";
            this.splitContainer_itemRecords.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_itemRecords.Panel1
            // 
            this.splitContainer_itemRecords.Panel1.Controls.Add(this.listView_records);
            this.splitContainer_itemRecords.Panel1.Controls.Add(this.label_message);
            // 
            // splitContainer_itemRecords.Panel2
            // 
            this.splitContainer_itemRecords.Panel2.Controls.Add(this.textBox_errorInfo);
            this.splitContainer_itemRecords.Size = new System.Drawing.Size(423, 211);
            this.splitContainer_itemRecords.SplitterDistance = 124;
            this.splitContainer_itemRecords.SplitterWidth = 6;
            this.splitContainer_itemRecords.TabIndex = 13;
            // 
            // listView_records
            // 
            this.listView_records.AllowDrop = true;
            this.listView_records.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1});
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(0, 0);
            this.listView_records.Name = "listView_records";
            this.listView_records.Size = new System.Drawing.Size(424, 101);
            this.listView_records.TabIndex = 11;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            this.listView_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_records_ColumnClick);
            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            this.listView_records.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_records_DragDrop);
            this.listView_records.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_records_DragEnter);
            this.listView_records.DoubleClick += new System.EventHandler(this.listView_records_DoubleClick);
            this.listView_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_records_MouseUp);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 150;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 200;
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(2, 104);
            this.label_message.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(419, 18);
            this.label_message.TabIndex = 12;
            // 
            // textBox_errorInfo
            // 
            this.textBox_errorInfo.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_errorInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_errorInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_errorInfo.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_errorInfo.Location = new System.Drawing.Point(0, 0);
            this.textBox_errorInfo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_errorInfo.MaxLength = 0;
            this.textBox_errorInfo.Multiline = true;
            this.textBox_errorInfo.Name = "textBox_errorInfo";
            this.textBox_errorInfo.ReadOnly = true;
            this.textBox_errorInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_errorInfo.Size = new System.Drawing.Size(423, 81);
            this.textBox_errorInfo.TabIndex = 0;
            this.textBox_errorInfo.DoubleClick += new System.EventHandler(this.textBox_errorInfo_DoubleClick);
            // 
            // button_findLabelDefFilename
            // 
            this.button_findLabelDefFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findLabelDefFilename.Location = new System.Drawing.Point(355, 8);
            this.button_findLabelDefFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_findLabelDefFilename.Name = "button_findLabelDefFilename";
            this.button_findLabelDefFilename.Size = new System.Drawing.Size(34, 22);
            this.button_findLabelDefFilename.TabIndex = 2;
            this.button_findLabelDefFilename.Text = "...";
            this.button_findLabelDefFilename.UseVisualStyleBackColor = true;
            this.button_findLabelDefFilename.Click += new System.EventHandler(this.button_labelFile_findLabelDefFilename_Click);
            // 
            // textBox_labelDefFilename
            // 
            this.textBox_labelDefFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_labelDefFilename.Location = new System.Drawing.Point(120, 10);
            this.textBox_labelDefFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_labelDefFilename.Name = "textBox_labelDefFilename";
            this.textBox_labelDefFilename.Size = new System.Drawing.Size(231, 21);
            this.textBox_labelDefFilename.TabIndex = 1;
            this.textBox_labelDefFilename.TextChanged += new System.EventHandler(this.textBox_labelDefFilename_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "标签定义文件名(&D):";
            // 
            // button_print
            // 
            this.button_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_print.Location = new System.Drawing.Point(237, 281);
            this.button_print.Margin = new System.Windows.Forms.Padding(2);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(88, 22);
            this.button_print.TabIndex = 6;
            this.button_print.Text = "打印(&P)...";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // button_printPreview
            // 
            this.button_printPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_printPreview.Location = new System.Drawing.Point(330, 281);
            this.button_printPreview.Margin = new System.Windows.Forms.Padding(2);
            this.button_printPreview.Name = "button_printPreview";
            this.button_printPreview.Size = new System.Drawing.Size(105, 22);
            this.button_printPreview.TabIndex = 7;
            this.button_printPreview.Text = "打印预览(&V)...";
            this.button_printPreview.UseVisualStyleBackColor = true;
            this.button_printPreview.Click += new System.EventHandler(this.button_printPreview_Click);
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // checkBox_testingGrid
            // 
            this.checkBox_testingGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_testingGrid.AutoSize = true;
            this.checkBox_testingGrid.Location = new System.Drawing.Point(0, 287);
            this.checkBox_testingGrid.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_testingGrid.Name = "checkBox_testingGrid";
            this.checkBox_testingGrid.Size = new System.Drawing.Size(102, 16);
            this.checkBox_testingGrid.TabIndex = 5;
            this.checkBox_testingGrid.Text = "打印调试线(&T)";
            this.checkBox_testingGrid.UseVisualStyleBackColor = true;
            // 
            // button_editDefFile
            // 
            this.button_editDefFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editDefFile.Location = new System.Drawing.Point(393, 8);
            this.button_editDefFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_editDefFile.Name = "button_editDefFile";
            this.button_editDefFile.Size = new System.Drawing.Size(38, 22);
            this.button_editDefFile.TabIndex = 3;
            this.button_editDefFile.Text = "设计";
            this.button_editDefFile.UseVisualStyleBackColor = true;
            this.button_editDefFile.Click += new System.EventHandler(this.button_editDefFile_Click);
            // 
            // LabelPrintForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 313);
            this.Controls.Add(this.button_editDefFile);
            this.Controls.Add(this.button_findLabelDefFilename);
            this.Controls.Add(this.checkBox_testingGrid);
            this.Controls.Add(this.textBox_labelDefFilename);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_printPreview);
            this.Controls.Add(this.button_print);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LabelPrintForm";
            this.ShowInTaskbar = false;
            this.Text = "标签打印";
            this.Activated += new System.EventHandler(this.LabelPrintForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LabelPrintForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LabelPrintForm_FormClosed);
            this.Load += new System.EventHandler(this.LabelPrintForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_labelFile.ResumeLayout(false);
            this.tabPage_labelFile.PerformLayout();
            this.tabPage_itemRecords.ResumeLayout(false);
            this.splitContainer_itemRecords.Panel1.ResumeLayout(false);
            this.splitContainer_itemRecords.Panel2.ResumeLayout(false);
            this.splitContainer_itemRecords.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemRecords)).EndInit();
            this.splitContainer_itemRecords.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_labelFile;
        private System.Windows.Forms.TabPage tabPage_itemRecords;
        private System.Windows.Forms.TextBox textBox_labelFile_labelFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_labelFile_findLabelFilename;
        private System.Windows.Forms.TextBox textBox_labelFile_content;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_printPreview;
        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.Button button_findLabelDefFilename;
        private System.Windows.Forms.TextBox textBox_labelDefFilename;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox_testingGrid;
        private DigitalPlatform.GUI.ListViewNF listView_records;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.SplitContainer splitContainer_itemRecords;
        private System.Windows.Forms.TextBox textBox_errorInfo;
        private System.Windows.Forms.Button button_editDefFile;
    }
}