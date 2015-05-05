namespace DigitalPlatform.CirculationClient
{
    partial class LocationCanBorrowDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocationCanBorrowDialog));
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_location_up = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_down = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_location_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_delete = new System.Windows.Forms.ToolStripButton();
            this.listView_location_list = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_location_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location_canBorrow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.toolStrip_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_main.AutoSize = false;
            this.toolStrip_main.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_location_up,
            this.toolStripButton_location_down,
            this.toolStripSeparator2,
            this.toolStripButton_location_new,
            this.toolStripButton_location_modify,
            this.toolStripButton_location_delete});
            this.toolStrip_main.Location = new System.Drawing.Point(9, 200);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Size = new System.Drawing.Size(286, 20);
            this.toolStrip_main.TabIndex = 1;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolStripButton_location_up
            // 
            this.toolStripButton_location_up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_up.Enabled = false;
            this.toolStripButton_location_up.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_up.Image")));
            this.toolStripButton_location_up.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_up.Name = "toolStripButton_location_up";
            this.toolStripButton_location_up.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_location_up.Text = "上移";
            this.toolStripButton_location_up.Click += new System.EventHandler(this.toolStripButton_location_up_Click);
            // 
            // toolStripButton_location_down
            // 
            this.toolStripButton_location_down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_down.Enabled = false;
            this.toolStripButton_location_down.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_down.Image")));
            this.toolStripButton_location_down.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_down.Name = "toolStripButton_location_down";
            this.toolStripButton_location_down.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_location_down.Text = "下移";
            this.toolStripButton_location_down.Click += new System.EventHandler(this.toolStripButton_location_down_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 20);
            // 
            // toolStripButton_location_new
            // 
            this.toolStripButton_location_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_new.Image")));
            this.toolStripButton_location_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_new.Name = "toolStripButton_location_new";
            this.toolStripButton_location_new.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_location_new.Text = "新增";
            this.toolStripButton_location_new.Click += new System.EventHandler(this.toolStripButton_location_new_Click);
            // 
            // toolStripButton_location_modify
            // 
            this.toolStripButton_location_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_modify.Enabled = false;
            this.toolStripButton_location_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_modify.Image")));
            this.toolStripButton_location_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_modify.Name = "toolStripButton_location_modify";
            this.toolStripButton_location_modify.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_location_modify.Text = "修改";
            this.toolStripButton_location_modify.Click += new System.EventHandler(this.toolStripButton_location_modify_Click);
            // 
            // toolStripButton_location_delete
            // 
            this.toolStripButton_location_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_delete.Enabled = false;
            this.toolStripButton_location_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_delete.Image")));
            this.toolStripButton_location_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_delete.Name = "toolStripButton_location_delete";
            this.toolStripButton_location_delete.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_location_delete.Text = "删除";
            this.toolStripButton_location_delete.Click += new System.EventHandler(this.toolStripButton_location_delete_Click);
            // 
            // listView_location_list
            // 
            this.listView_location_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_location_name,
            this.columnHeader_location_canBorrow});
            this.listView_location_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_location_list.FullRowSelect = true;
            this.listView_location_list.HideSelection = false;
            this.listView_location_list.Location = new System.Drawing.Point(0, 0);
            this.listView_location_list.Margin = new System.Windows.Forms.Padding(2);
            this.listView_location_list.Name = "listView_location_list";
            this.listView_location_list.Size = new System.Drawing.Size(286, 126);
            this.listView_location_list.TabIndex = 0;
            this.listView_location_list.UseCompatibleStateImageBehavior = false;
            this.listView_location_list.View = System.Windows.Forms.View.Details;
            this.listView_location_list.SelectedIndexChanged += new System.EventHandler(this.listView_location_list_SelectedIndexChanged);
            this.listView_location_list.DoubleClick += new System.EventHandler(this.listView_location_list_DoubleClick);
            this.listView_location_list.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_location_list_MouseUp);
            // 
            // columnHeader_location_name
            // 
            this.columnHeader_location_name.Text = "馆藏地";
            this.columnHeader_location_name.Width = 168;
            // 
            // columnHeader_location_canBorrow
            // 
            this.columnHeader_location_canBorrow.Text = "允许外借";
            this.columnHeader_location_canBorrow.Width = 94;
            // 
            // textBox_comment
            // 
            this.textBox_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_comment.Location = new System.Drawing.Point(0, 0);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(286, 56);
            this.textBox_comment.TabIndex = 0;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(10, 10);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.textBox_comment);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView_location_list);
            this.splitContainer_main.Size = new System.Drawing.Size(286, 188);
            this.splitContainer_main.SplitterDistance = 56;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 0;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(239, 228);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(178, 228);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // LocationCanBorrowDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(304, 260);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.toolStrip_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LocationCanBorrowDialog";
            this.ShowInTaskbar = false;
            this.Text = "馆藏地特性";
            this.Load += new System.EventHandler(this.LocationCanBorrowDialog_Load);
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel1.PerformLayout();
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_up;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_down;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_delete;
        private DigitalPlatform.GUI.ListViewNF listView_location_list;
        private System.Windows.Forms.ColumnHeader columnHeader_location_name;
        private System.Windows.Forms.ColumnHeader columnHeader_location_canBorrow;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}