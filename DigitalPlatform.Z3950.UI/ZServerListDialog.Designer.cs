namespace DigitalPlatform.Z3950.UI
{
    partial class ZServerListDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZServerListDialog));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_database = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_enabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSplitButton_new1 = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripButton_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_enabled = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_moveUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_moveDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_export = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_import = new System.Windows.Forms.ToolStripButton();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_database,
            this.columnHeader_enabled});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(16, 40);
            this.listView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(946, 425);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "服务器名";
            this.columnHeader_name.Width = 200;
            // 
            // columnHeader_database
            // 
            this.columnHeader_database.Text = "数据库名";
            this.columnHeader_database.Width = 200;
            // 
            // columnHeader_enabled
            // 
            this.columnHeader_enabled.Text = "是否启用";
            this.columnHeader_enabled.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton_new1,
            this.toolStripButton_modify,
            this.toolStripButton_delete,
            this.toolStripSeparator1,
            this.toolStripButton_enabled,
            this.toolStripSeparator2,
            this.toolStripButton_moveUp,
            this.toolStripButton_moveDown,
            this.toolStripSeparator3,
            this.toolStripButton_export,
            this.toolStripButton_import});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(978, 38);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSplitButton_new1
            // 
            this.toolStripSplitButton_new1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;

            this.toolStripSplitButton_new1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_new1.Image")));
            this.toolStripSplitButton_new1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_new1.Name = "toolStripSplitButton_new1";
            this.toolStripSplitButton_new1.Size = new System.Drawing.Size(78, 32);
            this.toolStripSplitButton_new1.Text = "新增";
            this.toolStripSplitButton_new1.ButtonClick += new System.EventHandler(this.toolStripSplitButton_new1_ButtonClick);
            this.toolStripSplitButton_new1.DropDownOpening += new System.EventHandler(this.toolStripSplitButton_new1_DropDownOpening);
            // 
            // toolStripButton_modify
            // 
            this.toolStripButton_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modify.Enabled = false;
            this.toolStripButton_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modify.Image")));
            this.toolStripButton_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modify.Name = "toolStripButton_modify";
            this.toolStripButton_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_modify.Text = "修改";
            this.toolStripButton_modify.Click += new System.EventHandler(this.toolStripButton_modify_Click);
            // 
            // toolStripButton_delete
            // 
            this.toolStripButton_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_delete.Enabled = false;
            this.toolStripButton_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_delete.Image")));
            this.toolStripButton_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_delete.Name = "toolStripButton_delete";
            this.toolStripButton_delete.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_enabled
            // 
            this.toolStripButton_enabled.CheckOnClick = true;
            this.toolStripButton_enabled.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_enabled.Enabled = false;
            this.toolStripButton_enabled.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_enabled.Image")));
            this.toolStripButton_enabled.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_enabled.Name = "toolStripButton_enabled";
            this.toolStripButton_enabled.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_enabled.Text = "启用";
            this.toolStripButton_enabled.Click += new System.EventHandler(this.toolStripButton_enabled_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_moveUp
            // 
            this.toolStripButton_moveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_moveUp.Enabled = false;
            this.toolStripButton_moveUp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_moveUp.Image")));
            this.toolStripButton_moveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_moveUp.Name = "toolStripButton_moveUp";
            this.toolStripButton_moveUp.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_moveUp.Text = "上移";
            this.toolStripButton_moveUp.Click += new System.EventHandler(this.toolStripButton_moveUp_Click);
            // 
            // toolStripButton_moveDown
            // 
            this.toolStripButton_moveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_moveDown.Enabled = false;
            this.toolStripButton_moveDown.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_moveDown.Image")));
            this.toolStripButton_moveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_moveDown.Name = "toolStripButton_moveDown";
            this.toolStripButton_moveDown.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_moveDown.Text = "下移";
            this.toolStripButton_moveDown.Click += new System.EventHandler(this.toolStripButton_moveDown_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_export
            // 
            this.toolStripButton_export.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_export.Enabled = false;
            this.toolStripButton_export.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_export.Image")));
            this.toolStripButton_export.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_export.Name = "toolStripButton_export";
            this.toolStripButton_export.Size = new System.Drawing.Size(79, 32);
            this.toolStripButton_export.Text = "导出 ...";
            this.toolStripButton_export.Click += new System.EventHandler(this.toolStripButton_export_Click);
            // 
            // toolStripButton_import
            // 
            this.toolStripButton_import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_import.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_import.Image")));
            this.toolStripButton_import.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_import.Name = "toolStripButton_import";
            this.toolStripButton_import.Size = new System.Drawing.Size(79, 32);
            this.toolStripButton_import.Text = "导入 ...";
            this.toolStripButton_import.Click += new System.EventHandler(this.toolStripButton_import_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(860, 472);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(748, 472);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // ZServerListDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(978, 525);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ZServerListDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Z39.50 服务器列表";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZServerListDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ZServerListDialog_FormClosed);
            this.Load += new System.EventHandler(this.ZServerListDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_database;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ToolStripButton toolStripButton_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
        private System.Windows.Forms.ColumnHeader columnHeader_enabled;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_enabled;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_moveUp;
        private System.Windows.Forms.ToolStripButton toolStripButton_moveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_export;
        private System.Windows.Forms.ToolStripButton toolStripButton_import;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_new1;

    }
}