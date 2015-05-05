namespace dp2Circulation
{
    partial class ChargingPrintManageForm
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
            System.Windows.Forms.ColumnHeader columnHeader_readerBarcode;
            System.Windows.Forms.ColumnHeader columnHeader_operTime;
            System.Windows.Forms.ColumnHeader columnHeader1;
            System.Windows.Forms.ColumnHeader columnHeader2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChargingPrintManageForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_currentContent = new System.Windows.Forms.TabPage();
            this.textBox_currentContent = new dp2Circulation.Print.TextAndHtmlControl();
            this.button_currentContent_push = new System.Windows.Forms.Button();
            this.button_currentContent_print = new System.Windows.Forms.Button();
            this.tabPage_printed = new System.Windows.Forms.TabPage();
            this.button_printed_print = new System.Windows.Forms.Button();
            this.splitContainer_printed = new System.Windows.Forms.SplitContainer();
            this.listView_printed_list = new DigitalPlatform.GUI.ListViewNF();
            this.textBox_printed_oneContent = new dp2Circulation.Print.TextAndHtmlControl();
            this.tabPage_unprint = new System.Windows.Forms.TabPage();
            this.button_unprint_print = new System.Windows.Forms.Button();
            this.splitContainer_unprint = new System.Windows.Forms.SplitContainer();
            this.listView_unprint_list = new DigitalPlatform.GUI.ListViewNF();
            this.textBox_unprint_oneContent = new dp2Circulation.Print.TextAndHtmlControl();
            this.button_refresh = new System.Windows.Forms.Button();
            this.button_testPrint = new System.Windows.Forms.Button();
            this.button_clearPrinterPreference = new System.Windows.Forms.Button();
            columnHeader_readerBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader_operTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl_main.SuspendLayout();
            this.tabPage_currentContent.SuspendLayout();
            this.tabPage_printed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_printed)).BeginInit();
            this.splitContainer_printed.Panel1.SuspendLayout();
            this.splitContainer_printed.Panel2.SuspendLayout();
            this.splitContainer_printed.SuspendLayout();
            this.tabPage_unprint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_unprint)).BeginInit();
            this.splitContainer_unprint.Panel1.SuspendLayout();
            this.splitContainer_unprint.Panel2.SuspendLayout();
            this.splitContainer_unprint.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeader_readerBarcode
            // 
            columnHeader_readerBarcode.Text = "读者证条码";
            columnHeader_readerBarcode.Width = 120;
            // 
            // columnHeader_operTime
            // 
            columnHeader_operTime.Text = "操作时间";
            columnHeader_operTime.Width = 200;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "读者证条码";
            columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "操作时间";
            columnHeader2.Width = 200;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_currentContent);
            this.tabControl_main.Controls.Add(this.tabPage_printed);
            this.tabControl_main.Controls.Add(this.tabPage_unprint);
            this.tabControl_main.Location = new System.Drawing.Point(0, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(418, 250);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_currentContent
            // 
            this.tabPage_currentContent.Controls.Add(this.textBox_currentContent);
            this.tabPage_currentContent.Controls.Add(this.button_currentContent_push);
            this.tabPage_currentContent.Controls.Add(this.button_currentContent_print);
            this.tabPage_currentContent.Location = new System.Drawing.Point(4, 22);
            this.tabPage_currentContent.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_currentContent.Name = "tabPage_currentContent";
            this.tabPage_currentContent.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_currentContent.Size = new System.Drawing.Size(410, 224);
            this.tabPage_currentContent.TabIndex = 2;
            this.tabPage_currentContent.Text = "当前缓冲";
            this.tabPage_currentContent.UseVisualStyleBackColor = true;
            // 
            // textBox_currentContent
            // 
            this.textBox_currentContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_currentContent.Location = new System.Drawing.Point(0, 6);
            this.textBox_currentContent.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_currentContent.Name = "textBox_currentContent";
            this.textBox_currentContent.ReadOnly = true;
            this.textBox_currentContent.Size = new System.Drawing.Size(412, 190);
            this.textBox_currentContent.TabIndex = 7;
            // 
            // button_currentContent_push
            // 
            this.button_currentContent_push.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_currentContent_push.Location = new System.Drawing.Point(337, 201);
            this.button_currentContent_push.Margin = new System.Windows.Forms.Padding(2);
            this.button_currentContent_push.Name = "button_currentContent_push";
            this.button_currentContent_push.Size = new System.Drawing.Size(71, 22);
            this.button_currentContent_push.TabIndex = 6;
            this.button_currentContent_push.Text = "推走(&H)";
            this.button_currentContent_push.UseVisualStyleBackColor = true;
            this.button_currentContent_push.Click += new System.EventHandler(this.button_currentContent_push_Click);
            // 
            // button_currentContent_print
            // 
            this.button_currentContent_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_currentContent_print.Location = new System.Drawing.Point(0, 201);
            this.button_currentContent_print.Margin = new System.Windows.Forms.Padding(2);
            this.button_currentContent_print.Name = "button_currentContent_print";
            this.button_currentContent_print.Size = new System.Drawing.Size(71, 22);
            this.button_currentContent_print.TabIndex = 5;
            this.button_currentContent_print.Text = "打印(&P)";
            this.button_currentContent_print.UseVisualStyleBackColor = true;
            this.button_currentContent_print.Click += new System.EventHandler(this.button_currentContent_print_Click);
            // 
            // tabPage_printed
            // 
            this.tabPage_printed.Controls.Add(this.button_printed_print);
            this.tabPage_printed.Controls.Add(this.splitContainer_printed);
            this.tabPage_printed.Location = new System.Drawing.Point(4, 22);
            this.tabPage_printed.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_printed.Name = "tabPage_printed";
            this.tabPage_printed.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_printed.Size = new System.Drawing.Size(410, 224);
            this.tabPage_printed.TabIndex = 0;
            this.tabPage_printed.Text = "已打印队列";
            this.tabPage_printed.UseVisualStyleBackColor = true;
            // 
            // button_printed_print
            // 
            this.button_printed_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_printed_print.Enabled = false;
            this.button_printed_print.Location = new System.Drawing.Point(0, 201);
            this.button_printed_print.Margin = new System.Windows.Forms.Padding(2);
            this.button_printed_print.Name = "button_printed_print";
            this.button_printed_print.Size = new System.Drawing.Size(56, 22);
            this.button_printed_print.TabIndex = 2;
            this.button_printed_print.Text = "打印(&P)";
            this.button_printed_print.UseVisualStyleBackColor = true;
            this.button_printed_print.Click += new System.EventHandler(this.button_printed_print_Click);
            // 
            // splitContainer_printed
            // 
            this.splitContainer_printed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_printed.Location = new System.Drawing.Point(0, 6);
            this.splitContainer_printed.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_printed.Name = "splitContainer_printed";
            // 
            // splitContainer_printed.Panel1
            // 
            this.splitContainer_printed.Panel1.Controls.Add(this.listView_printed_list);
            // 
            // splitContainer_printed.Panel2
            // 
            this.splitContainer_printed.Panel2.Controls.Add(this.textBox_printed_oneContent);
            this.splitContainer_printed.Size = new System.Drawing.Size(412, 190);
            this.splitContainer_printed.SplitterDistance = 182;
            this.splitContainer_printed.SplitterWidth = 6;
            this.splitContainer_printed.TabIndex = 1;
            // 
            // listView_printed_list
            // 
            this.listView_printed_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader_readerBarcode,
            columnHeader_operTime});
            this.listView_printed_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_printed_list.FullRowSelect = true;
            this.listView_printed_list.HideSelection = false;
            this.listView_printed_list.Location = new System.Drawing.Point(0, 0);
            this.listView_printed_list.Margin = new System.Windows.Forms.Padding(0);
            this.listView_printed_list.Name = "listView_printed_list";
            this.listView_printed_list.Size = new System.Drawing.Size(182, 190);
            this.listView_printed_list.TabIndex = 0;
            this.listView_printed_list.UseCompatibleStateImageBehavior = false;
            this.listView_printed_list.View = System.Windows.Forms.View.Details;
            this.listView_printed_list.SelectedIndexChanged += new System.EventHandler(this.listView_printed_list_SelectedIndexChanged);
            // 
            // textBox_printed_oneContent
            // 
            this.textBox_printed_oneContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_printed_oneContent.Location = new System.Drawing.Point(0, 0);
            this.textBox_printed_oneContent.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_printed_oneContent.Name = "textBox_printed_oneContent";
            this.textBox_printed_oneContent.ReadOnly = true;
            this.textBox_printed_oneContent.Size = new System.Drawing.Size(224, 190);
            this.textBox_printed_oneContent.TabIndex = 0;
            // 
            // tabPage_unprint
            // 
            this.tabPage_unprint.Controls.Add(this.button_unprint_print);
            this.tabPage_unprint.Controls.Add(this.splitContainer_unprint);
            this.tabPage_unprint.Location = new System.Drawing.Point(4, 22);
            this.tabPage_unprint.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_unprint.Name = "tabPage_unprint";
            this.tabPage_unprint.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_unprint.Size = new System.Drawing.Size(410, 224);
            this.tabPage_unprint.TabIndex = 1;
            this.tabPage_unprint.Text = "未打印队列";
            this.tabPage_unprint.UseVisualStyleBackColor = true;
            // 
            // button_unprint_print
            // 
            this.button_unprint_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_unprint_print.Enabled = false;
            this.button_unprint_print.Location = new System.Drawing.Point(0, 200);
            this.button_unprint_print.Margin = new System.Windows.Forms.Padding(2);
            this.button_unprint_print.Name = "button_unprint_print";
            this.button_unprint_print.Size = new System.Drawing.Size(56, 22);
            this.button_unprint_print.TabIndex = 4;
            this.button_unprint_print.Text = "打印(&P)";
            this.button_unprint_print.UseVisualStyleBackColor = true;
            this.button_unprint_print.Click += new System.EventHandler(this.button_unprint_print_Click);
            // 
            // splitContainer_unprint
            // 
            this.splitContainer_unprint.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_unprint.Location = new System.Drawing.Point(0, 6);
            this.splitContainer_unprint.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_unprint.Name = "splitContainer_unprint";
            // 
            // splitContainer_unprint.Panel1
            // 
            this.splitContainer_unprint.Panel1.Controls.Add(this.listView_unprint_list);
            // 
            // splitContainer_unprint.Panel2
            // 
            this.splitContainer_unprint.Panel2.Controls.Add(this.textBox_unprint_oneContent);
            this.splitContainer_unprint.Size = new System.Drawing.Size(412, 190);
            this.splitContainer_unprint.SplitterDistance = 182;
            this.splitContainer_unprint.SplitterWidth = 6;
            this.splitContainer_unprint.TabIndex = 3;
            // 
            // listView_unprint_list
            // 
            this.listView_unprint_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2});
            this.listView_unprint_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_unprint_list.FullRowSelect = true;
            this.listView_unprint_list.HideSelection = false;
            this.listView_unprint_list.Location = new System.Drawing.Point(0, 0);
            this.listView_unprint_list.Margin = new System.Windows.Forms.Padding(0);
            this.listView_unprint_list.Name = "listView_unprint_list";
            this.listView_unprint_list.Size = new System.Drawing.Size(182, 190);
            this.listView_unprint_list.TabIndex = 0;
            this.listView_unprint_list.UseCompatibleStateImageBehavior = false;
            this.listView_unprint_list.View = System.Windows.Forms.View.Details;
            this.listView_unprint_list.SelectedIndexChanged += new System.EventHandler(this.listView_unprint_list_SelectedIndexChanged);
            // 
            // textBox_unprint_oneContent
            // 
            this.textBox_unprint_oneContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_unprint_oneContent.Location = new System.Drawing.Point(0, 0);
            this.textBox_unprint_oneContent.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_unprint_oneContent.Name = "textBox_unprint_oneContent";
            this.textBox_unprint_oneContent.ReadOnly = true;
            this.textBox_unprint_oneContent.Size = new System.Drawing.Size(224, 190);
            this.textBox_unprint_oneContent.TabIndex = 0;
            // 
            // button_refresh
            // 
            this.button_refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_refresh.Location = new System.Drawing.Point(0, 266);
            this.button_refresh.Margin = new System.Windows.Forms.Padding(2);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(71, 22);
            this.button_refresh.TabIndex = 1;
            this.button_refresh.Text = "刷新(&R)";
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // button_testPrint
            // 
            this.button_testPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_testPrint.Location = new System.Drawing.Point(323, 266);
            this.button_testPrint.Margin = new System.Windows.Forms.Padding(2);
            this.button_testPrint.Name = "button_testPrint";
            this.button_testPrint.Size = new System.Drawing.Size(95, 22);
            this.button_testPrint.TabIndex = 2;
            this.button_testPrint.Text = "测试打印(&T)";
            this.button_testPrint.UseVisualStyleBackColor = true;
            this.button_testPrint.Click += new System.EventHandler(this.button_testPrint_Click);
            // 
            // button_clearPrinterPreference
            // 
            this.button_clearPrinterPreference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_clearPrinterPreference.Location = new System.Drawing.Point(182, 266);
            this.button_clearPrinterPreference.Margin = new System.Windows.Forms.Padding(2);
            this.button_clearPrinterPreference.Name = "button_clearPrinterPreference";
            this.button_clearPrinterPreference.Size = new System.Drawing.Size(137, 22);
            this.button_clearPrinterPreference.TabIndex = 3;
            this.button_clearPrinterPreference.Text = "清除打印机配置(&C)";
            this.button_clearPrinterPreference.UseVisualStyleBackColor = true;
            this.button_clearPrinterPreference.Click += new System.EventHandler(this.button_clearPrinterPreference_Click);
            // 
            // ChargingPrintManageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 298);
            this.Controls.Add(this.button_clearPrinterPreference);
            this.Controls.Add(this.button_testPrint);
            this.Controls.Add(this.button_refresh);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ChargingPrintManageForm";
            this.ShowInTaskbar = false;
            this.Text = "出纳打印管理";
            this.Load += new System.EventHandler(this.ChargingPrintManageDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_currentContent.ResumeLayout(false);
            this.tabPage_printed.ResumeLayout(false);
            this.splitContainer_printed.Panel1.ResumeLayout(false);
            this.splitContainer_printed.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_printed)).EndInit();
            this.splitContainer_printed.ResumeLayout(false);
            this.tabPage_unprint.ResumeLayout(false);
            this.splitContainer_unprint.Panel1.ResumeLayout(false);
            this.splitContainer_unprint.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_unprint)).EndInit();
            this.splitContainer_unprint.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_printed;
        private System.Windows.Forms.TabPage tabPage_unprint;
        private System.Windows.Forms.SplitContainer splitContainer_printed;
        private System.Windows.Forms.Button button_printed_print;
        private DigitalPlatform.GUI.ListViewNF listView_printed_list;
        private System.Windows.Forms.Button button_unprint_print;
        private System.Windows.Forms.SplitContainer splitContainer_unprint;
        private DigitalPlatform.GUI.ListViewNF listView_unprint_list;
        private System.Windows.Forms.TabPage tabPage_currentContent;
        private System.Windows.Forms.Button button_currentContent_print;
        private System.Windows.Forms.Button button_refresh;
        private System.Windows.Forms.Button button_testPrint;
        private System.Windows.Forms.Button button_currentContent_push;
        private dp2Circulation.Print.TextAndHtmlControl textBox_currentContent;
        private dp2Circulation.Print.TextAndHtmlControl textBox_printed_oneContent;
        private dp2Circulation.Print.TextAndHtmlControl textBox_unprint_oneContent;
        private System.Windows.Forms.Button button_clearPrinterPreference;
    }
}