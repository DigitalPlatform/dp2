namespace dp2Circulation
{
    partial class PrintReaderInfoInputDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintReaderInfoInputDialog));
            this.checkBox_range_inPeriod = new System.Windows.Forms.CheckBox();
            this.checkBox_range_outofPeriod = new System.Windows.Forms.CheckBox();
            this.checkBox_range_hasOverdueItem = new System.Windows.Forms.CheckBox();
            this.checkBox_range_hasBorrowItem = new System.Windows.Forms.CheckBox();
            this.checkBox_range_noBorrowAndOverdueItem = new System.Windows.Forms.CheckBox();
            this.radioButton_range_part = new System.Windows.Forms.RadioButton();
            this.radioButton_range_all = new System.Windows.Forms.RadioButton();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_outputRange = new System.Windows.Forms.TabPage();
            this.tabPage_printOption = new System.Windows.Forms.TabPage();
            this.textBox_maxSummaryChars = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_tableTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_pageFooter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_pageHeader = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_linesPerPage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_sortOption = new System.Windows.Forms.TabPage();
            this.listBox_sortDef = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_outputRange.SuspendLayout();
            this.tabPage_printOption.SuspendLayout();
            this.tabPage_sortOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_range_inPeriod
            // 
            this.checkBox_range_inPeriod.AutoSize = true;
            this.checkBox_range_inPeriod.Location = new System.Drawing.Point(54, 95);
            this.checkBox_range_inPeriod.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_range_inPeriod.Name = "checkBox_range_inPeriod";
            this.checkBox_range_inPeriod.Size = new System.Drawing.Size(60, 16);
            this.checkBox_range_inPeriod.TabIndex = 6;
            this.checkBox_range_inPeriod.Text = "未超期";
            this.checkBox_range_inPeriod.UseVisualStyleBackColor = true;
            this.checkBox_range_inPeriod.CheckedChanged += new System.EventHandler(this.checkBox_range_inPeriod_CheckedChanged);
            // 
            // checkBox_range_outofPeriod
            // 
            this.checkBox_range_outofPeriod.AutoSize = true;
            this.checkBox_range_outofPeriod.Location = new System.Drawing.Point(54, 115);
            this.checkBox_range_outofPeriod.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_range_outofPeriod.Name = "checkBox_range_outofPeriod";
            this.checkBox_range_outofPeriod.Size = new System.Drawing.Size(60, 16);
            this.checkBox_range_outofPeriod.TabIndex = 5;
            this.checkBox_range_outofPeriod.Text = "已超期";
            this.checkBox_range_outofPeriod.UseVisualStyleBackColor = true;
            this.checkBox_range_outofPeriod.CheckedChanged += new System.EventHandler(this.checkBox_range_outofPeriod_CheckedChanged);
            // 
            // checkBox_range_hasOverdueItem
            // 
            this.checkBox_range_hasOverdueItem.AutoSize = true;
            this.checkBox_range_hasOverdueItem.Location = new System.Drawing.Point(39, 135);
            this.checkBox_range_hasOverdueItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_range_hasOverdueItem.Name = "checkBox_range_hasOverdueItem";
            this.checkBox_range_hasOverdueItem.Size = new System.Drawing.Size(84, 16);
            this.checkBox_range_hasOverdueItem.TabIndex = 4;
            this.checkBox_range_hasOverdueItem.Text = "有违约金的";
            this.checkBox_range_hasOverdueItem.UseVisualStyleBackColor = true;
            // 
            // checkBox_range_hasBorrowItem
            // 
            this.checkBox_range_hasBorrowItem.AutoSize = true;
            this.checkBox_range_hasBorrowItem.Location = new System.Drawing.Point(39, 75);
            this.checkBox_range_hasBorrowItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_range_hasBorrowItem.Name = "checkBox_range_hasBorrowItem";
            this.checkBox_range_hasBorrowItem.Size = new System.Drawing.Size(84, 16);
            this.checkBox_range_hasBorrowItem.TabIndex = 3;
            this.checkBox_range_hasBorrowItem.Text = "有在借册的";
            this.checkBox_range_hasBorrowItem.UseVisualStyleBackColor = true;
            this.checkBox_range_hasBorrowItem.CheckedChanged += new System.EventHandler(this.checkBox_hasBorrowItem_CheckedChanged);
            // 
            // checkBox_range_noBorrowAndOverdueItem
            // 
            this.checkBox_range_noBorrowAndOverdueItem.AutoSize = true;
            this.checkBox_range_noBorrowAndOverdueItem.Location = new System.Drawing.Point(39, 54);
            this.checkBox_range_noBorrowAndOverdueItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_range_noBorrowAndOverdueItem.Name = "checkBox_range_noBorrowAndOverdueItem";
            this.checkBox_range_noBorrowAndOverdueItem.Size = new System.Drawing.Size(144, 16);
            this.checkBox_range_noBorrowAndOverdueItem.TabIndex = 2;
            this.checkBox_range_noBorrowAndOverdueItem.Text = "无 在借册和违约金 的";
            this.checkBox_range_noBorrowAndOverdueItem.UseVisualStyleBackColor = true;
            // 
            // radioButton_range_part
            // 
            this.radioButton_range_part.AutoSize = true;
            this.radioButton_range_part.Location = new System.Drawing.Point(5, 34);
            this.radioButton_range_part.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_range_part.Name = "radioButton_range_part";
            this.radioButton_range_part.Size = new System.Drawing.Size(65, 16);
            this.radioButton_range_part.TabIndex = 1;
            this.radioButton_range_part.Text = "部分(&P)";
            this.radioButton_range_part.UseVisualStyleBackColor = true;
            this.radioButton_range_part.CheckedChanged += new System.EventHandler(this.radioButton_range_part_CheckedChanged);
            // 
            // radioButton_range_all
            // 
            this.radioButton_range_all.AutoSize = true;
            this.radioButton_range_all.Checked = true;
            this.radioButton_range_all.Location = new System.Drawing.Point(5, 13);
            this.radioButton_range_all.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_range_all.Name = "radioButton_range_all";
            this.radioButton_range_all.Size = new System.Drawing.Size(65, 16);
            this.radioButton_range_all.TabIndex = 0;
            this.radioButton_range_all.TabStop = true;
            this.radioButton_range_all.Text = "全部(&A)";
            this.radioButton_range_all.UseVisualStyleBackColor = true;
            this.radioButton_range_all.CheckedChanged += new System.EventHandler(this.radioButton_range_all_CheckedChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(266, 218);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(205, 218);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_outputRange);
            this.tabControl_main.Controls.Add(this.tabPage_printOption);
            this.tabControl_main.Controls.Add(this.tabPage_sortOption);
            this.tabControl_main.Location = new System.Drawing.Point(9, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(313, 204);
            this.tabControl_main.TabIndex = 8;
            // 
            // tabPage_outputRange
            // 
            this.tabPage_outputRange.Controls.Add(this.checkBox_range_inPeriod);
            this.tabPage_outputRange.Controls.Add(this.checkBox_range_noBorrowAndOverdueItem);
            this.tabPage_outputRange.Controls.Add(this.checkBox_range_outofPeriod);
            this.tabPage_outputRange.Controls.Add(this.radioButton_range_all);
            this.tabPage_outputRange.Controls.Add(this.checkBox_range_hasOverdueItem);
            this.tabPage_outputRange.Controls.Add(this.radioButton_range_part);
            this.tabPage_outputRange.Controls.Add(this.checkBox_range_hasBorrowItem);
            this.tabPage_outputRange.Location = new System.Drawing.Point(4, 22);
            this.tabPage_outputRange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_outputRange.Name = "tabPage_outputRange";
            this.tabPage_outputRange.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_outputRange.Size = new System.Drawing.Size(305, 178);
            this.tabPage_outputRange.TabIndex = 0;
            this.tabPage_outputRange.Text = "输出范围";
            this.tabPage_outputRange.UseVisualStyleBackColor = true;
            // 
            // tabPage_printOption
            // 
            this.tabPage_printOption.Controls.Add(this.textBox_maxSummaryChars);
            this.tabPage_printOption.Controls.Add(this.label5);
            this.tabPage_printOption.Controls.Add(this.textBox_tableTitle);
            this.tabPage_printOption.Controls.Add(this.label4);
            this.tabPage_printOption.Controls.Add(this.textBox_pageFooter);
            this.tabPage_printOption.Controls.Add(this.label3);
            this.tabPage_printOption.Controls.Add(this.textBox_pageHeader);
            this.tabPage_printOption.Controls.Add(this.label2);
            this.tabPage_printOption.Controls.Add(this.textBox_linesPerPage);
            this.tabPage_printOption.Controls.Add(this.label1);
            this.tabPage_printOption.Location = new System.Drawing.Point(4, 22);
            this.tabPage_printOption.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_printOption.Name = "tabPage_printOption";
            this.tabPage_printOption.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_printOption.Size = new System.Drawing.Size(305, 178);
            this.tabPage_printOption.TabIndex = 1;
            this.tabPage_printOption.Text = "打印特性";
            this.tabPage_printOption.UseVisualStyleBackColor = true;
            // 
            // textBox_maxSummaryChars
            // 
            this.textBox_maxSummaryChars.Location = new System.Drawing.Point(106, 130);
            this.textBox_maxSummaryChars.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_maxSummaryChars.Name = "textBox_maxSummaryChars";
            this.textBox_maxSummaryChars.Size = new System.Drawing.Size(84, 21);
            this.textBox_maxSummaryChars.TabIndex = 19;
            this.textBox_maxSummaryChars.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_maxSummaryChars_Validating);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 133);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 18;
            this.label5.Text = "摘要字数上限(&S):";
            // 
            // textBox_tableTitle
            // 
            this.textBox_tableTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tableTitle.Location = new System.Drawing.Point(106, 70);
            this.textBox_tableTitle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_tableTitle.Name = "textBox_tableTitle";
            this.textBox_tableTitle.Size = new System.Drawing.Size(197, 21);
            this.textBox_tableTitle.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 73);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "表格标题(&T):";
            // 
            // textBox_pageFooter
            // 
            this.textBox_pageFooter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_pageFooter.Location = new System.Drawing.Point(106, 34);
            this.textBox_pageFooter.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_pageFooter.Name = "textBox_pageFooter";
            this.textBox_pageFooter.Size = new System.Drawing.Size(197, 21);
            this.textBox_pageFooter.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 36);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "页脚文字(&F):";
            // 
            // textBox_pageHeader
            // 
            this.textBox_pageHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_pageHeader.Location = new System.Drawing.Point(106, 9);
            this.textBox_pageHeader.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_pageHeader.Name = "textBox_pageHeader";
            this.textBox_pageHeader.Size = new System.Drawing.Size(197, 21);
            this.textBox_pageHeader.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 11);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "页眉文字(&H):";
            // 
            // textBox_linesPerPage
            // 
            this.textBox_linesPerPage.Location = new System.Drawing.Point(106, 106);
            this.textBox_linesPerPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_linesPerPage.Name = "textBox_linesPerPage";
            this.textBox_linesPerPage.Size = new System.Drawing.Size(84, 21);
            this.textBox_linesPerPage.TabIndex = 17;
            this.textBox_linesPerPage.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_linesPerPage_Validating);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 108);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "每页行数(&L):";
            // 
            // tabPage_sortOption
            // 
            this.tabPage_sortOption.Controls.Add(this.listBox_sortDef);
            this.tabPage_sortOption.Controls.Add(this.label6);
            this.tabPage_sortOption.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sortOption.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_sortOption.Name = "tabPage_sortOption";
            this.tabPage_sortOption.Size = new System.Drawing.Size(305, 178);
            this.tabPage_sortOption.TabIndex = 2;
            this.tabPage_sortOption.Text = "排序特性";
            this.tabPage_sortOption.UseVisualStyleBackColor = true;
            // 
            // listBox_sortDef
            // 
            this.listBox_sortDef.FormattingEnabled = true;
            this.listBox_sortDef.ItemHeight = 12;
            this.listBox_sortDef.Items.AddRange(new object[] {
            "单位,证条码:  1,-1",
            "证状态,证条码: 5,-1",
            "证条码: -1"});
            this.listBox_sortDef.Location = new System.Drawing.Point(2, 28);
            this.listBox_sortDef.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBox_sortDef.MultiColumn = true;
            this.listBox_sortDef.Name = "listBox_sortDef";
            this.listBox_sortDef.ScrollAlwaysVisible = true;
            this.listBox_sortDef.Size = new System.Drawing.Size(303, 124);
            this.listBox_sortDef.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(2, 14);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "排序方式(&S):";
            // 
            // PrintReaderInfoInputDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(331, 250);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "PrintReaderInfoInputDialog";
            this.ShowInTaskbar = false;
            this.Text = "打印读者信息 -- 指定输出特性";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrintReaderInfoInputDialog_FormClosed);
            this.Load += new System.EventHandler(this.PrintReaderInfoInputDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_outputRange.ResumeLayout(false);
            this.tabPage_outputRange.PerformLayout();
            this.tabPage_printOption.ResumeLayout(false);
            this.tabPage_printOption.PerformLayout();
            this.tabPage_sortOption.ResumeLayout(false);
            this.tabPage_sortOption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_range_hasBorrowItem;
        private System.Windows.Forms.CheckBox checkBox_range_noBorrowAndOverdueItem;
        private System.Windows.Forms.RadioButton radioButton_range_part;
        private System.Windows.Forms.RadioButton radioButton_range_all;
        private System.Windows.Forms.CheckBox checkBox_range_hasOverdueItem;
        private System.Windows.Forms.CheckBox checkBox_range_outofPeriod;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_range_inPeriod;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_outputRange;
        private System.Windows.Forms.TabPage tabPage_printOption;
        private System.Windows.Forms.TextBox textBox_maxSummaryChars;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_tableTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_pageFooter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_pageHeader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_linesPerPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_sortOption;
        private System.Windows.Forms.ListBox listBox_sortDef;
        private System.Windows.Forms.Label label6;
    }
}