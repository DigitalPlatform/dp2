namespace DigitalPlatform.CirculationClient
{
    partial class KernelCfgFileDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KernelCfgFileDialog));
            this.textBox_serverUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_mime = new System.Windows.Forms.TextBox();
            this.textBox_path = new System.Windows.Forms.TextBox();
            this.textBox_content = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_property = new System.Windows.Forms.TabPage();
            this.tabPage_content = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_formatXml = new System.Windows.Forms.ToolStripButton();
            this.tabControl_main.SuspendLayout();
            this.tabPage_property.SuspendLayout();
            this.tabPage_content.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_serverUrl
            // 
            this.textBox_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_serverUrl.Location = new System.Drawing.Point(82, 6);
            this.textBox_serverUrl.Name = "textBox_serverUrl";
            this.textBox_serverUrl.ReadOnly = true;
            this.textBox_serverUrl.Size = new System.Drawing.Size(444, 21);
            this.textBox_serverUrl.TabIndex = 26;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 25;
            this.label3.Text = "服务器(&U):";
            // 
            // textBox_mime
            // 
            this.textBox_mime.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_mime.Location = new System.Drawing.Point(82, 55);
            this.textBox_mime.Name = "textBox_mime";
            this.textBox_mime.Size = new System.Drawing.Size(192, 21);
            this.textBox_mime.TabIndex = 17;
            // 
            // textBox_path
            // 
            this.textBox_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_path.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_path.Location = new System.Drawing.Point(82, 30);
            this.textBox_path.Name = "textBox_path";
            this.textBox_path.Size = new System.Drawing.Size(444, 21);
            this.textBox_path.TabIndex = 15;
            // 
            // textBox_content
            // 
            this.textBox_content.AcceptsReturn = true;
            this.textBox_content.AcceptsTab = true;
            this.textBox_content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_content.HideSelection = false;
            this.textBox_content.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_content.Location = new System.Drawing.Point(3, 3);
            this.textBox_content.MaxLength = 2000000000;
            this.textBox_content.Multiline = true;
            this.textBox_content.Name = "textBox_content";
            this.textBox_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_content.Size = new System.Drawing.Size(526, 332);
            this.textBox_content.TabIndex = 18;
            this.textBox_content.TextChanged += new System.EventHandler(this.textBox_content_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "&MIME:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 14;
            this.label1.Text = "路径(&P):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(477, 382);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 24;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(396, 382);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 24);
            this.button_OK.TabIndex = 23;
            this.button_OK.Text = "保存";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_property);
            this.tabControl_main.Controls.Add(this.tabPage_content);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(540, 364);
            this.tabControl_main.TabIndex = 27;
            // 
            // tabPage_property
            // 
            this.tabPage_property.Controls.Add(this.textBox_serverUrl);
            this.tabPage_property.Controls.Add(this.label1);
            this.tabPage_property.Controls.Add(this.label3);
            this.tabPage_property.Controls.Add(this.label2);
            this.tabPage_property.Controls.Add(this.textBox_mime);
            this.tabPage_property.Controls.Add(this.textBox_path);
            this.tabPage_property.Location = new System.Drawing.Point(4, 22);
            this.tabPage_property.Name = "tabPage_property";
            this.tabPage_property.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_property.Size = new System.Drawing.Size(532, 338);
            this.tabPage_property.TabIndex = 0;
            this.tabPage_property.Text = "属性";
            this.tabPage_property.UseVisualStyleBackColor = true;
            // 
            // tabPage_content
            // 
            this.tabPage_content.Controls.Add(this.textBox_content);
            this.tabPage_content.Location = new System.Drawing.Point(4, 22);
            this.tabPage_content.Name = "tabPage_content";
            this.tabPage_content.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_content.Size = new System.Drawing.Size(532, 338);
            this.tabPage_content.TabIndex = 1;
            this.tabPage_content.Text = "内容";
            this.tabPage_content.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_formatXml});
            this.toolStrip1.Location = new System.Drawing.Point(12, 375);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(78, 25);
            this.toolStrip1.TabIndex = 28;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_formatXml
            // 
            this.toolStripButton_formatXml.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_formatXml.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_formatXml.Image")));
            this.toolStripButton_formatXml.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_formatXml.Name = "toolStripButton_formatXml";
            this.toolStripButton_formatXml.Size = new System.Drawing.Size(66, 22);
            this.toolStripButton_formatXml.Text = "整理 XML";
            this.toolStripButton_formatXml.Click += new System.EventHandler(this.toolStripButton_formatXml_Click);
            // 
            // KernelCfgFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(564, 416);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "KernelCfgFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "KernelCfgFileDialog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KernelCfgFileDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KernelCfgFileDialog_FormClosed);
            this.Load += new System.EventHandler(this.KernelCfgFileDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_property.ResumeLayout(false);
            this.tabPage_property.PerformLayout();
            this.tabPage_content.ResumeLayout(false);
            this.tabPage_content.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox textBox_serverUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_mime;
        public System.Windows.Forms.TextBox textBox_path;
        public System.Windows.Forms.TextBox textBox_content;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_property;
        private System.Windows.Forms.TabPage tabPage_content;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_formatXml;
    }
}