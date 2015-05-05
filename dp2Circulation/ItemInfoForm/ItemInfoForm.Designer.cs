namespace dp2Circulation
{
    partial class ItemInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemInfoForm));
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_item = new System.Windows.Forms.TabControl();
            this.tabPage_html = new System.Windows.Forms.TabPage();
            this.webBrowser_itemHTML = new System.Windows.Forms.WebBrowser();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.webBrowser_itemXml = new System.Windows.Forms.WebBrowser();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.button_load = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_nextRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prevRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_message = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_addSubject = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_item.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 37);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_item);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser_biblio);
            this.splitContainer_main.Size = new System.Drawing.Size(437, 258);
            this.splitContainer_main.SplitterDistance = 160;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 0;
            // 
            // tabControl_item
            // 
            this.tabControl_item.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_item.Controls.Add(this.tabPage_html);
            this.tabControl_item.Controls.Add(this.tabPage_xml);
            this.tabControl_item.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_item.Location = new System.Drawing.Point(0, 0);
            this.tabControl_item.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_item.Name = "tabControl_item";
            this.tabControl_item.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_item.SelectedIndex = 0;
            this.tabControl_item.Size = new System.Drawing.Size(160, 258);
            this.tabControl_item.TabIndex = 1;
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser_itemHTML);
            this.tabPage_html.Location = new System.Drawing.Point(4, 25);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Size = new System.Drawing.Size(152, 229);
            this.tabPage_html.TabIndex = 0;
            this.tabPage_html.Text = "HTML";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // webBrowser_itemHTML
            // 
            this.webBrowser_itemHTML.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_itemHTML.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_itemHTML.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_itemHTML.Name = "webBrowser_itemHTML";
            this.webBrowser_itemHTML.Size = new System.Drawing.Size(152, 229);
            this.webBrowser_itemHTML.TabIndex = 0;
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.Controls.Add(this.webBrowser_itemXml);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 25);
            this.tabPage_xml.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Size = new System.Drawing.Size(152, 229);
            this.tabPage_xml.TabIndex = 1;
            this.tabPage_xml.Text = "XML";
            this.tabPage_xml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_itemXml
            // 
            this.webBrowser_itemXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_itemXml.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_itemXml.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_itemXml.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_itemXml.Name = "webBrowser_itemXml";
            this.webBrowser_itemXml.Size = new System.Drawing.Size(154, 235);
            this.webBrowser_itemXml.TabIndex = 0;
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(271, 258);
            this.webBrowser_biblio.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "途径(&F):";
            // 
            // comboBox_from
            // 
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.Items.AddRange(new object[] {
            "册条码",
            "册记录路径"});
            this.comboBox_from.Location = new System.Drawing.Point(74, 10);
            this.comboBox_from.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(87, 20);
            this.comboBox_from.TabIndex = 3;
            this.comboBox_from.Text = "册条码";
            this.comboBox_from.DropDown += new System.EventHandler(this.comboBox_from_DropDown);
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_queryWord.Location = new System.Drawing.Point(166, 10);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(204, 21);
            this.textBox_queryWord.TabIndex = 4;
            // 
            // button_load
            // 
            this.button_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_load.Location = new System.Drawing.Point(374, 10);
            this.button_load.Margin = new System.Windows.Forms.Padding(2);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(63, 22);
            this.button_load.TabIndex = 5;
            this.button_load.Text = "装载(&L)";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_nextRecord,
            this.toolStripButton_prevRecord,
            this.toolStripLabel_message,
            this.toolStripButton_addSubject});
            this.toolStrip1.Location = new System.Drawing.Point(0, 298);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(437, 20);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_nextRecord
            // 
            this.toolStripButton_nextRecord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_nextRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_nextRecord.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_nextRecord.Image")));
            this.toolStripButton_nextRecord.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_nextRecord.Name = "toolStripButton_nextRecord";
            this.toolStripButton_nextRecord.Size = new System.Drawing.Size(23, 17);
            this.toolStripButton_nextRecord.Text = "后一记录";
            this.toolStripButton_nextRecord.Click += new System.EventHandler(this.toolStripButton_nextRecord_Click);
            // 
            // toolStripButton_prevRecord
            // 
            this.toolStripButton_prevRecord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_prevRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_prevRecord.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prevRecord.Image")));
            this.toolStripButton_prevRecord.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_prevRecord.Name = "toolStripButton_prevRecord";
            this.toolStripButton_prevRecord.Size = new System.Drawing.Size(23, 17);
            this.toolStripButton_prevRecord.Text = "前一记录";
            this.toolStripButton_prevRecord.Click += new System.EventHandler(this.toolStripButton_prevRecord_Click);
            // 
            // toolStripLabel_message
            // 
            this.toolStripLabel_message.Name = "toolStripLabel_message";
            this.toolStripLabel_message.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripLabel_message.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripButton_addSubject
            // 
            this.toolStripButton_addSubject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_addSubject.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_addSubject.Image")));
            this.toolStripButton_addSubject.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_addSubject.Name = "toolStripButton_addSubject";
            this.toolStripButton_addSubject.Size = new System.Drawing.Size(23, 17);
            this.toolStripButton_addSubject.Text = "增添自由词";
            this.toolStripButton_addSubject.Visible = false;
            this.toolStripButton_addSubject.Click += new System.EventHandler(this.toolStripButton_addSubject_Click);
            // 
            // ItemInfoForm
            // 
            this.AcceptButton = this.button_load;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 326);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.textBox_queryWord);
            this.Controls.Add(this.comboBox_from);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ItemInfoForm";
            this.ShowInTaskbar = false;
            this.Text = "实体";
            this.Activated += new System.EventHandler(this.ItemInfoForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ItemInfoForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ItemInfoForm_FormClosed);
            this.Load += new System.EventHandler(this.ItemInfoForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_item.ResumeLayout(false);
            this.tabPage_html.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser_itemHTML;
        private System.Windows.Forms.WebBrowser webBrowser_biblio;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_from;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.TabControl tabControl_item;
        private System.Windows.Forms.TabPage tabPage_html;
        private System.Windows.Forms.TabPage tabPage_xml;
        private System.Windows.Forms.WebBrowser webBrowser_itemXml;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_prevRecord;
        private System.Windows.Forms.ToolStripButton toolStripButton_nextRecord;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_message;
        private System.Windows.Forms.ToolStripButton toolStripButton_addSubject;
    }
}