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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
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
            this.columnHeader_database});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 34);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(775, 365);
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
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_new,
            this.toolStripButton_modify,
            this.toolStripButton_delete});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 31);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_new
            // 
            this.toolStripButton_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_new.Image")));
            this.toolStripButton_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_new.Name = "toolStripButton_new";
            this.toolStripButton_new.Size = new System.Drawing.Size(50, 28);
            this.toolStripButton_new.Text = "新增";
            this.toolStripButton_new.Click += new System.EventHandler(this.toolStripButton_new_Click);
            // 
            // toolStripButton_modify
            // 
            this.toolStripButton_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modify.Enabled = false;
            this.toolStripButton_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modify.Image")));
            this.toolStripButton_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modify.Name = "toolStripButton_modify";
            this.toolStripButton_modify.Size = new System.Drawing.Size(50, 28);
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
            this.toolStripButton_delete.Size = new System.Drawing.Size(50, 28);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(704, 405);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 33);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(612, 405);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 33);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // ZServerListDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView1);
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
        private System.Windows.Forms.ToolStripButton toolStripButton_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
    }
}