namespace dp2Circulation
{
    partial class CommentViewerForm
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
            this.ExitWebBrowser();
            this.DisposeFreeControls();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommentViewerForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dock = new System.Windows.Forms.ToolStripButton();
            this.webBrowser_html = new System.Windows.Forms.WebBrowser();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_html = new System.Windows.Forms.TabPage();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.webBrowser_xml = new System.Windows.Forms.WebBrowser();
            this.tabPage_subrecords = new System.Windows.Forms.TabPage();
            this.webBrowser_subrecords = new System.Windows.Forms.WebBrowser();
            this.toolStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.tabPage_subrecords.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dock});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(611, 38);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_dock
            // 
            this.toolStripButton_dock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_dock.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dock.Image")));
            this.toolStripButton_dock.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_dock.Name = "toolStripButton_dock";
            this.toolStripButton_dock.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_dock.Text = "停靠到固定面板";
            this.toolStripButton_dock.Click += new System.EventHandler(this.toolStripButton_dock_Click);
            // 
            // webBrowser_html
            // 
            this.webBrowser_html.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_html.Location = new System.Drawing.Point(6, 5);
            this.webBrowser_html.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser_html.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_html.Name = "webBrowser_html";
            this.webBrowser_html.Size = new System.Drawing.Size(591, 379);
            this.webBrowser_html.TabIndex = 1;
            this.webBrowser_html.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_html_DocumentCompleted);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_html);
            this.tabControl_main.Controls.Add(this.tabPage_xml);
            this.tabControl_main.Controls.Add(this.tabPage_subrecords);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 38);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(611, 424);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser_html);
            this.tabPage_html.Location = new System.Drawing.Point(4, 31);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_html.Size = new System.Drawing.Size(603, 389);
            this.tabPage_html.TabIndex = 0;
            this.tabPage_html.Text = "HTML";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.Controls.Add(this.webBrowser_xml);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 31);
            this.tabPage_xml.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_xml.Size = new System.Drawing.Size(603, 389);
            this.tabPage_xml.TabIndex = 1;
            this.tabPage_xml.Text = "XML";
            this.tabPage_xml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_xml
            // 
            this.webBrowser_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_xml.Location = new System.Drawing.Point(6, 5);
            this.webBrowser_xml.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser_xml.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_xml.Name = "webBrowser_xml";
            this.webBrowser_xml.Size = new System.Drawing.Size(591, 379);
            this.webBrowser_xml.TabIndex = 0;
            // 
            // tabPage_subrecords
            // 
            this.tabPage_subrecords.Controls.Add(this.webBrowser_subrecords);
            this.tabPage_subrecords.Location = new System.Drawing.Point(4, 31);
            this.tabPage_subrecords.Name = "tabPage_subrecords";
            this.tabPage_subrecords.Size = new System.Drawing.Size(603, 389);
            this.tabPage_subrecords.TabIndex = 2;
            this.tabPage_subrecords.Text = "子记录";
            this.tabPage_subrecords.UseVisualStyleBackColor = true;
            // 
            // webBrowser_subrecords
            // 
            this.webBrowser_subrecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_subrecords.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_subrecords.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser_subrecords.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_subrecords.Name = "webBrowser_subrecords";
            this.webBrowser_subrecords.Size = new System.Drawing.Size(603, 389);
            this.webBrowser_subrecords.TabIndex = 2;
            this.webBrowser_subrecords.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_subrecords_DocumentCompleted);
            // 
            // CommentViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 462);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "CommentViewerForm";
            this.ShowInTaskbar = false;
            this.Text = "CommentViewerForm";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_html.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.tabPage_subrecords.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_dock;
        private System.Windows.Forms.WebBrowser webBrowser_html;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_html;
        private System.Windows.Forms.TabPage tabPage_xml;
        private System.Windows.Forms.WebBrowser webBrowser_xml;
        private System.Windows.Forms.TabPage tabPage_subrecords;
        private System.Windows.Forms.WebBrowser webBrowser_subrecords;
    }
}