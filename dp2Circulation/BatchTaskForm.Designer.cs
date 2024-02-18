namespace dp2Circulation
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BatchTaskForm));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_taskName = new System.Windows.Forms.ComboBox();
            this.webBrowser_info = new System.Windows.Forms.WebBrowser();
            this.label_progress = new System.Windows.Forms.Label();
            this.button_start = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.contextMenuStrip_messageStyle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_result = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_progress = new System.Windows.Forms.ToolStripMenuItem();
            this.timer_monitorTask = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_monitoring = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_rewind = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_pauseAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_continue = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStrip_messageStyle.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "������(&N):";
            // 
            // comboBox_taskName
            // 
            this.comboBox_taskName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_taskName.FormattingEnabled = true;
            this.comboBox_taskName.Items.AddRange(new object[] {
            "ԤԼ�������",
            "����֪ͨ",
            "��־�ָ�",
            "~��Ԫһ��ͨ������Ϣͬ��",
            "~�Ͽ�Զ��һ��ͨ������Ϣͬ��",
            "������Ϣͬ��",
            "~dp2Library ͬ��",
            "�ؽ�������",
            "���� MongoDB ��־��",
            "�󱸷�",
            "<��־����>",
            "����������ͬ��",
            "������"});
            this.comboBox_taskName.Location = new System.Drawing.Point(180, 18);
            this.comboBox_taskName.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_taskName.Name = "comboBox_taskName";
            this.comboBox_taskName.Size = new System.Drawing.Size(336, 29);
            this.comboBox_taskName.TabIndex = 1;
            this.comboBox_taskName.TextChanged += new System.EventHandler(this.comboBox_taskName_TextChanged);
            // 
            // webBrowser_info
            // 
            this.webBrowser_info.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser_info.Location = new System.Drawing.Point(17, 74);
            this.webBrowser_info.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_info.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser_info.Name = "webBrowser_info";
            this.webBrowser_info.Size = new System.Drawing.Size(832, 364);
            this.webBrowser_info.TabIndex = 2;
            // 
            // label_progress
            // 
            this.label_progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_progress.Location = new System.Drawing.Point(13, 441);
            this.label_progress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_progress.Name = "label_progress";
            this.label_progress.Size = new System.Drawing.Size(832, 21);
            this.label_progress.TabIndex = 3;
            this.label_progress.Text = "����";
            // 
            // button_start
            // 
            this.button_start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_start.Image = ((System.Drawing.Image)(resources.GetObject("button_start.Image")));
            this.button_start.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button_start.Location = new System.Drawing.Point(546, 18);
            this.button_start.Margin = new System.Windows.Forms.Padding(4);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(147, 38);
            this.button_start.TabIndex = 4;
            this.button_start.Text = "��ʼ(&B)";
            this.button_start.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Image = ((System.Drawing.Image)(resources.GetObject("button_stop.Image")));
            this.button_stop.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button_stop.Location = new System.Drawing.Point(700, 18);
            this.button_stop.Margin = new System.Windows.Forms.Padding(4);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(147, 38);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "ֹͣ(&S)";
            this.button_stop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // contextMenuStrip_messageStyle
            // 
            this.contextMenuStrip_messageStyle.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.contextMenuStrip_messageStyle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_result,
            this.ToolStripMenuItem_progress});
            this.contextMenuStrip_messageStyle.Name = "contextMenuStrip_messageStyle";
            this.contextMenuStrip_messageStyle.Size = new System.Drawing.Size(197, 72);
            // 
            // ToolStripMenuItem_result
            // 
            this.ToolStripMenuItem_result.Name = "ToolStripMenuItem_result";
            this.ToolStripMenuItem_result.Size = new System.Drawing.Size(196, 34);
            this.ToolStripMenuItem_result.Text = "�ۻ�����(&C)";
            this.ToolStripMenuItem_result.Click += new System.EventHandler(this.ToolStripMenuItem_result_Click);
            // 
            // ToolStripMenuItem_progress
            // 
            this.ToolStripMenuItem_progress.Name = "ToolStripMenuItem_progress";
            this.ToolStripMenuItem_progress.Size = new System.Drawing.Size(196, 34);
            this.ToolStripMenuItem_progress.Text = "����(&P)";
            this.ToolStripMenuItem_progress.Click += new System.EventHandler(this.ToolStripMenuItem_progress_Click);
            // 
            // timer_monitorTask
            // 
            this.timer_monitorTask.Interval = 5000;
            this.timer_monitorTask.Tick += new System.EventHandler(this.timer_monitorTask_Tick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_refresh,
            this.toolStripSeparator1,
            this.toolStripButton_monitoring,
            this.toolStripSeparator2,
            this.toolStripButton_rewind,
            this.toolStripButton_clear,
            this.toolStripSeparator3,
            this.toolStripButton_pauseAll,
            this.toolStripButton_continue});
            this.toolStrip1.Location = new System.Drawing.Point(17, 489);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(696, 38);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_refresh.Text = "ˢ��";
            this.toolStripButton_refresh.Click += new System.EventHandler(this.toolStripButton_refresh_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_monitoring
            // 
            this.toolStripButton_monitoring.CheckOnClick = true;
            this.toolStripButton_monitoring.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_monitoring.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_monitoring.Image")));
            this.toolStripButton_monitoring.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_monitoring.Name = "toolStripButton_monitoring";
            this.toolStripButton_monitoring.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_monitoring.Text = "һֱ��ʾ����";
            this.toolStripButton_monitoring.CheckedChanged += new System.EventHandler(this.toolStripButton_monitoring_CheckedChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_rewind
            // 
            this.toolStripButton_rewind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_rewind.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_rewind.Image")));
            this.toolStripButton_rewind.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_rewind.Name = "toolStripButton_rewind";
            this.toolStripButton_rewind.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_rewind.Text = "��ͷ���»�ȡ";
            this.toolStripButton_rewind.Click += new System.EventHandler(this.toolStripButton_rewind_Click);
            // 
            // toolStripButton_clear
            // 
            this.toolStripButton_clear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_clear.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clear.Image")));
            this.toolStripButton_clear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clear.Name = "toolStripButton_clear";
            this.toolStripButton_clear.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_clear.Text = "���";
            this.toolStripButton_clear.Click += new System.EventHandler(this.toolStripButton_clear_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_pauseAll
            // 
            this.toolStripButton_pauseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_pauseAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_pauseAll.Image")));
            this.toolStripButton_pauseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_pauseAll.Name = "toolStripButton_pauseAll";
            this.toolStripButton_pauseAll.Size = new System.Drawing.Size(205, 32);
            this.toolStripButton_pauseAll.Text = "��ͣȫ������������";
            this.toolStripButton_pauseAll.Click += new System.EventHandler(this.toolStripButton_pauseAll_Click);
            // 
            // toolStripButton_continue
            // 
            this.toolStripButton_continue.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_continue.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_continue.Image")));
            this.toolStripButton_continue.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_continue.Name = "toolStripButton_continue";
            this.toolStripButton_continue.Size = new System.Drawing.Size(205, 32);
            this.toolStripButton_continue.Text = "������������������";
            this.toolStripButton_continue.Click += new System.EventHandler(this.toolStripButton_continue_Click);
            // 
            // BatchTaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 542);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.webBrowser_info);
            this.Controls.Add(this.label_progress);
            this.Controls.Add(this.comboBox_taskName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BatchTaskForm";
            this.ShowInTaskbar = false;
            this.Text = "����������";
            this.Activated += new System.EventHandler(this.BatchTaskForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BatchTaskForm_FormClosed);
            this.Load += new System.EventHandler(this.BatchTaskForm_Load);
            this.contextMenuStrip_messageStyle.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_taskName;
        private System.Windows.Forms.WebBrowser webBrowser_info;
        private System.Windows.Forms.Label label_progress;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Timer timer_monitorTask;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_messageStyle;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_result;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_progress;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.ToolStripButton toolStripButton_monitoring;
        private System.Windows.Forms.ToolStripButton toolStripButton_rewind;
        private System.Windows.Forms.ToolStripButton toolStripButton_clear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_pauseAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_continue;
    }
}