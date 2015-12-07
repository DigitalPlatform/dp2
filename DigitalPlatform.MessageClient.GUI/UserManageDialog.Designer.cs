namespace DigitalPlatform.MessageClient
{
    partial class UserManageDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserManageDialog));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_deparmtent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_rights = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_refresh,
            this.toolStripButton_new,
            this.toolStripButton_modify,
            this.toolStripSeparator1,
            this.toolStripButton_delete});
            this.toolStrip1.Location = new System.Drawing.Point(0, 293);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(469, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_refresh.Text = "刷新";
            this.toolStripButton_refresh.Click += new System.EventHandler(this.toolStripButton_refresh_Click);
            // 
            // toolStripButton_new
            // 
            this.toolStripButton_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_new.Image")));
            this.toolStripButton_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_new.Name = "toolStripButton_new";
            this.toolStripButton_new.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_new.Text = "新增";
            this.toolStripButton_new.Click += new System.EventHandler(this.toolStripButton_new_Click);
            // 
            // toolStripButton_modify
            // 
            this.toolStripButton_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modify.Image")));
            this.toolStripButton_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modify.Name = "toolStripButton_modify";
            this.toolStripButton_modify.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_modify.Text = "修改";
            this.toolStripButton_modify.Click += new System.EventHandler(this.toolStripButton_modify_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_delete
            // 
            this.toolStripButton_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_delete.Image")));
            this.toolStripButton_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_delete.Name = "toolStripButton_delete";
            this.toolStripButton_delete.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_userName,
            this.columnHeader_deparmtent,
            this.columnHeader_rights,
            this.columnHeader_tel,
            this.columnHeader_comment});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(469, 293);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 100;
            // 
            // columnHeader_deparmtent
            // 
            this.columnHeader_deparmtent.Text = "单位";
            this.columnHeader_deparmtent.Width = 140;
            // 
            // columnHeader_rights
            // 
            this.columnHeader_rights.Text = "权限";
            this.columnHeader_rights.Width = 100;
            // 
            // columnHeader_tel
            // 
            this.columnHeader_tel.Text = "电话";
            this.columnHeader_tel.Width = 100;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 200;
            // 
            // UserManageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 318);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "UserManageDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "用户管理";
            this.Load += new System.EventHandler(this.UsersDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader_userName;
        private System.Windows.Forms.ColumnHeader columnHeader_deparmtent;
        private System.Windows.Forms.ColumnHeader columnHeader_rights;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.ToolStripButton toolStripButton_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_modify;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
        private System.Windows.Forms.ColumnHeader columnHeader_tel;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
    }
}