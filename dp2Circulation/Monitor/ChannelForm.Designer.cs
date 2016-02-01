namespace dp2Circulation
{
    partial class ChannelForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChannelForm));
            this.toolStrip_channel = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox_IP = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox_UserName = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_count = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_detail = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prevQuery = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_nextQuery = new System.Windows.Forms.ToolStripButton();
            this.listView_channel = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_clientIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_via = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_count = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_callCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_lang = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_sessionID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_channel = new System.Windows.Forms.TabPage();
            this.label_channel_message = new System.Windows.Forms.Label();
            this.tabPage_blackList = new System.Windows.Forms.TabPage();
            this.toolStrip_channel.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_channel.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip_channel
            // 
            this.toolStrip_channel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripTextBox_IP,
            this.toolStripLabel2,
            this.toolStripTextBox_UserName,
            this.toolStripSeparator1,
            this.toolStripButton_count,
            this.toolStripButton_detail,
            this.toolStripSeparator2,
            this.toolStripButton_refresh,
            this.toolStripButton_prevQuery,
            this.toolStripButton_nextQuery});
            this.toolStrip_channel.Location = new System.Drawing.Point(3, 3);
            this.toolStrip_channel.Name = "toolStrip_channel";
            this.toolStrip_channel.Size = new System.Drawing.Size(472, 25);
            this.toolStrip_channel.TabIndex = 0;
            this.toolStrip_channel.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(22, 22);
            this.toolStripLabel1.Text = "IP:";
            // 
            // toolStripTextBox_IP
            // 
            this.toolStripTextBox_IP.Name = "toolStripTextBox_IP";
            this.toolStripTextBox_IP.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(47, 22);
            this.toolStripLabel2.Text = "用户名:";
            // 
            // toolStripTextBox_UserName
            // 
            this.toolStripTextBox_UserName.Name = "toolStripTextBox_UserName";
            this.toolStripTextBox_UserName.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_count
            // 
            this.toolStripButton_count.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_count.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_count.Image")));
            this.toolStripButton_count.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_count.Name = "toolStripButton_count";
            this.toolStripButton_count.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_count.Text = "概览";
            this.toolStripButton_count.Click += new System.EventHandler(this.toolStripButton_channel_count_Click);
            // 
            // toolStripButton_detail
            // 
            this.toolStripButton_detail.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_detail.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_detail.Image")));
            this.toolStripButton_detail.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_detail.Name = "toolStripButton_detail";
            this.toolStripButton_detail.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_detail.Text = "详情";
            this.toolStripButton_detail.Click += new System.EventHandler(this.toolStripButton_channel_detail_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_refresh.Text = "刷新";
            this.toolStripButton_refresh.Click += new System.EventHandler(this.toolStripButton_channel_refresh_Click);
            // 
            // toolStripButton_prevQuery
            // 
            this.toolStripButton_prevQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_prevQuery.Enabled = false;
            this.toolStripButton_prevQuery.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prevQuery.Image")));
            this.toolStripButton_prevQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_prevQuery.Name = "toolStripButton_prevQuery";
            this.toolStripButton_prevQuery.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_prevQuery.Text = "后退";
            this.toolStripButton_prevQuery.Click += new System.EventHandler(this.toolStripButton_channel_prevQuery_Click);
            // 
            // toolStripButton_nextQuery
            // 
            this.toolStripButton_nextQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_nextQuery.Enabled = false;
            this.toolStripButton_nextQuery.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_nextQuery.Image")));
            this.toolStripButton_nextQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_nextQuery.Name = "toolStripButton_nextQuery";
            this.toolStripButton_nextQuery.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_nextQuery.Text = "前进";
            this.toolStripButton_nextQuery.Click += new System.EventHandler(this.toolStripButton_channel_nextQuery_Click);
            // 
            // listView_channel
            // 
            this.listView_channel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_channel.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_clientIP,
            this.columnHeader_via,
            this.columnHeader_count,
            this.columnHeader_userName,
            this.columnHeader_location,
            this.columnHeader_callCount,
            this.columnHeader_libraryCode,
            this.columnHeader_lang,
            this.columnHeader_sessionID});
            this.listView_channel.FullRowSelect = true;
            this.listView_channel.HideSelection = false;
            this.listView_channel.Location = new System.Drawing.Point(3, 31);
            this.listView_channel.Name = "listView_channel";
            this.listView_channel.Size = new System.Drawing.Size(472, 189);
            this.listView_channel.TabIndex = 1;
            this.listView_channel.UseCompatibleStateImageBehavior = false;
            this.listView_channel.View = System.Windows.Forms.View.Details;
            this.listView_channel.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_channel_ColumnClick);
            this.listView_channel.DoubleClick += new System.EventHandler(this.listView_channel_DoubleClick);
            this.listView_channel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_channel_MouseUp);
            // 
            // columnHeader_clientIP
            // 
            this.columnHeader_clientIP.Text = "前端 IP";
            this.columnHeader_clientIP.Width = 141;
            // 
            // columnHeader_via
            // 
            this.columnHeader_via.Text = "经由服务";
            this.columnHeader_via.Width = 108;
            // 
            // columnHeader_count
            // 
            this.columnHeader_count.Text = "数量";
            this.columnHeader_count.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_count.Width = 81;
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 77;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "工作台号";
            this.columnHeader_location.Width = 100;
            // 
            // columnHeader_callCount
            // 
            this.columnHeader_callCount.Text = "请求数";
            this.columnHeader_callCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_callCount.Width = 80;
            // 
            // columnHeader_libraryCode
            // 
            this.columnHeader_libraryCode.Text = "图书馆代码";
            this.columnHeader_libraryCode.Width = 96;
            // 
            // columnHeader_lang
            // 
            this.columnHeader_lang.Text = "语言";
            // 
            // columnHeader_sessionID
            // 
            this.columnHeader_sessionID.Text = "会话 ID";
            this.columnHeader_sessionID.Width = 300;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_channel);
            this.tabControl_main.Controls.Add(this.tabPage_blackList);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(486, 264);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_channel
            // 
            this.tabPage_channel.Controls.Add(this.label_channel_message);
            this.tabPage_channel.Controls.Add(this.listView_channel);
            this.tabPage_channel.Controls.Add(this.toolStrip_channel);
            this.tabPage_channel.Location = new System.Drawing.Point(4, 22);
            this.tabPage_channel.Name = "tabPage_channel";
            this.tabPage_channel.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_channel.Size = new System.Drawing.Size(478, 238);
            this.tabPage_channel.TabIndex = 0;
            this.tabPage_channel.Text = "通道";
            this.tabPage_channel.UseVisualStyleBackColor = true;
            // 
            // label_channel_message
            // 
            this.label_channel_message.AutoSize = true;
            this.label_channel_message.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label_channel_message.Location = new System.Drawing.Point(3, 223);
            this.label_channel_message.Name = "label_channel_message";
            this.label_channel_message.Size = new System.Drawing.Size(0, 12);
            this.label_channel_message.TabIndex = 2;
            // 
            // tabPage_blackList
            // 
            this.tabPage_blackList.Location = new System.Drawing.Point(4, 22);
            this.tabPage_blackList.Name = "tabPage_blackList";
            this.tabPage_blackList.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_blackList.Size = new System.Drawing.Size(478, 238);
            this.tabPage_blackList.TabIndex = 1;
            this.tabPage_blackList.Text = "黑名单";
            this.tabPage_blackList.UseVisualStyleBackColor = true;
            // 
            // ChannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 264);
            this.Controls.Add(this.tabControl_main);
            this.Name = "ChannelForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "通道管理窗";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChannelForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChannelForm_FormClosed);
            this.Load += new System.EventHandler(this.ChannelForm_Load);
            this.toolStrip_channel.ResumeLayout(false);
            this.toolStrip_channel.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_channel.ResumeLayout(false);
            this.tabPage_channel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip_channel;
        private DigitalPlatform.GUI.ListViewNF listView_channel;
        private System.Windows.Forms.ColumnHeader columnHeader_clientIP;
        private System.Windows.Forms.ColumnHeader columnHeader_via;
        private System.Windows.Forms.ColumnHeader columnHeader_userName;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryCode;
        private System.Windows.Forms.ColumnHeader columnHeader_count;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_IP;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_UserName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.ColumnHeader columnHeader_sessionID;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ToolStripButton toolStripButton_count;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_prevQuery;
        private System.Windows.Forms.ToolStripButton toolStripButton_nextQuery;
        private System.Windows.Forms.ColumnHeader columnHeader_callCount;
        private System.Windows.Forms.ToolStripButton toolStripButton_detail;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_channel;
        private System.Windows.Forms.Label label_channel_message;
        private System.Windows.Forms.TabPage tabPage_blackList;
        private System.Windows.Forms.ColumnHeader columnHeader_lang;
    }
}