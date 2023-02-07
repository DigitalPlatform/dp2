
namespace dp2KernelApiTester
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_stop = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_settings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_initializeDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_records = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_search = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_refreshKeys = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_largeObject = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_fragmentWrite = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_pdf = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_special = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 75);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(800, 353);
            this.webBrowser1.TabIndex = 7;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_stop});
            this.toolStrip1.Location = new System.Drawing.Point(0, 37);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.toolStrip1.Size = new System.Drawing.Size(800, 38);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_stop
            // 
            this.toolStripButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_stop.Enabled = false;
            this.toolStripButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stop.Image")));
            this.toolStripButton_stop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_stop.Name = "toolStripButton_stop";
            this.toolStripButton_stop.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_stop.Text = "停止";
            this.toolStripButton_stop.Click += new System.EventHandler(this.toolStripButton_stop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
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
            this.menuStrip1.Size = new System.Drawing.Size(800, 37);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_settings});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_file.Text = "文件";
            // 
            // MenuItem_settings
            // 
            this.MenuItem_settings.Name = "MenuItem_settings";
            this.MenuItem_settings.Size = new System.Drawing.Size(192, 40);
            this.MenuItem_settings.Text = "设置 ...";
            this.MenuItem_settings.Click += new System.EventHandler(this.MenuItem_settings_Click);
            // 
            // MenuItem_test
            // 
            this.MenuItem_test.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_test_initializeDatabase,
            this.MenuItem_test_records,
            this.MenuItem_test_search,
            this.MenuItem_test_refreshKeys,
            this.MenuItem_test_largeObject,
            this.MenuItem_fragmentWrite,
            this.MenuItem_test_pdf,
            this.MenuItem_test_special});
            this.MenuItem_test.Name = "MenuItem_test";
            this.MenuItem_test.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_test.Text = "测试";
            // 
            // MenuItem_test_initializeDatabase
            // 
            this.MenuItem_test_initializeDatabase.Name = "MenuItem_test_initializeDatabase";
            this.MenuItem_test_initializeDatabase.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_initializeDatabase.Text = "创建和删除数据库";
            this.MenuItem_test_initializeDatabase.Click += new System.EventHandler(this.MenuItem_test_initializeDatabase_Click);
            // 
            // MenuItem_test_records
            // 
            this.MenuItem_test_records.Name = "MenuItem_test_records";
            this.MenuItem_test_records.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_records.Text = "记录相关";
            this.MenuItem_test_records.Click += new System.EventHandler(this.MenuItem_test_records_Click);
            // 
            // MenuItem_test_search
            // 
            this.MenuItem_test_search.Name = "MenuItem_test_search";
            this.MenuItem_test_search.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_search.Text = "检索相关";
            this.MenuItem_test_search.Click += new System.EventHandler(this.MenuItem_test_search_Click);
            // 
            // MenuItem_test_refreshKeys
            // 
            this.MenuItem_test_refreshKeys.Name = "MenuItem_test_refreshKeys";
            this.MenuItem_test_refreshKeys.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_refreshKeys.Text = "刷新检索点";
            this.MenuItem_test_refreshKeys.Click += new System.EventHandler(this.MenuItem_test_refreshKeys_Click);
            // 
            // MenuItem_test_largeObject
            // 
            this.MenuItem_test_largeObject.Name = "MenuItem_test_largeObject";
            this.MenuItem_test_largeObject.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_largeObject.Text = "大对象";
            this.MenuItem_test_largeObject.Click += new System.EventHandler(this.MenuItem_test_largeObject_Click);
            // 
            // MenuItem_fragmentWrite
            // 
            this.MenuItem_fragmentWrite.Name = "MenuItem_fragmentWrite";
            this.MenuItem_fragmentWrite.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_fragmentWrite.Text = "碎片式写入";
            this.MenuItem_fragmentWrite.Click += new System.EventHandler(this.MenuItem_fragmentWrite_Click);
            // 
            // MenuItem_test_pdf
            // 
            this.MenuItem_test_pdf.Name = "MenuItem_test_pdf";
            this.MenuItem_test_pdf.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_pdf.Text = "PDF 相关";
            this.MenuItem_test_pdf.Click += new System.EventHandler(this.MenuItem_test_pdf_Click);
            // 
            // MenuItem_test_special
            // 
            this.MenuItem_test_special.Name = "MenuItem_test_special";
            this.MenuItem_test_special.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_special.Text = "特殊测试";
            this.MenuItem_test_special.Click += new System.EventHandler(this.MenuItem_test_special_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_settings;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_initializeDatabase;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_records;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_search;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_refreshKeys;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_fragmentWrite;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_largeObject;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_pdf;
        private System.Windows.Forms.ToolStripButton toolStripButton_stop;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_special;
    }
}

