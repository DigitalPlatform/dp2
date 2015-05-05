namespace DigitalPlatform.Script
{
    partial class GenerateDataForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenerateDataForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dock = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_excute = new System.Windows.Forms.Button();
            this.checkBox_autoRun = new System.Windows.Forms.CheckBox();
            this.ActionTable = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_name = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_shortcutKey = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_comment = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_entry = new DigitalPlatform.CommonControl.DpColumn();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dock});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(331, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_dock
            // 
            this.toolStripButton_dock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_dock.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dock.Image")));
            this.toolStripButton_dock.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_dock.Name = "toolStripButton_dock";
            this.toolStripButton_dock.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_dock.Text = "停靠到固定面板";
            this.toolStripButton_dock.Click += new System.EventHandler(this.toolStripButton_dock_Click);
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel_main.Controls.Add(this.ActionTable, 0, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(331, 239);
            this.tableLayoutPanel_main.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.button_excute);
            this.panel1.Controls.Add(this.checkBox_autoRun);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 210);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(325, 26);
            this.panel1.TabIndex = 5;
            // 
            // button_excute
            // 
            this.button_excute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_excute.AutoSize = true;
            this.button_excute.Location = new System.Drawing.Point(250, 0);
            this.button_excute.Name = "button_excute";
            this.button_excute.Size = new System.Drawing.Size(75, 23);
            this.button_excute.TabIndex = 1;
            this.button_excute.Text = "执行";
            this.button_excute.UseVisualStyleBackColor = true;
            this.button_excute.Click += new System.EventHandler(this.button_excute_Click);
            // 
            // checkBox_autoRun
            // 
            this.checkBox_autoRun.AutoSize = true;
            this.checkBox_autoRun.Location = new System.Drawing.Point(0, 4);
            this.checkBox_autoRun.Name = "checkBox_autoRun";
            this.checkBox_autoRun.Size = new System.Drawing.Size(138, 16);
            this.checkBox_autoRun.TabIndex = 0;
            this.checkBox_autoRun.Text = "自动执行加亮事项(&A)";
            this.checkBox_autoRun.UseVisualStyleBackColor = true;
            // 
            // ActionTable
            // 
            this.ActionTable.AutoDocCenter = true;
            this.ActionTable.BackColor = System.Drawing.SystemColors.Window;
            this.ActionTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ActionTable.Columns.Add(this.dpColumn_name);
            this.ActionTable.Columns.Add(this.dpColumn_shortcutKey);
            this.ActionTable.Columns.Add(this.dpColumn_comment);
            this.ActionTable.Columns.Add(this.dpColumn_entry);
            this.ActionTable.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.ActionTable.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.ActionTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActionTable.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.ActionTable.DocumentMargin = new System.Windows.Forms.Padding(8);
            this.ActionTable.DocumentOrgX = ((long)(0));
            this.ActionTable.DocumentOrgY = ((long)(0));
            this.ActionTable.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ActionTable.FocusedItem = null;
            this.ActionTable.FullRowSelect = true;
            this.ActionTable.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.ActionTable.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.ActionTable.HoverBackColor = System.Drawing.Color.ForestGreen;
            this.ActionTable.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.ActionTable.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ActionTable.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.ActionTable.Location = new System.Drawing.Point(3, 3);
            this.ActionTable.Name = "ActionTable";
            this.ActionTable.Padding = new System.Windows.Forms.Padding(16);
            this.ActionTable.Size = new System.Drawing.Size(325, 201);
            this.ActionTable.TabIndex = 3;
            this.ActionTable.Text = "dpTable1";
            this.ActionTable.SelectionChanged += new System.EventHandler(this.ActionTable_SelectionChanged);
            this.ActionTable.DoubleClick += new System.EventHandler(this.ActionTable_DoubleClick);
            this.ActionTable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ActionTable_KeyDown);
            this.ActionTable.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ActionTable_KeyPress);
            // 
            // dpColumn_name
            // 
            this.dpColumn_name.Alignment = System.Drawing.StringAlignment.Far;
            this.dpColumn_name.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_name.Font = null;
            this.dpColumn_name.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_name.Text = "名称";
            this.dpColumn_name.Width = 148;
            // 
            // dpColumn_shortcutKey
            // 
            this.dpColumn_shortcutKey.Alignment = System.Drawing.StringAlignment.Center;
            this.dpColumn_shortcutKey.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_shortcutKey.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dpColumn_shortcutKey.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_shortcutKey.Text = "快捷键";
            this.dpColumn_shortcutKey.Width = 45;
            // 
            // dpColumn_comment
            // 
            this.dpColumn_comment.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_comment.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_comment.Font = null;
            this.dpColumn_comment.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_comment.Text = "说明";
            this.dpColumn_comment.Width = 204;
            // 
            // dpColumn_entry
            // 
            this.dpColumn_entry.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_entry.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_entry.Font = null;
            this.dpColumn_entry.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_entry.Text = "入口函数";
            this.dpColumn_entry.Width = 150;
            // 
            // GenerateDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 264);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GenerateDataForm";
            this.ShowInTaskbar = false;
            this.Text = "创建数据";
            this.Load += new System.EventHandler(this.GenerateDataForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_dock;
        public DigitalPlatform.CommonControl.DpTable ActionTable;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_name;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_shortcutKey;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_comment;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_entry;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.CheckBox checkBox_autoRun;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_excute;
    }
}