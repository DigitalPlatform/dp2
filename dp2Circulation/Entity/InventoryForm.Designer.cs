namespace dp2Circulation
{
    partial class InventoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventoryForm));
            this.tableLayoutPanel_scan = new System.Windows.Forms.TableLayoutPanel();
            this.panel_input = new System.Windows.Forms.Panel();
            this.colorSummaryControl1 = new dp2Circulation.ColorSummaryControl();
            this.textBox_input = new System.Windows.Forms.TextBox();
            this.label_barcode_type = new System.Windows.Forms.Label();
            this.label_input_message = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dpTable_tasks = new DigitalPlatform.CommonControl.DpTable();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel_currentPatron = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_openPatronSummaryWindow = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_enableHanzi = new System.Windows.Forms.ToolStripButton();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.tabPage_scan = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tabComboBox_inputBatchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.tableLayoutPanel_scan.SuspendLayout();
            this.panel_input.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.toolStrip_main.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            this.tabPage_scan.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_scan
            // 
            this.tableLayoutPanel_scan.ColumnCount = 1;
            this.tableLayoutPanel_scan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_scan.Controls.Add(this.panel_input, 0, 1);
            this.tableLayoutPanel_scan.Controls.Add(this.dpTable_tasks, 0, 0);
            this.tableLayoutPanel_scan.Controls.Add(this.toolStrip_main, 0, 2);
            this.tableLayoutPanel_scan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_scan.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel_scan.Name = "tableLayoutPanel_scan";
            this.tableLayoutPanel_scan.RowCount = 4;
            this.tableLayoutPanel_scan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_scan.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_scan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_scan.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_scan.Size = new System.Drawing.Size(416, 246);
            this.tableLayoutPanel_scan.TabIndex = 1;
            // 
            // panel_input
            // 
            this.panel_input.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel_input.BackColor = System.Drawing.SystemColors.Window;
            this.panel_input.Controls.Add(this.colorSummaryControl1);
            this.panel_input.Controls.Add(this.textBox_input);
            this.panel_input.Controls.Add(this.label_barcode_type);
            this.panel_input.Controls.Add(this.label_input_message);
            this.panel_input.Controls.Add(this.pictureBox1);
            this.panel_input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_input.Location = new System.Drawing.Point(3, 141);
            this.panel_input.Name = "panel_input";
            this.panel_input.Size = new System.Drawing.Size(410, 82);
            this.panel_input.TabIndex = 1;
            // 
            // colorSummaryControl1
            // 
            this.colorSummaryControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorSummaryControl1.ColorList = "";
            this.colorSummaryControl1.Location = new System.Drawing.Point(0, 69);
            this.colorSummaryControl1.Name = "colorSummaryControl1";
            this.colorSummaryControl1.Size = new System.Drawing.Size(410, 10);
            this.colorSummaryControl1.TabIndex = 5;
            // 
            // textBox_input
            // 
            this.textBox_input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_input.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_input.Font = new System.Drawing.Font("宋体", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_input.Location = new System.Drawing.Point(3, 25);
            this.textBox_input.Name = "textBox_input";
            this.textBox_input.Size = new System.Drawing.Size(330, 42);
            this.textBox_input.TabIndex = 1;
            // 
            // label_barcode_type
            // 
            this.label_barcode_type.ImageIndex = 0;
            this.label_barcode_type.Location = new System.Drawing.Point(-2, 6);
            this.label_barcode_type.Name = "label_barcode_type";
            this.label_barcode_type.Size = new System.Drawing.Size(26, 20);
            this.label_barcode_type.TabIndex = 4;
            // 
            // label_input_message
            // 
            this.label_input_message.AutoSize = true;
            this.label_input_message.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_input_message.ImageIndex = 0;
            this.label_input_message.Location = new System.Drawing.Point(25, 10);
            this.label_input_message.Name = "label_input_message";
            this.label_input_message.Size = new System.Drawing.Size(29, 12);
            this.label_input_message.TabIndex = 3;
            this.label_input_message.Text = "test";
            this.label_input_message.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(342, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(68, 67);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // dpTable_tasks
            // 
            this.dpTable_tasks.AutoDocCenter = true;
            this.dpTable_tasks.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable_tasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dpTable_tasks.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_tasks.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_tasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable_tasks.DocumentBorderColor = System.Drawing.Color.Transparent;
            this.dpTable_tasks.DocumentOrgX = ((long)(0));
            this.dpTable_tasks.DocumentOrgY = ((long)(0));
            this.dpTable_tasks.DocumentShadowColor = System.Drawing.Color.Transparent;
            this.dpTable_tasks.FocusedItem = null;
            this.dpTable_tasks.Font = new System.Drawing.Font("宋体", 10F);
            this.dpTable_tasks.FullRowSelect = true;
            this.dpTable_tasks.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_tasks.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_tasks.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_tasks.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_tasks.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_tasks.LineDistance = 10;
            this.dpTable_tasks.Location = new System.Drawing.Point(3, 3);
            this.dpTable_tasks.MaxTextHeight = 150;
            this.dpTable_tasks.Name = "dpTable_tasks";
            this.dpTable_tasks.Padding = new System.Windows.Forms.Padding(8);
            this.dpTable_tasks.Size = new System.Drawing.Size(410, 132);
            this.dpTable_tasks.TabIndex = 1;
            this.dpTable_tasks.Text = "dpTable1";
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel_currentPatron,
            this.toolStripButton_openPatronSummaryWindow,
            this.toolStripButton_enableHanzi});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 226);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Size = new System.Drawing.Size(416, 20);
            this.toolStrip_main.TabIndex = 2;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolStripLabel_currentPatron
            // 
            this.toolStripLabel_currentPatron.Name = "toolStripLabel_currentPatron";
            this.toolStripLabel_currentPatron.Size = new System.Drawing.Size(0, 17);
            this.toolStripLabel_currentPatron.ToolTipText = "当前读者证条码号";
            // 
            // toolStripButton_openPatronSummaryWindow
            // 
            this.toolStripButton_openPatronSummaryWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_openPatronSummaryWindow.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_openPatronSummaryWindow.Image")));
            this.toolStripButton_openPatronSummaryWindow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_openPatronSummaryWindow.Name = "toolStripButton_openPatronSummaryWindow";
            this.toolStripButton_openPatronSummaryWindow.Size = new System.Drawing.Size(23, 17);
            this.toolStripButton_openPatronSummaryWindow.Text = "打开读者摘要窗口";
            // 
            // toolStripButton_enableHanzi
            // 
            this.toolStripButton_enableHanzi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_enableHanzi.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_enableHanzi.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_enableHanzi.Image")));
            this.toolStripButton_enableHanzi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_enableHanzi.Name = "toolStripButton_enableHanzi";
            this.toolStripButton_enableHanzi.Size = new System.Drawing.Size(24, 17);
            this.toolStripButton_enableHanzi.Text = "汉";
            this.toolStripButton_enableHanzi.ToolTipText = "是否允许输入汉字";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_start);
            this.tabControl_main.Controls.Add(this.tabPage_scan);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(430, 278);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_start
            // 
            this.tabPage_start.Controls.Add(this.tabComboBox_inputBatchNo);
            this.tabPage_start.Controls.Add(this.label1);
            this.tabPage_start.Location = new System.Drawing.Point(4, 22);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_start.Size = new System.Drawing.Size(422, 252);
            this.tabPage_start.TabIndex = 0;
            this.tabPage_start.Text = "开始";
            this.tabPage_start.UseVisualStyleBackColor = true;
            // 
            // tabPage_scan
            // 
            this.tabPage_scan.Controls.Add(this.tableLayoutPanel_scan);
            this.tabPage_scan.Location = new System.Drawing.Point(4, 22);
            this.tabPage_scan.Name = "tabPage_scan";
            this.tabPage_scan.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_scan.Size = new System.Drawing.Size(422, 252);
            this.tabPage_scan.TabIndex = 1;
            this.tabPage_scan.Text = "扫入";
            this.tabPage_scan.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "批次号(&B):";
            // 
            // tabComboBox_inputBatchNo
            // 
            this.tabComboBox_inputBatchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_inputBatchNo.FormattingEnabled = true;
            this.tabComboBox_inputBatchNo.LeftFontStyle = System.Drawing.FontStyle.Bold;
            this.tabComboBox_inputBatchNo.Location = new System.Drawing.Point(94, 16);
            this.tabComboBox_inputBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.tabComboBox_inputBatchNo.Name = "tabComboBox_inputBatchNo";
            this.tabComboBox_inputBatchNo.RightFontStyle = System.Drawing.FontStyle.Italic;
            this.tabComboBox_inputBatchNo.Size = new System.Drawing.Size(140, 22);
            this.tabComboBox_inputBatchNo.TabIndex = 6;
            // 
            // InventoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 278);
            this.Controls.Add(this.tabControl_main);
            this.Name = "InventoryForm";
            this.Text = "InventoryForm";
            this.tableLayoutPanel_scan.ResumeLayout(false);
            this.tableLayoutPanel_scan.PerformLayout();
            this.panel_input.ResumeLayout(false);
            this.panel_input.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.tabPage_start.PerformLayout();
            this.tabPage_scan.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_scan;
        private System.Windows.Forms.Panel panel_input;
        private ColorSummaryControl colorSummaryControl1;
        private System.Windows.Forms.TextBox textBox_input;
        private System.Windows.Forms.Label label_barcode_type;
        private System.Windows.Forms.Label label_input_message;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DigitalPlatform.CommonControl.DpTable dpTable_tasks;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_currentPatron;
        private System.Windows.Forms.ToolStripButton toolStripButton_openPatronSummaryWindow;
        private System.Windows.Forms.ToolStripButton toolStripButton_enableHanzi;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.TabPage tabPage_scan;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_inputBatchNo;
    }
}