
namespace dp2Inventory
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_writeTag = new System.Windows.Forms.TabPage();
            this.listView_writeHistory = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_currentLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tou = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_writeTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_action = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_batchNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_lineNo = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_readerCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_inventory = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exportLocalItemsToExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_saveHistoryToExcelFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearHistory_all = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearHistory_selected = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_settings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_maintenance = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_importUidUiiTable = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearUidUiiTable = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_userManual = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_resetSerialCode = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_about = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPage_writeTag.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_writeTag);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 74);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1209, 798);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage_writeTag
            // 
            this.tabPage_writeTag.Controls.Add(this.listView_writeHistory);
            this.tabPage_writeTag.Location = new System.Drawing.Point(4, 41);
            this.tabPage_writeTag.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_writeTag.Name = "tabPage_writeTag";
            this.tabPage_writeTag.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_writeTag.Size = new System.Drawing.Size(1201, 753);
            this.tabPage_writeTag.TabIndex = 0;
            this.tabPage_writeTag.Text = "盘点历史";
            this.tabPage_writeTag.UseVisualStyleBackColor = true;
            // 
            // listView_writeHistory
            // 
            this.listView_writeHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_pii,
            this.columnHeader_title,
            this.columnHeader_currentLocation,
            this.columnHeader_location,
            this.columnHeader_state,
            this.columnHeader_tou,
            this.columnHeader_oi,
            this.columnHeader_writeTime,
            this.columnHeader_action,
            this.columnHeader_batchNo});
            this.listView_writeHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_writeHistory.Font = new System.Drawing.Font("微软雅黑", 10.71429F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView_writeHistory.FullRowSelect = true;
            this.listView_writeHistory.HideSelection = false;
            this.listView_writeHistory.Location = new System.Drawing.Point(6, 5);
            this.listView_writeHistory.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.listView_writeHistory.Name = "listView_writeHistory";
            this.listView_writeHistory.ShowItemToolTips = true;
            this.listView_writeHistory.Size = new System.Drawing.Size(1189, 743);
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
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "题名";
            this.columnHeader_title.Width = 200;
            // 
            // columnHeader_currentLocation
            // 
            this.columnHeader_currentLocation.Text = "当前位置";
            this.columnHeader_currentLocation.Width = 200;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "永久位置";
            this.columnHeader_location.Width = 200;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 160;
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
            // columnHeader_writeTime
            // 
            this.columnHeader_writeTime.Text = "写入时间";
            this.columnHeader_writeTime.Width = 260;
            // 
            // columnHeader_action
            // 
            this.columnHeader_action.Text = "动作";
            this.columnHeader_action.Width = 200;
            // 
            // columnHeader_batchNo
            // 
            this.columnHeader_batchNo.Text = "批次号";
            this.columnHeader_batchNo.Width = 200;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_message,
            this.toolStripStatusLabel_lineNo,
            this.toolStripStatusLabel_readerCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 872);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 20, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1209, 37);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(1154, 28);
            this.toolStripStatusLabel_message.Spring = true;
            this.toolStripStatusLabel_message.Text = "...";
            // 
            // toolStripStatusLabel_lineNo
            // 
            this.toolStripStatusLabel_lineNo.Name = "toolStripStatusLabel_lineNo";
            this.toolStripStatusLabel_lineNo.Size = new System.Drawing.Size(17, 28);
            this.toolStripStatusLabel_lineNo.Text = ".";
            // 
            // toolStripStatusLabel_readerCount
            // 
            this.toolStripStatusLabel_readerCount.Name = "toolStripStatusLabel_readerCount";
            this.toolStripStatusLabel_readerCount.Size = new System.Drawing.Size(17, 28);
            this.toolStripStatusLabel_readerCount.Text = ".";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 49);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1209, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.71429F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_maintenance,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 5, 0, 5);
            this.menuStrip1.Size = new System.Drawing.Size(1209, 49);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_inventory,
            this.toolStripSeparator5,
            this.MenuItem_exportLocalItemsToExcel,
            this.toolStripSeparator3,
            this.MenuItem_saveHistoryToExcelFile,
            this.MenuItem_clearHistory,
            this.toolStripSeparator1,
            this.MenuItem_settings,
            this.toolStripSeparator2,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(117, 39);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_inventory
            // 
            this.MenuItem_inventory.Name = "MenuItem_inventory";
            this.MenuItem_inventory.Size = new System.Drawing.Size(530, 44);
            this.MenuItem_inventory.Text = "盘点(&I)...";
            this.MenuItem_inventory.Click += new System.EventHandler(this.MenuItem_inventory_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(527, 6);
            // 
            // MenuItem_exportLocalItemsToExcel
            // 
            this.MenuItem_exportLocalItemsToExcel.Name = "MenuItem_exportLocalItemsToExcel";
            this.MenuItem_exportLocalItemsToExcel.Size = new System.Drawing.Size(530, 44);
            this.MenuItem_exportLocalItemsToExcel.Text = "导出本地册记录到 Excel 文件(&L) ...";
            this.MenuItem_exportLocalItemsToExcel.Click += new System.EventHandler(this.MenuItem_exportLocalItemsToExcel_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(527, 6);
            // 
            // MenuItem_saveHistoryToExcelFile
            // 
            this.MenuItem_saveHistoryToExcelFile.Name = "MenuItem_saveHistoryToExcelFile";
            this.MenuItem_saveHistoryToExcelFile.Size = new System.Drawing.Size(530, 44);
            this.MenuItem_saveHistoryToExcelFile.Text = "保存盘点历史到 Excel 文件(&S) ...";
            this.MenuItem_saveHistoryToExcelFile.Click += new System.EventHandler(this.MenuItem_saveHistoryToExcelFile_Click);
            // 
            // MenuItem_clearHistory
            // 
            this.MenuItem_clearHistory.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_clearHistory_all,
            this.MenuItem_clearHistory_selected});
            this.MenuItem_clearHistory.Name = "MenuItem_clearHistory";
            this.MenuItem_clearHistory.Size = new System.Drawing.Size(530, 44);
            this.MenuItem_clearHistory.Text = "清除“盘点历史”列表(&C)";
            // 
            // MenuItem_clearHistory_all
            // 
            this.MenuItem_clearHistory_all.Name = "MenuItem_clearHistory_all";
            this.MenuItem_clearHistory_all.Size = new System.Drawing.Size(315, 44);
            this.MenuItem_clearHistory_all.Text = "清除全部事项";
            this.MenuItem_clearHistory_all.Click += new System.EventHandler(this.MenuItem_clearHistory_all_Click);
            // 
            // MenuItem_clearHistory_selected
            // 
            this.MenuItem_clearHistory_selected.Name = "MenuItem_clearHistory_selected";
            this.MenuItem_clearHistory_selected.Size = new System.Drawing.Size(315, 44);
            this.MenuItem_clearHistory_selected.Text = "清除所选事项";
            this.MenuItem_clearHistory_selected.Click += new System.EventHandler(this.MenuItem_clearHistory_selected_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(527, 6);
            // 
            // MenuItem_settings
            // 
            this.MenuItem_settings.Name = "MenuItem_settings";
            this.MenuItem_settings.Size = new System.Drawing.Size(530, 44);
            this.MenuItem_settings.Text = "设置(&S) ...";
            this.MenuItem_settings.Click += new System.EventHandler(this.MenuItem_settings_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(527, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(530, 44);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_maintenance
            // 
            this.MenuItem_maintenance.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_importUidUiiTable,
            this.MenuItem_clearUidUiiTable});
            this.MenuItem_maintenance.Name = "MenuItem_maintenance";
            this.MenuItem_maintenance.Size = new System.Drawing.Size(128, 39);
            this.MenuItem_maintenance.Text = "维护(&M)";
            // 
            // MenuItem_importUidUiiTable
            // 
            this.MenuItem_importUidUiiTable.Name = "MenuItem_importUidUiiTable";
            this.MenuItem_importUidUiiTable.Size = new System.Drawing.Size(482, 44);
            this.MenuItem_importUidUiiTable.Text = "导入 UID-->UII 对照关系(&I) ...";
            this.MenuItem_importUidUiiTable.Click += new System.EventHandler(this.MenuItem_importUidUiiTable_Click);
            // 
            // MenuItem_clearUidUiiTable
            // 
            this.MenuItem_clearUidUiiTable.Name = "MenuItem_clearUidUiiTable";
            this.MenuItem_clearUidUiiTable.Size = new System.Drawing.Size(482, 44);
            this.MenuItem_clearUidUiiTable.Text = "清除 UID-->UII 对照关系(&C)";
            this.MenuItem_clearUidUiiTable.Click += new System.EventHandler(this.MenuItem_clearUidUiiTable_Click);
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
            this.MenuItem_help.Size = new System.Drawing.Size(123, 39);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(479, 44);
            this.MenuItem_openUserFolder.Text = "打开用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(479, 44);
            this.MenuItem_openDataFolder.Text = "打开数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(479, 44);
            this.MenuItem_openProgramFolder.Text = "打开程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(476, 6);
            // 
            // MenuItem_userManual
            // 
            this.MenuItem_userManual.Name = "MenuItem_userManual";
            this.MenuItem_userManual.Size = new System.Drawing.Size(479, 44);
            this.MenuItem_userManual.Text = "dp2Inventory 使用指南(&U) ...";
            this.MenuItem_userManual.Click += new System.EventHandler(this.MenuItem_userManual_Click);
            // 
            // MenuItem_resetSerialCode
            // 
            this.MenuItem_resetSerialCode.Name = "MenuItem_resetSerialCode";
            this.MenuItem_resetSerialCode.Size = new System.Drawing.Size(479, 44);
            this.MenuItem_resetSerialCode.Text = "设置序列号(&R) ...";
            this.MenuItem_resetSerialCode.Click += new System.EventHandler(this.MenuItem_resetSerialCode_Click);
            // 
            // MenuItem_about
            // 
            this.MenuItem_about.Name = "MenuItem_about";
            this.MenuItem_about.Size = new System.Drawing.Size(479, 44);
            this.MenuItem_about.Text = "关于(&A)...";
            this.MenuItem_about.Click += new System.EventHandler(this.MenuItem_about_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1209, 909);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.71429F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "dp2Inventory -- 盘点";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_writeTag.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_writeTag;
        private System.Windows.Forms.ListView listView_writeHistory;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_tou;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.ColumnHeader columnHeader_writeTime;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_lineNo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_readerCount;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_inventory;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_saveHistoryToExcelFile;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearHistory;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearHistory_all;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearHistory_selected;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_settings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_userManual;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetSerialCode;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_about;
        private System.Windows.Forms.ColumnHeader columnHeader_currentLocation;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_action;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exportLocalItemsToExcel;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_maintenance;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_importUidUiiTable;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearUidUiiTable;
    }
}

