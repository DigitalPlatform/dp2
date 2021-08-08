
namespace dp2LibraryApiTester
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_settings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_initialEnvironment = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_searchReaderSafety = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_searchBiblioSafety = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_searchItemSafety = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.MenuItem_test_setReaderInfoApi = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuStrip1.Size = new System.Drawing.Size(800, 37);
            this.menuStrip1.TabIndex = 0;
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
            this.MenuItem_initialEnvironment,
            this.MenuItem_test_searchReaderSafety,
            this.MenuItem_test_searchBiblioSafety,
            this.MenuItem_test_searchItemSafety,
            this.MenuItem_test_setReaderInfoApi});
            this.MenuItem_test.Name = "MenuItem_test";
            this.MenuItem_test.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_test.Text = "测试";
            // 
            // MenuItem_initialEnvironment
            // 
            this.MenuItem_initialEnvironment.Name = "MenuItem_initialEnvironment";
            this.MenuItem_initialEnvironment.Size = new System.Drawing.Size(385, 40);
            this.MenuItem_initialEnvironment.Text = "初始化测试环境 ...";
            this.MenuItem_initialEnvironment.Click += new System.EventHandler(this.MenuItem_initialEnvironment_Click);
            // 
            // MenuItem_test_searchReaderSafety
            // 
            this.MenuItem_test_searchReaderSafety.Name = "MenuItem_test_searchReaderSafety";
            this.MenuItem_test_searchReaderSafety.Size = new System.Drawing.Size(385, 40);
            this.MenuItem_test_searchReaderSafety.Text = "测试 SearchReader 安全性";
            this.MenuItem_test_searchReaderSafety.Click += new System.EventHandler(this.MenuItem_test_searchReaderSafety_Click);
            // 
            // MenuItem_test_searchBiblioSafety
            // 
            this.MenuItem_test_searchBiblioSafety.Name = "MenuItem_test_searchBiblioSafety";
            this.MenuItem_test_searchBiblioSafety.Size = new System.Drawing.Size(385, 40);
            this.MenuItem_test_searchBiblioSafety.Text = "测试 SearchBiblio 安全性";
            this.MenuItem_test_searchBiblioSafety.Click += new System.EventHandler(this.MenuItem_test_searchBiblioSafety_Click);
            // 
            // MenuItem_test_searchItemSafety
            // 
            this.MenuItem_test_searchItemSafety.Name = "MenuItem_test_searchItemSafety";
            this.MenuItem_test_searchItemSafety.Size = new System.Drawing.Size(385, 40);
            this.MenuItem_test_searchItemSafety.Text = "测试 SearchItem 安全性";
            this.MenuItem_test_searchItemSafety.Click += new System.EventHandler(this.MenuItem_test_searchItemSafety_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 37);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 62);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(800, 366);
            this.webBrowser1.TabIndex = 3;
            // 
            // MenuItem_test_setReaderInfoApi
            // 
            this.MenuItem_test_setReaderInfoApi.Name = "MenuItem_test_setReaderInfoApi";
            this.MenuItem_test_setReaderInfoApi.Size = new System.Drawing.Size(385, 40);
            this.MenuItem_test_setReaderInfoApi.Text = "测试 SerReaderInfo() API";
            this.MenuItem_test_setReaderInfoApi.Click += new System.EventHandler(this.MenuItem_test_setReaderInfoApi_Click);
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
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_initialEnvironment;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_settings;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_searchReaderSafety;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_searchBiblioSafety;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_searchItemSafety;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_setReaderInfoApi;
    }
}

