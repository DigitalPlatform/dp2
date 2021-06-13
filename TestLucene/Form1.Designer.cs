
namespace TestLucene
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_addDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_deleteDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_search = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_test});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(828, 37);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 37);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(828, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Location = new System.Drawing.Point(0, 512);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(828, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_file.Text = "文件";
            // 
            // MenuItem_test
            // 
            this.MenuItem_test.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_test_addDocument,
            this.MenuItem_test_deleteDocument,
            this.MenuItem_test_search});
            this.MenuItem_test.Name = "MenuItem_test";
            this.MenuItem_test.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_test.Text = "测试";
            // 
            // MenuItem_test_addDocument
            // 
            this.MenuItem_test_addDocument.Name = "MenuItem_test_addDocument";
            this.MenuItem_test_addDocument.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_addDocument.Text = "添加 Document";
            this.MenuItem_test_addDocument.Click += new System.EventHandler(this.MenuItem_test_addDocument_Click);
            // 
            // MenuItem_test_deleteDocument
            // 
            this.MenuItem_test_deleteDocument.Name = "MenuItem_test_deleteDocument";
            this.MenuItem_test_deleteDocument.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_deleteDocument.Text = "删除 Document";
            this.MenuItem_test_deleteDocument.Click += new System.EventHandler(this.MenuItem_test_deleteDocument_Click);
            // 
            // MenuItem_test_search
            // 
            this.MenuItem_test_search.Name = "MenuItem_test_search";
            this.MenuItem_test_search.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_search.Text = "查询 Document";
            this.MenuItem_test_search.Click += new System.EventHandler(this.MenuItem_test_search_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 534);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_addDocument;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_deleteDocument;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_search;
    }
}

