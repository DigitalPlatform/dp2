namespace dp2Circulation
{
    partial class ChatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel_input = new System.Windows.Forms.Panel();
            this.button_send = new System.Windows.Forms.Button();
            this.textBox_input = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_searchBiblio = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_message = new System.Windows.Forms.SplitContainer();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.dpTable_groups = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_icon = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_name = new DigitalPlatform.CommonControl.DpColumn();
            this.panel_input.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_message)).BeginInit();
            this.splitContainer_message.Panel1.SuspendLayout();
            this.splitContainer_message.Panel2.SuspendLayout();
            this.splitContainer_message.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(798, 434);
            this.webBrowser1.TabIndex = 0;
            // 
            // panel_input
            // 
            this.panel_input.Controls.Add(this.button_send);
            this.panel_input.Controls.Add(this.textBox_input);
            this.panel_input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_input.Location = new System.Drawing.Point(0, 0);
            this.panel_input.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_input.Name = "panel_input";
            this.panel_input.Size = new System.Drawing.Size(798, 116);
            this.panel_input.TabIndex = 1;
            // 
            // button_send
            // 
            this.button_send.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_send.Location = new System.Drawing.Point(655, 7);
            this.button_send.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_send.Name = "button_send";
            this.button_send.Size = new System.Drawing.Size(143, 40);
            this.button_send.TabIndex = 1;
            this.button_send.Text = "发送";
            this.button_send.UseVisualStyleBackColor = true;
            this.button_send.Click += new System.EventHandler(this.button_send_Click);
            // 
            // textBox_input
            // 
            this.textBox_input.AcceptsReturn = true;
            this.textBox_input.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_input.Location = new System.Drawing.Point(7, 7);
            this.textBox_input.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_input.MinimumSize = new System.Drawing.Size(88, 4);
            this.textBox_input.Multiline = true;
            this.textBox_input.Name = "textBox_input";
            this.textBox_input.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_input.Size = new System.Drawing.Size(633, 106);
            this.textBox_input.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_searchBiblio});
            this.toolStrip1.Location = new System.Drawing.Point(0, 564);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1012, 38);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_searchBiblio
            // 
            this.toolStripButton_searchBiblio.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_searchBiblio.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_searchBiblio.Image")));
            this.toolStripButton_searchBiblio.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_searchBiblio.Name = "toolStripButton_searchBiblio";
            this.toolStripButton_searchBiblio.Size = new System.Drawing.Size(100, 32);
            this.toolStripButton_searchBiblio.Text = "书目检索";
            this.toolStripButton_searchBiblio.Click += new System.EventHandler(this.toolStripButton_searchBiblio_Click);
            // 
            // splitContainer_message
            // 
            this.splitContainer_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_message.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_message.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.splitContainer_message.Name = "splitContainer_message";
            this.splitContainer_message.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_message.Panel1
            // 
            this.splitContainer_message.Panel1.Controls.Add(this.webBrowser1);
            // 
            // splitContainer_message.Panel2
            // 
            this.splitContainer_message.Panel2.Controls.Add(this.panel_input);
            this.splitContainer_message.Size = new System.Drawing.Size(798, 564);
            this.splitContainer_message.SplitterDistance = 434;
            this.splitContainer_message.SplitterWidth = 14;
            this.splitContainer_message.TabIndex = 3;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.dpTable_groups);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.splitContainer_message);
            this.splitContainer_main.Size = new System.Drawing.Size(1012, 564);
            this.splitContainer_main.SplitterDistance = 199;
            this.splitContainer_main.SplitterWidth = 15;
            this.splitContainer_main.TabIndex = 4;
            // 
            // dpTable_groups
            // 
            this.dpTable_groups.AutoDocCenter = true;
            this.dpTable_groups.Columns.Add(this.dpColumn_icon);
            this.dpTable_groups.Columns.Add(this.dpColumn_name);
            this.dpTable_groups.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_groups.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_groups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable_groups.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.dpTable_groups.DocumentOrgX = ((long)(0));
            this.dpTable_groups.DocumentOrgY = ((long)(0));
            this.dpTable_groups.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable_groups.FocusedItem = null;
            this.dpTable_groups.FullRowSelect = true;
            this.dpTable_groups.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_groups.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_groups.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_groups.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_groups.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_groups.Location = new System.Drawing.Point(0, 0);
            this.dpTable_groups.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.dpTable_groups.MaxTextHeight = 57;
            this.dpTable_groups.Name = "dpTable_groups";
            this.dpTable_groups.Size = new System.Drawing.Size(199, 564);
            this.dpTable_groups.TabIndex = 0;
            this.dpTable_groups.Text = "dpTable1";
            this.dpTable_groups.SelectionChanged += new System.EventHandler(this.dpTable_groups_SelectionChanged);
            this.dpTable_groups.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dpTable_groups_MouseUp);
            // 
            // dpColumn_icon
            // 
            this.dpColumn_icon.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_icon.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_icon.Font = null;
            this.dpColumn_icon.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_icon.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_icon.Width = 50;
            // 
            // dpColumn_name
            // 
            this.dpColumn_name.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_name.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_name.Font = null;
            this.dpColumn_name.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_name.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_name.Width = 200;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 602);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "ChatForm";
            this.Text = "聊天";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IMForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IMForm_FormClosed);
            this.Load += new System.EventHandler(this.IMForm_Load);
            this.panel_input.ResumeLayout(false);
            this.panel_input.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer_message.Panel1.ResumeLayout(false);
            this.splitContainer_message.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_message)).EndInit();
            this.splitContainer_message.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panel_input;
        private System.Windows.Forms.Button button_send;
        private System.Windows.Forms.TextBox textBox_input;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_searchBiblio;
        private System.Windows.Forms.SplitContainer splitContainer_message;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private DigitalPlatform.CommonControl.DpTable dpTable_groups;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_icon;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_name;
    }
}