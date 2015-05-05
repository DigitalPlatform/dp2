namespace dp2Circulation
{
    partial class CardPrintForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CardPrintForm));
            this.button_print = new System.Windows.Forms.Button();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_cardFile = new System.Windows.Forms.TabPage();
            this.checkBox_cardFile_indent = new System.Windows.Forms.CheckBox();
            this.textBox_cardFile_content = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_cardFile_findCardFilename = new System.Windows.Forms.Button();
            this.textBox_cardFile_cardFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_printPreview = new System.Windows.Forms.Button();
            this.checkBox_testingGrid = new System.Windows.Forms.CheckBox();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.tabControl_main.SuspendLayout();
            this.tabPage_cardFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_print
            // 
            this.button_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_print.Location = new System.Drawing.Point(206, 281);
            this.button_print.Margin = new System.Windows.Forms.Padding(2);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(88, 22);
            this.button_print.TabIndex = 2;
            this.button_print.Text = "打印(&P)...";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
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
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_cardFile);
            this.tabControl_main.Location = new System.Drawing.Point(0, 11);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(404, 246);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_cardFile
            // 
            this.tabPage_cardFile.Controls.Add(this.checkBox_cardFile_indent);
            this.tabPage_cardFile.Controls.Add(this.textBox_cardFile_content);
            this.tabPage_cardFile.Controls.Add(this.label2);
            this.tabPage_cardFile.Controls.Add(this.button_cardFile_findCardFilename);
            this.tabPage_cardFile.Controls.Add(this.textBox_cardFile_cardFilename);
            this.tabPage_cardFile.Controls.Add(this.label1);
            this.tabPage_cardFile.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage_cardFile.Location = new System.Drawing.Point(4, 22);
            this.tabPage_cardFile.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_cardFile.Name = "tabPage_cardFile";
            this.tabPage_cardFile.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_cardFile.Size = new System.Drawing.Size(396, 220);
            this.tabPage_cardFile.TabIndex = 0;
            this.tabPage_cardFile.Text = "卡片文件";
            this.tabPage_cardFile.UseVisualStyleBackColor = true;
            // 
            // checkBox_cardFile_indent
            // 
            this.checkBox_cardFile_indent.AutoSize = true;
            this.checkBox_cardFile_indent.Location = new System.Drawing.Point(116, 38);
            this.checkBox_cardFile_indent.Name = "checkBox_cardFile_indent";
            this.checkBox_cardFile_indent.Size = new System.Drawing.Size(126, 16);
            this.checkBox_cardFile_indent.TabIndex = 4;
            this.checkBox_cardFile_indent.Text = "以缩进格式显示(&I)";
            this.checkBox_cardFile_indent.UseVisualStyleBackColor = true;
            this.checkBox_cardFile_indent.CheckedChanged += new System.EventHandler(this.checkBox_cardFile_indent_CheckedChanged);
            // 
            // textBox_cardFile_content
            // 
            this.textBox_cardFile_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cardFile_content.Location = new System.Drawing.Point(0, 57);
            this.textBox_cardFile_content.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_cardFile_content.MaxLength = 0;
            this.textBox_cardFile_content.Multiline = true;
            this.textBox_cardFile_content.Name = "textBox_cardFile_content";
            this.textBox_cardFile_content.ReadOnly = true;
            this.textBox_cardFile_content.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_cardFile_content.Size = new System.Drawing.Size(399, 168);
            this.textBox_cardFile_content.TabIndex = 5;
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
            // button_cardFile_findCardFilename
            // 
            this.button_cardFile_findCardFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cardFile_findCardFilename.Location = new System.Drawing.Point(354, 11);
            this.button_cardFile_findCardFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_cardFile_findCardFilename.Name = "button_cardFile_findCardFilename";
            this.button_cardFile_findCardFilename.Size = new System.Drawing.Size(38, 22);
            this.button_cardFile_findCardFilename.TabIndex = 2;
            this.button_cardFile_findCardFilename.Text = "...";
            this.button_cardFile_findCardFilename.UseVisualStyleBackColor = true;
            this.button_cardFile_findCardFilename.Click += new System.EventHandler(this.button_cardFile_findCardFilename_Click);
            // 
            // textBox_cardFile_cardFilename
            // 
            this.textBox_cardFile_cardFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cardFile_cardFilename.Location = new System.Drawing.Point(116, 12);
            this.textBox_cardFile_cardFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_cardFile_cardFilename.Name = "textBox_cardFile_cardFilename";
            this.textBox_cardFile_cardFilename.Size = new System.Drawing.Size(234, 21);
            this.textBox_cardFile_cardFilename.TabIndex = 1;
            this.textBox_cardFile_cardFilename.TextChanged += new System.EventHandler(this.textBox_cardFile_cardFilename_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "卡片文件名(&C):";
            // 
            // button_printPreview
            // 
            this.button_printPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_printPreview.Location = new System.Drawing.Point(299, 281);
            this.button_printPreview.Margin = new System.Windows.Forms.Padding(2);
            this.button_printPreview.Name = "button_printPreview";
            this.button_printPreview.Size = new System.Drawing.Size(105, 22);
            this.button_printPreview.TabIndex = 3;
            this.button_printPreview.Text = "打印预览(&V)...";
            this.button_printPreview.UseVisualStyleBackColor = true;
            this.button_printPreview.Click += new System.EventHandler(this.button_printPreview_Click);
            // 
            // checkBox_testingGrid
            // 
            this.checkBox_testingGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_testingGrid.AutoSize = true;
            this.checkBox_testingGrid.Location = new System.Drawing.Point(0, 287);
            this.checkBox_testingGrid.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_testingGrid.Name = "checkBox_testingGrid";
            this.checkBox_testingGrid.Size = new System.Drawing.Size(102, 16);
            this.checkBox_testingGrid.TabIndex = 1;
            this.checkBox_testingGrid.Text = "打印调试线(&T)";
            this.checkBox_testingGrid.UseVisualStyleBackColor = true;
            // 
            // progressBar_records
            // 
            this.progressBar_records.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar_records.Location = new System.Drawing.Point(4, 261);
            this.progressBar_records.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar_records.Name = "progressBar_records";
            this.progressBar_records.Size = new System.Drawing.Size(396, 16);
            this.progressBar_records.TabIndex = 4;
            // 
            // CardPrintForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 313);
            this.Controls.Add(this.progressBar_records);
            this.Controls.Add(this.button_print);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_printPreview);
            this.Controls.Add(this.checkBox_testingGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CardPrintForm";
            this.Text = "卡片打印";
            this.Activated += new System.EventHandler(this.CardPrintForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CardPrintForm_FormClosed);
            this.Load += new System.EventHandler(this.CardPrintForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_cardFile.ResumeLayout(false);
            this.tabPage_cardFile.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_cardFile;
        private System.Windows.Forms.TextBox textBox_cardFile_content;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_cardFile_findCardFilename;
        private System.Windows.Forms.TextBox textBox_cardFile_cardFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_printPreview;
        private System.Windows.Forms.CheckBox checkBox_testingGrid;
        private System.Windows.Forms.CheckBox checkBox_cardFile_indent;
        private System.Windows.Forms.ProgressBar progressBar_records;
    }
}