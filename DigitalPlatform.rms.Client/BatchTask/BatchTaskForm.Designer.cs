namespace DigitalPlatform.rms.Client
{
    partial class BatchTaskForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BatchTaskForm));
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.listView_tasks = new System.Windows.Forms.ListView();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.columnHeader_taskName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStripButton_listTasks = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_listTasks});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Size = new System.Drawing.Size(502, 25);
            this.toolStrip_main.TabIndex = 0;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 25);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.listView_tasks);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer_main.Size = new System.Drawing.Size(502, 239);
            this.splitContainer_main.SplitterDistance = 150;
            this.splitContainer_main.TabIndex = 1;
            // 
            // listView_tasks
            // 
            this.listView_tasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_taskName,
            this.columnHeader_id,
            this.columnHeader_comment});
            this.listView_tasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_tasks.FullRowSelect = true;
            this.listView_tasks.HideSelection = false;
            this.listView_tasks.Location = new System.Drawing.Point(0, 0);
            this.listView_tasks.Name = "listView_tasks";
            this.listView_tasks.Size = new System.Drawing.Size(150, 239);
            this.listView_tasks.TabIndex = 0;
            this.listView_tasks.UseCompatibleStateImageBehavior = false;
            this.listView_tasks.View = System.Windows.Forms.View.Details;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(348, 239);
            this.webBrowser1.TabIndex = 0;
            // 
            // columnHeader_taskName
            // 
            this.columnHeader_taskName.Text = "任务名";
            this.columnHeader_taskName.Width = 100;
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "ID";
            this.columnHeader_id.Width = 100;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "描述";
            this.columnHeader_comment.Width = 200;
            // 
            // toolStripButton_listTasks
            // 
            this.toolStripButton_listTasks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_listTasks.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_listTasks.Image")));
            this.toolStripButton_listTasks.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_listTasks.Name = "toolStripButton_listTasks";
            this.toolStripButton_listTasks.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_listTasks.Text = "列出任务";
            this.toolStripButton_listTasks.Click += new System.EventHandler(this.toolStripButton_listTasks_Click);
            // 
            // BatchTaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 264);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.toolStrip_main);
            this.Name = "BatchTaskForm";
            this.Text = "BatchTaskForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BatchTaskForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BatchTaskForm_FormClosed);
            this.Load += new System.EventHandler(this.BatchTaskForm_Load);
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.ListView listView_tasks;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ColumnHeader columnHeader_taskName;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ToolStripButton toolStripButton_listTasks;
    }
}