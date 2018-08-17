namespace GcatLite
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.checkBox_selectEntry = new System.Windows.Forms.CheckBox();
            this.checkBox_selectPinyin = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_debugInfo = new System.Windows.Forms.TextBox();
            this.checkBox_outputDebugInfo = new System.Windows.Forms.CheckBox();
            this.textBox_url = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_get = new System.Windows.Forms.Button();
            this.textBox_number = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_author = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_copyResultToClipboard = new System.Windows.Forms.CheckBox();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkBox_clipboardChain = new System.Windows.Forms.CheckBox();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_support = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_copyright = new System.Windows.Forms.ToolStripButton();
            this.statusStrip_main.SuspendLayout();
            this.toolStrip_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_selectEntry
            // 
            this.checkBox_selectEntry.AutoSize = true;
            this.checkBox_selectEntry.Checked = true;
            this.checkBox_selectEntry.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_selectEntry.Location = new System.Drawing.Point(318, 278);
            this.checkBox_selectEntry.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_selectEntry.Name = "checkBox_selectEntry";
            this.checkBox_selectEntry.Size = new System.Drawing.Size(223, 22);
            this.checkBox_selectEntry.TabIndex = 23;
            this.checkBox_selectEntry.Text = "遇多个条目提示选择(&E)";
            // 
            // checkBox_selectPinyin
            // 
            this.checkBox_selectPinyin.AutoSize = true;
            this.checkBox_selectPinyin.Checked = true;
            this.checkBox_selectPinyin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_selectPinyin.Location = new System.Drawing.Point(318, 244);
            this.checkBox_selectPinyin.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_selectPinyin.Name = "checkBox_selectPinyin";
            this.checkBox_selectPinyin.Size = new System.Drawing.Size(205, 22);
            this.checkBox_selectPinyin.TabIndex = 17;
            this.checkBox_selectPinyin.Text = "遇多音字提示选择(&S)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 456);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 18);
            this.label4.TabIndex = 21;
            this.label4.Text = "调试信息:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // textBox_debugInfo
            // 
            this.textBox_debugInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_debugInfo.Location = new System.Drawing.Point(16, 478);
            this.textBox_debugInfo.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_debugInfo.Multiline = true;
            this.textBox_debugInfo.Name = "textBox_debugInfo";
            this.textBox_debugInfo.ReadOnly = true;
            this.textBox_debugInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_debugInfo.Size = new System.Drawing.Size(576, 82);
            this.textBox_debugInfo.TabIndex = 22;
            // 
            // checkBox_outputDebugInfo
            // 
            this.checkBox_outputDebugInfo.AutoSize = true;
            this.checkBox_outputDebugInfo.Checked = true;
            this.checkBox_outputDebugInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_outputDebugInfo.Location = new System.Drawing.Point(318, 212);
            this.checkBox_outputDebugInfo.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_outputDebugInfo.Name = "checkBox_outputDebugInfo";
            this.checkBox_outputDebugInfo.Size = new System.Drawing.Size(169, 22);
            this.checkBox_outputDebugInfo.TabIndex = 16;
            this.checkBox_outputDebugInfo.Text = "输出调试信息(&D)";
            // 
            // textBox_url
            // 
            this.textBox_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_url.Location = new System.Drawing.Point(16, 81);
            this.textBox_url.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_url.Name = "textBox_url";
            this.textBox_url.Size = new System.Drawing.Size(576, 28);
            this.textBox_url.TabIndex = 13;
            this.textBox_url.Text = "http://dp2003.com/dp2library/rest";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 58);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(188, 18);
            this.label3.TabIndex = 12;
            this.label3.Text = "GCAT Webservice URL:";
            // 
            // button_get
            // 
            this.button_get.Location = new System.Drawing.Point(18, 278);
            this.button_get.Margin = new System.Windows.Forms.Padding(4);
            this.button_get.Name = "button_get";
            this.button_get.Size = new System.Drawing.Size(112, 45);
            this.button_get.TabIndex = 18;
            this.button_get.Text = "获取(&G)";
            this.button_get.Click += new System.EventHandler(this.button_get_Click);
            // 
            // textBox_number
            // 
            this.textBox_number.Location = new System.Drawing.Point(16, 387);
            this.textBox_number.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_number.Name = "textBox_number";
            this.textBox_number.Size = new System.Drawing.Size(262, 28);
            this.textBox_number.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 364);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 18);
            this.label2.TabIndex = 19;
            this.label2.Text = "著者号(&N):";
            // 
            // textBox_author
            // 
            this.textBox_author.Location = new System.Drawing.Point(16, 168);
            this.textBox_author.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_author.Name = "textBox_author";
            this.textBox_author.Size = new System.Drawing.Size(574, 28);
            this.textBox_author.TabIndex = 15;
            this.textBox_author.Enter += new System.EventHandler(this.textBox_author_Enter);
            this.textBox_author.Leave += new System.EventHandler(this.textBox_author_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 146);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 18);
            this.label1.TabIndex = 14;
            this.label1.Text = "著者(&A):";
            // 
            // checkBox_copyResultToClipboard
            // 
            this.checkBox_copyResultToClipboard.AutoSize = true;
            this.checkBox_copyResultToClipboard.Location = new System.Drawing.Point(318, 390);
            this.checkBox_copyResultToClipboard.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_copyResultToClipboard.Name = "checkBox_copyResultToClipboard";
            this.checkBox_copyResultToClipboard.Size = new System.Drawing.Size(241, 22);
            this.checkBox_copyResultToClipboard.TabIndex = 24;
            this.checkBox_copyResultToClipboard.Text = "结果自动复制到剪贴板(&C)";
            this.checkBox_copyResultToClipboard.UseVisualStyleBackColor = true;
            this.checkBox_copyResultToClipboard.CheckedChanged += new System.EventHandler(this.checkBox_copyResultToClipboard_CheckedChanged);
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip_main.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_main});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 578);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip_main.Size = new System.Drawing.Size(612, 22);
            this.statusStrip_main.TabIndex = 25;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(0, 17);
            // 
            // checkBox_clipboardChain
            // 
            this.checkBox_clipboardChain.AutoSize = true;
            this.checkBox_clipboardChain.Location = new System.Drawing.Point(18, 212);
            this.checkBox_clipboardChain.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_clipboardChain.Name = "checkBox_clipboardChain";
            this.checkBox_clipboardChain.Size = new System.Drawing.Size(187, 22);
            this.checkBox_clipboardChain.TabIndex = 26;
            this.checkBox_clipboardChain.Text = "剪贴板活动敏感(&S)";
            this.checkBox_clipboardChain.UseVisualStyleBackColor = true;
            this.checkBox_clipboardChain.CheckedChanged += new System.EventHandler(this.checkBox_clipboardChain_CheckedChanged);
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_support,
            this.toolStripButton_copyright});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_main.Size = new System.Drawing.Size(612, 31);
            this.toolStrip_main.TabIndex = 27;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolStripButton_support
            // 
            this.toolStripButton_support.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_support.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_support.Image")));
            this.toolStripButton_support.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_support.Name = "toolStripButton_support";
            this.toolStripButton_support.Size = new System.Drawing.Size(86, 28);
            this.toolStripButton_support.Text = "技术支持";
            this.toolStripButton_support.Click += new System.EventHandler(this.toolStripButton_copyright_Click);
            // 
            // toolStripButton_copyright
            // 
            this.toolStripButton_copyright.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_copyright.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_copyright.Image")));
            this.toolStripButton_copyright.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_copyright.Name = "toolStripButton_copyright";
            this.toolStripButton_copyright.Size = new System.Drawing.Size(50, 28);
            this.toolStripButton_copyright.Text = "版权";
            this.toolStripButton_copyright.Click += new System.EventHandler(this.toolStripButton_copyright_Click_1);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 600);
            this.Controls.Add(this.toolStrip_main);
            this.Controls.Add(this.checkBox_clipboardChain);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.checkBox_copyResultToClipboard);
            this.Controls.Add(this.checkBox_selectEntry);
            this.Controls.Add(this.checkBox_selectPinyin);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_debugInfo);
            this.Controls.Add(this.checkBox_outputDebugInfo);
            this.Controls.Add(this.textBox_url);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_get);
            this.Controls.Add(this.textBox_number);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_author);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "GcatLite 通用著者号码表 取号小程序";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_selectEntry;
        private System.Windows.Forms.CheckBox checkBox_selectPinyin;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_debugInfo;
        private System.Windows.Forms.CheckBox checkBox_outputDebugInfo;
        private System.Windows.Forms.TextBox textBox_url;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_get;
        private System.Windows.Forms.TextBox textBox_number;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_author;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_copyResultToClipboard;
        private System.Windows.Forms.StatusStrip statusStrip_main;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_main;
        private System.Windows.Forms.CheckBox checkBox_clipboardChain;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripButton toolStripButton_support;
        private System.Windows.Forms.ToolStripButton toolStripButton_copyright;
    }
}

