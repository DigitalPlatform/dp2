namespace ZkFingerprint
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

            if (this.pause_event != null)
                this.pause_event.Dispose();

            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.checkBox_beep = new System.Windows.Forms.CheckBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_image = new System.Windows.Forms.Panel();
            this.label_message = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ToolStripMenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_start = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_reopen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_option = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_copyright = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_gameState = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox_speak = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_beep
            // 
            this.checkBox_beep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_beep.AutoSize = true;
            this.checkBox_beep.Location = new System.Drawing.Point(11, 249);
            this.checkBox_beep.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_beep.Name = "checkBox_beep";
            this.checkBox_beep.Size = new System.Drawing.Size(95, 28);
            this.checkBox_beep.TabIndex = 1;
            this.checkBox_beep.Text = "蜂鸣(&B)";
            this.checkBox_beep.UseVisualStyleBackColor = true;
            this.checkBox_beep.CheckedChanged += new System.EventHandler(this.checkBox_beep_CheckedChanged);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(159, -34);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(23, 28);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(159, 218);
            this.webBrowser1.TabIndex = 16;
            this.webBrowser1.Visible = false;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 29);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_image);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.label_message);
            this.splitContainer_main.Size = new System.Drawing.Size(451, 218);
            this.splitContainer_main.SplitterDistance = 199;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 19;
            // 
            // panel_image
            // 
            this.panel_image.BackColor = System.Drawing.SystemColors.Window;
            this.panel_image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_image.Location = new System.Drawing.Point(0, 0);
            this.panel_image.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel_image.Name = "panel_image";
            this.panel_image.Size = new System.Drawing.Size(199, 218);
            this.panel_image.TabIndex = 0;
            // 
            // label_message
            // 
            this.label_message.BackColor = System.Drawing.SystemColors.Window;
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label_message.Location = new System.Drawing.Point(0, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(244, 218);
            this.label_message.TabIndex = 0;
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(307, 256);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(131, 47);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Visible = false;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_file});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(451, 32);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ToolStripMenuItem_file
            // 
            this.ToolStripMenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_start,
            this.ToolStripMenuItem_reopen,
            this.toolStripSeparator2,
            this.ToolStripMenuItem_option,
            this.ToolStripMenuItem_copyright,
            this.toolStripSeparator1,
            this.ToolStripMenuItem_gameState,
            this.toolStripSeparator3,
            this.ToolStripMenuItem_exit});
            this.ToolStripMenuItem_file.Name = "ToolStripMenuItem_file";
            this.ToolStripMenuItem_file.Size = new System.Drawing.Size(80, 28);
            this.ToolStripMenuItem_file.Text = "文件(&F)";
            // 
            // ToolStripMenuItem_start
            // 
            this.ToolStripMenuItem_start.Name = "ToolStripMenuItem_start";
            this.ToolStripMenuItem_start.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_start.Text = "启动(&S)";
            this.ToolStripMenuItem_start.Click += new System.EventHandler(this.ToolStripMenuItem_start_Click);
            // 
            // ToolStripMenuItem_reopen
            // 
            this.ToolStripMenuItem_reopen.Name = "ToolStripMenuItem_reopen";
            this.ToolStripMenuItem_reopen.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_reopen.Text = "重新启动(&R)";
            this.ToolStripMenuItem_reopen.Click += new System.EventHandler(this.ToolStripMenuItem_reopen_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(249, 6);
            // 
            // ToolStripMenuItem_option
            // 
            this.ToolStripMenuItem_option.Name = "ToolStripMenuItem_option";
            this.ToolStripMenuItem_option.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_option.Text = "选项(&O)...";
            this.ToolStripMenuItem_option.Click += new System.EventHandler(this.ToolStripMenuItem_option_Click);
            // 
            // ToolStripMenuItem_copyright
            // 
            this.ToolStripMenuItem_copyright.Name = "ToolStripMenuItem_copyright";
            this.ToolStripMenuItem_copyright.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_copyright.Text = "版权(&C)...";
            this.ToolStripMenuItem_copyright.Click += new System.EventHandler(this.ToolStripMenuItem_copyright_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(249, 6);
            // 
            // ToolStripMenuItem_gameState
            // 
            this.ToolStripMenuItem_gameState.Name = "ToolStripMenuItem_gameState";
            this.ToolStripMenuItem_gameState.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_gameState.Text = "练习状态(&G)";
            this.ToolStripMenuItem_gameState.Click += new System.EventHandler(this.ToolStripMenuItem_gameState_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(249, 6);
            this.toolStripSeparator3.Click += new System.EventHandler(this.toolStripSeparator3_Click);
            // 
            // ToolStripMenuItem_exit
            // 
            this.ToolStripMenuItem_exit.Name = "ToolStripMenuItem_exit";
            this.ToolStripMenuItem_exit.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_exit.Text = "退出(&X)";
            this.ToolStripMenuItem_exit.Click += new System.EventHandler(this.ToolStripMenuItem_exit_Click);
            // 
            // checkBox_speak
            // 
            this.checkBox_speak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_speak.AutoSize = true;
            this.checkBox_speak.Location = new System.Drawing.Point(11, 274);
            this.checkBox_speak.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_speak.Name = "checkBox_speak";
            this.checkBox_speak.Size = new System.Drawing.Size(130, 28);
            this.checkBox_speak.TabIndex = 2;
            this.checkBox_speak.Text = "语音提示(&S)";
            this.checkBox_speak.UseVisualStyleBackColor = true;
            this.checkBox_speak.CheckedChanged += new System.EventHandler(this.checkBox_speak_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 314);
            this.Controls.Add(this.checkBox_speak);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.checkBox_beep);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.Text = "dp2-中控指纹阅读器接口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_beep;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        public System.Windows.Forms.Panel panel_image;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_copyright;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.CheckBox checkBox_speak;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_option;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_start;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_reopen;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_gameState;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

