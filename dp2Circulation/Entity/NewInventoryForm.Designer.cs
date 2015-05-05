namespace dp2Circulation
{
    partial class NewInventoryForm
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
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_up = new System.Windows.Forms.Panel();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_load = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_load_type = new System.Windows.Forms.ComboBox();
            this.button_load_loadFromRecPathFile = new System.Windows.Forms.Button();
            this.button_load_loadFromBarcodeFile = new System.Windows.Forms.Button();
            this.tabPage_verify = new System.Windows.Forms.TabPage();
            this.button_verify_loadFromBarcodeFile = new System.Windows.Forms.Button();
            this.checkBox_verify_autoUppercaseBarcode = new System.Windows.Forms.CheckBox();
            this.button_verify_load = new System.Windows.Forms.Button();
            this.textBox_verify_itemBarcode = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print_option = new System.Windows.Forms.Button();
            this.button_print_printList = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.dpTable1 = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_icon1 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_icon2 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_barcode = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_summary = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_recpath = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_biblioRecPath = new DigitalPlatform.CommonControl.DpColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.panel_up.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_load.SuspendLayout();
            this.tabPage_verify.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_up);
            this.splitContainer_main.Panel1.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.dpTable1);
            this.splitContainer_main.Panel2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.splitContainer_main.Size = new System.Drawing.Size(426, 310);
            this.splitContainer_main.SplitterDistance = 149;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 7;
            // 
            // panel_up
            // 
            this.panel_up.Controls.Add(this.tabControl_main);
            this.panel_up.Controls.Add(this.button_next);
            this.panel_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_up.Location = new System.Drawing.Point(0, 12);
            this.panel_up.Name = "panel_up";
            this.panel_up.Size = new System.Drawing.Size(426, 137);
            this.panel_up.TabIndex = 4;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_load);
            this.tabControl_main.Controls.Add(this.tabPage_verify);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(0, 2);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(427, 109);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_load
            // 
            this.tabPage_load.AutoScroll = true;
            this.tabPage_load.Controls.Add(this.label4);
            this.tabPage_load.Controls.Add(this.comboBox_load_type);
            this.tabPage_load.Controls.Add(this.button_load_loadFromRecPathFile);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBarcodeFile);
            this.tabPage_load.Location = new System.Drawing.Point(4, 22);
            this.tabPage_load.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_load.Name = "tabPage_load";
            this.tabPage_load.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_load.Size = new System.Drawing.Size(419, 83);
            this.tabPage_load.TabIndex = 0;
            this.tabPage_load.Text = "装载";
            this.tabPage_load.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 8);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "出版物类型(&T):";
            // 
            // comboBox_load_type
            // 
            this.comboBox_load_type.FormattingEnabled = true;
            this.comboBox_load_type.Items.AddRange(new object[] {
            "图书",
            "连续出版物"});
            this.comboBox_load_type.Location = new System.Drawing.Point(7, 22);
            this.comboBox_load_type.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_load_type.Name = "comboBox_load_type";
            this.comboBox_load_type.Size = new System.Drawing.Size(116, 20);
            this.comboBox_load_type.TabIndex = 5;
            this.comboBox_load_type.Text = "图书";
            // 
            // button_load_loadFromRecPathFile
            // 
            this.button_load_loadFromRecPathFile.Location = new System.Drawing.Point(140, 31);
            this.button_load_loadFromRecPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromRecPathFile.Name = "button_load_loadFromRecPathFile";
            this.button_load_loadFromRecPathFile.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromRecPathFile.TabIndex = 3;
            this.button_load_loadFromRecPathFile.Text = "从册记录路径文件(&R)...";
            this.button_load_loadFromRecPathFile.UseVisualStyleBackColor = true;
            // 
            // button_load_loadFromBarcodeFile
            // 
            this.button_load_loadFromBarcodeFile.Location = new System.Drawing.Point(140, 5);
            this.button_load_loadFromBarcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromBarcodeFile.Name = "button_load_loadFromBarcodeFile";
            this.button_load_loadFromBarcodeFile.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromBarcodeFile.TabIndex = 2;
            this.button_load_loadFromBarcodeFile.Text = "从条码号文件(&F)...";
            this.button_load_loadFromBarcodeFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromBarcodeFile.Click += new System.EventHandler(this.button_load_loadFromBarcodeFile_Click);
            // 
            // tabPage_verify
            // 
            this.tabPage_verify.AutoScroll = true;
            this.tabPage_verify.Controls.Add(this.button_verify_loadFromBarcodeFile);
            this.tabPage_verify.Controls.Add(this.checkBox_verify_autoUppercaseBarcode);
            this.tabPage_verify.Controls.Add(this.button_verify_load);
            this.tabPage_verify.Controls.Add(this.textBox_verify_itemBarcode);
            this.tabPage_verify.Controls.Add(this.label3);
            this.tabPage_verify.Location = new System.Drawing.Point(4, 22);
            this.tabPage_verify.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_verify.Name = "tabPage_verify";
            this.tabPage_verify.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_verify.Size = new System.Drawing.Size(419, 83);
            this.tabPage_verify.TabIndex = 1;
            this.tabPage_verify.Text = "验证";
            this.tabPage_verify.UseVisualStyleBackColor = true;
            // 
            // button_verify_loadFromBarcodeFile
            // 
            this.button_verify_loadFromBarcodeFile.Location = new System.Drawing.Point(91, 53);
            this.button_verify_loadFromBarcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_verify_loadFromBarcodeFile.Name = "button_verify_loadFromBarcodeFile";
            this.button_verify_loadFromBarcodeFile.Size = new System.Drawing.Size(170, 22);
            this.button_verify_loadFromBarcodeFile.TabIndex = 9;
            this.button_verify_loadFromBarcodeFile.Text = "从条码号文件(&F)...";
            this.button_verify_loadFromBarcodeFile.UseVisualStyleBackColor = true;
            // 
            // checkBox_verify_autoUppercaseBarcode
            // 
            this.checkBox_verify_autoUppercaseBarcode.AutoSize = true;
            this.checkBox_verify_autoUppercaseBarcode.Location = new System.Drawing.Point(91, 33);
            this.checkBox_verify_autoUppercaseBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_verify_autoUppercaseBarcode.Name = "checkBox_verify_autoUppercaseBarcode";
            this.checkBox_verify_autoUppercaseBarcode.Size = new System.Drawing.Size(222, 16);
            this.checkBox_verify_autoUppercaseBarcode.TabIndex = 8;
            this.checkBox_verify_autoUppercaseBarcode.Text = "自动把输入的条码字符串转为大写(&U)";
            this.checkBox_verify_autoUppercaseBarcode.UseVisualStyleBackColor = true;
            // 
            // button_verify_load
            // 
            this.button_verify_load.Location = new System.Drawing.Point(227, 6);
            this.button_verify_load.Margin = new System.Windows.Forms.Padding(2);
            this.button_verify_load.Name = "button_verify_load";
            this.button_verify_load.Size = new System.Drawing.Size(56, 22);
            this.button_verify_load.TabIndex = 2;
            this.button_verify_load.Text = "提交(&S)";
            this.button_verify_load.UseVisualStyleBackColor = true;
            // 
            // textBox_verify_itemBarcode
            // 
            this.textBox_verify_itemBarcode.Location = new System.Drawing.Point(91, 5);
            this.textBox_verify_itemBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_verify_itemBarcode.Name = "textBox_verify_itemBarcode";
            this.textBox_verify_itemBarcode.Size = new System.Drawing.Size(132, 21);
            this.textBox_verify_itemBarcode.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "册条码号(&B):";
            // 
            // tabPage_print
            // 
            this.tabPage_print.AutoScroll = true;
            this.tabPage_print.Controls.Add(this.button_print_option);
            this.tabPage_print.Controls.Add(this.button_print_printList);
            this.tabPage_print.Location = new System.Drawing.Point(4, 22);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(419, 83);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = "打印";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print_option
            // 
            this.button_print_option.Location = new System.Drawing.Point(162, 5);
            this.button_print_option.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_option.Name = "button_print_option";
            this.button_print_option.Size = new System.Drawing.Size(152, 22);
            this.button_print_option.TabIndex = 2;
            this.button_print_option.Text = "打印配置(&O)...";
            this.button_print_option.UseVisualStyleBackColor = true;
            // 
            // button_print_printList
            // 
            this.button_print_printList.Location = new System.Drawing.Point(5, 5);
            this.button_print_printList.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_printList.Name = "button_print_printList";
            this.button_print_printList.Size = new System.Drawing.Size(152, 22);
            this.button_print_printList.TabIndex = 0;
            this.button_print_printList.Text = "打印各种清单(&P)...";
            this.button_print_printList.UseVisualStyleBackColor = true;
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(343, 115);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(83, 22);
            this.button_next.TabIndex = 0;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            // 
            // dpTable1
            // 
            this.dpTable1.AutoDocCenter = false;
            this.dpTable1.BackColor = System.Drawing.SystemColors.Window;
            this.dpTable1.Columns.Add(this.dpColumn_icon1);
            this.dpTable1.Columns.Add(this.dpColumn_icon2);
            this.dpTable1.Columns.Add(this.dpColumn_barcode);
            this.dpTable1.Columns.Add(this.dpColumn_summary);
            this.dpTable1.Columns.Add(this.dpColumn_recpath);
            this.dpTable1.Columns.Add(this.dpColumn_biblioRecPath);
            this.dpTable1.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable1.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable1.DocumentBorderColor = System.Drawing.Color.Transparent;
            this.dpTable1.DocumentOrgX = ((long)(0));
            this.dpTable1.DocumentOrgY = ((long)(0));
            this.dpTable1.DocumentShadowColor = System.Drawing.Color.Transparent;
            this.dpTable1.FocusedItem = null;
            this.dpTable1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dpTable1.FullRowSelect = true;
            this.dpTable1.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable1.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable1.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable1.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable1.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable1.Location = new System.Drawing.Point(0, 0);
            this.dpTable1.Name = "dpTable1";
            this.dpTable1.Size = new System.Drawing.Size(426, 141);
            this.dpTable1.TabIndex = 0;
            this.dpTable1.Text = "dpTable1";
            // 
            // dpColumn_icon1
            // 
            this.dpColumn_icon1.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_icon1.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_icon1.Font = null;
            this.dpColumn_icon1.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_icon1.Width = 50;
            // 
            // dpColumn_icon2
            // 
            this.dpColumn_icon2.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_icon2.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_icon2.Font = null;
            this.dpColumn_icon2.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_icon2.Width = 50;
            // 
            // dpColumn_barcode
            // 
            this.dpColumn_barcode.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_barcode.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_barcode.Font = null;
            this.dpColumn_barcode.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_barcode.Text = "册条码号";
            // 
            // dpColumn_summary
            // 
            this.dpColumn_summary.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_summary.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_summary.Font = null;
            this.dpColumn_summary.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_summary.Text = "书目摘要";
            this.dpColumn_summary.Width = 200;
            // 
            // dpColumn_recpath
            // 
            this.dpColumn_recpath.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_recpath.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_recpath.Font = null;
            this.dpColumn_recpath.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_recpath.Text = "册记录路径";
            this.dpColumn_recpath.Width = 130;
            // 
            // dpColumn_biblioRecPath
            // 
            this.dpColumn_biblioRecPath.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_biblioRecPath.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_biblioRecPath.Font = null;
            this.dpColumn_biblioRecPath.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_biblioRecPath.Text = "书目记录路径";
            this.dpColumn_biblioRecPath.Width = 130;
            // 
            // NewInventoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 310);
            this.Controls.Add(this.splitContainer_main);
            this.Name = "NewInventoryForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "NewInventoryForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewInventoryForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NewInventoryForm_FormClosed);
            this.Load += new System.EventHandler(this.NewInventoryForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.panel_up.ResumeLayout(false);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_load.ResumeLayout(false);
            this.tabPage_load.PerformLayout();
            this.tabPage_verify.ResumeLayout(false);
            this.tabPage_verify.PerformLayout();
            this.tabPage_print.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Panel panel_up;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_load;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_load_type;
        private System.Windows.Forms.Button button_load_loadFromRecPathFile;
        private System.Windows.Forms.Button button_load_loadFromBarcodeFile;
        private System.Windows.Forms.TabPage tabPage_verify;
        private System.Windows.Forms.Button button_verify_loadFromBarcodeFile;
        private System.Windows.Forms.CheckBox checkBox_verify_autoUppercaseBarcode;
        private System.Windows.Forms.Button button_verify_load;
        private System.Windows.Forms.TextBox textBox_verify_itemBarcode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_print_option;
        private System.Windows.Forms.Button button_print_printList;
        private System.Windows.Forms.Button button_next;
        private DigitalPlatform.CommonControl.DpTable dpTable1;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_icon1;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_icon2;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_barcode;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_summary;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_recpath;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_biblioRecPath;
    }
}