
namespace RfidTool
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_writeBookTags = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_writeShelfTags = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_writePatronTags = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_saveToExcelFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearHistory_all = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearHistory_selected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_settings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_reconnectReader = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_resetConnectReader = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_userManual = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_resetSerialCode = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_about = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_readerCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_writeTag = new System.Windows.Forms.TabPage();
            this.listView_writeHistory = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tou = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_aoi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_writeTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage_writeTag.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1018, 38);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_writeBookTags,
            this.MenuItem_writeShelfTags,
            this.MenuItem_writePatronTags,
            this.toolStripSeparator3,
            this.MenuItem_saveToExcelFile,
            this.MenuItem_clearHistory,
            this.toolStripSeparator1,
            this.MenuItem_settings,
            this.MenuItem_reconnectReader,
            this.MenuItem_resetConnectReader,
            this.toolStripSeparator2,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(97, 32);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_writeBookTags
            // 
            this.MenuItem_writeBookTags.Name = "MenuItem_writeBookTags";
            this.MenuItem_writeBookTags.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_writeBookTags.Text = "写入图书标签(&B) ...";
            this.MenuItem_writeBookTags.Click += new System.EventHandler(this.MenuItem_writeBookTags_Click);
            // 
            // MenuItem_writeShelfTags
            // 
            this.MenuItem_writeShelfTags.Name = "MenuItem_writeShelfTags";
            this.MenuItem_writeShelfTags.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_writeShelfTags.Text = "写入层架标(&S) ...";
            this.MenuItem_writeShelfTags.Click += new System.EventHandler(this.MenuItem_writeShelfTags_Click);
            // 
            // MenuItem_writePatronTags
            // 
            this.MenuItem_writePatronTags.Name = "MenuItem_writePatronTags";
            this.MenuItem_writePatronTags.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_writePatronTags.Text = "写入读者证(&P) ...";
            this.MenuItem_writePatronTags.Click += new System.EventHandler(this.MenuItem_writePatronTags_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(384, 6);
            // 
            // MenuItem_saveToExcelFile
            // 
            this.MenuItem_saveToExcelFile.Name = "MenuItem_saveToExcelFile";
            this.MenuItem_saveToExcelFile.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_saveToExcelFile.Text = "保存列表到 Excel 文件(&S) ...";
            this.MenuItem_saveToExcelFile.Click += new System.EventHandler(this.MenuItem_saveToExcelFile_Click);
            // 
            // MenuItem_clearHistory
            // 
            this.MenuItem_clearHistory.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_clearHistory_all,
            this.MenuItem_clearHistory_selected});
            this.MenuItem_clearHistory.Name = "MenuItem_clearHistory";
            this.MenuItem_clearHistory.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_clearHistory.Text = "清除“写入历史”列表(&C)";
            this.MenuItem_clearHistory.DropDownOpening += new System.EventHandler(this.MenuItem_clearHistory_DropDownOpening);
            // 
            // MenuItem_clearHistory_all
            // 
            this.MenuItem_clearHistory_all.Name = "MenuItem_clearHistory_all";
            this.MenuItem_clearHistory_all.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_clearHistory_all.Text = "清除全部事项";
            this.MenuItem_clearHistory_all.Click += new System.EventHandler(this.MenuItem_clearHistory_all_Click);
            // 
            // MenuItem_clearHistory_selected
            // 
            this.MenuItem_clearHistory_selected.Name = "MenuItem_clearHistory_selected";
            this.MenuItem_clearHistory_selected.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_clearHistory_selected.Text = "清除所选事项";
            this.MenuItem_clearHistory_selected.Click += new System.EventHandler(this.MenuItem_clearHistory_selected_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(384, 6);
            // 
            // MenuItem_settings
            // 
            this.MenuItem_settings.Name = "MenuItem_settings";
            this.MenuItem_settings.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_settings.Text = "设置(&S) ...";
            this.MenuItem_settings.Click += new System.EventHandler(this.MenuItem_settings_Click);
            // 
            // MenuItem_reconnectReader
            // 
            this.MenuItem_reconnectReader.Name = "MenuItem_reconnectReader";
            this.MenuItem_reconnectReader.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_reconnectReader.Text = "重新连接读写器";
            this.MenuItem_reconnectReader.Click += new System.EventHandler(this.MenuItem_reconnectReader_Click);
            // 
            // MenuItem_resetConnectReader
            // 
            this.MenuItem_resetConnectReader.Name = "MenuItem_resetConnectReader";
            this.MenuItem_resetConnectReader.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_resetConnectReader.Text = "重新探测读写器";
            this.MenuItem_resetConnectReader.Click += new System.EventHandler(this.MenuItem_resetConnectReader_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(384, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(387, 40);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder,
            this.toolStripSeparator4,
            this.MenuItem_userManual,
            this.MenuItem_resetSerialCode,
            this.MenuItem_about});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(102, 32);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(354, 40);
            this.MenuItem_openUserFolder.Text = "打开用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(354, 40);
            this.MenuItem_openDataFolder.Text = "打开数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(354, 40);
            this.MenuItem_openProgramFolder.Text = "打开程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(351, 6);
            // 
            // MenuItem_userManual
            // 
            this.MenuItem_userManual.Name = "MenuItem_userManual";
            this.MenuItem_userManual.Size = new System.Drawing.Size(354, 40);
            this.MenuItem_userManual.Text = "RfidTool 使用指南(&U) ...";
            this.MenuItem_userManual.Click += new System.EventHandler(this.MenuItem_userManual_Click);
            // 
            // MenuItem_resetSerialCode
            // 
            this.MenuItem_resetSerialCode.Name = "MenuItem_resetSerialCode";
            this.MenuItem_resetSerialCode.Size = new System.Drawing.Size(354, 40);
            this.MenuItem_resetSerialCode.Text = "设置序列号(&R) ...";
            this.MenuItem_resetSerialCode.Click += new System.EventHandler(this.MenuItem_resetSerialCode_Click);
            // 
            // MenuItem_about
            // 
            this.MenuItem_about.Name = "MenuItem_about";
            this.MenuItem_about.Size = new System.Drawing.Size(354, 40);
            this.MenuItem_about.Text = "关于(&A)...";
            this.MenuItem_about.Click += new System.EventHandler(this.MenuItem_about_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 38);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1018, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_message,
            this.toolStripStatusLabel_readerCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 557);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1018, 37);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(986, 28);
            this.toolStripStatusLabel_message.Spring = true;
            this.toolStripStatusLabel_message.Text = "...";
            // 
            // toolStripStatusLabel_readerCount
            // 
            this.toolStripStatusLabel_readerCount.Name = "toolStripStatusLabel_readerCount";
            this.toolStripStatusLabel_readerCount.Size = new System.Drawing.Size(17, 28);
            this.toolStripStatusLabel_readerCount.Text = ".";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_writeTag);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 63);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1018, 494);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage_writeTag
            // 
            this.tabPage_writeTag.Controls.Add(this.listView_writeHistory);
            this.tabPage_writeTag.Location = new System.Drawing.Point(4, 37);
            this.tabPage_writeTag.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_writeTag.Name = "tabPage_writeTag";
            this.tabPage_writeTag.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_writeTag.Size = new System.Drawing.Size(1010, 453);
            this.tabPage_writeTag.TabIndex = 0;
            this.tabPage_writeTag.Text = "写入历史";
            this.tabPage_writeTag.UseVisualStyleBackColor = true;
            // 
            // listView_writeHistory
            // 
            this.listView_writeHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_pii,
            this.columnHeader_tou,
            this.columnHeader_oi,
            this.columnHeader_aoi,
            this.columnHeader_writeTime});
            this.listView_writeHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_writeHistory.FullRowSelect = true;
            this.listView_writeHistory.HideSelection = false;
            this.listView_writeHistory.Location = new System.Drawing.Point(4, 3);
            this.listView_writeHistory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listView_writeHistory.Name = "listView_writeHistory";
            this.listView_writeHistory.Size = new System.Drawing.Size(1002, 447);
            this.listView_writeHistory.TabIndex = 0;
            this.listView_writeHistory.UseCompatibleStateImageBehavior = false;
            this.listView_writeHistory.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Name = "columnHeader_uid";
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 221;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Name = "columnHeader_pii";
            this.columnHeader_pii.Text = "PII(条码号)";
            this.columnHeader_pii.Width = 160;
            // 
            // columnHeader_tou
            // 
            this.columnHeader_tou.Name = "columnHeader_tou";
            this.columnHeader_tou.Text = "TOU(用途)";
            this.columnHeader_tou.Width = 160;
            // 
            // columnHeader_oi
            // 
            this.columnHeader_oi.Name = "columnHeader_oi";
            this.columnHeader_oi.Text = "OI(所属机构)";
            this.columnHeader_oi.Width = 260;
            // 
            // columnHeader_aoi
            // 
            this.columnHeader_aoi.Text = "AOI(非标准所属机构)";
            this.columnHeader_aoi.Width = 222;
            // 
            // columnHeader_writeTime
            // 
            this.columnHeader_writeTime.Text = "写入时间";
            this.columnHeader_writeTime.Width = 260;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1018, 594);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.Text = "RfidTool - RFID 工具箱";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage_writeTag.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_writeTag;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ListView listView_writeHistory;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_tou;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_writeBookTags;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_settings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ColumnHeader columnHeader_aoi;
        private System.Windows.Forms.ColumnHeader columnHeader_writeTime;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_saveToExcelFile;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_writeShelfTags;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_writePatronTags;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_about;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_reconnectReader;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetConnectReader;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_readerCount;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearHistory;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_userManual;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetSerialCode;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearHistory_all;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearHistory_selected;
    }
}

