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

            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Dispose();
            if (this.m_webExternalHost_item != null)
                this.m_webExternalHost_item.Dispose();
            if (this.m_chargingInterface != null)
                this.m_chargingInterface.Dispose();

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
            this.tabPage_borrowHistory = new System.Windows.Forms.TabPage();
            this.webBrowser_borrowHistory = new System.Windows.Forms.WebBrowser();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.webBrowser_itemXml = new System.Windows.Forms.WebBrowser();
            this.tabPage_object = new System.Windows.Forms.TabPage();
            this.binaryResControl1 = new DigitalPlatform.CirculationClient.BinaryResControl();
            this.tabPage_editor = new System.Windows.Forms.TabPage();
            this.textBox_editor = new System.Windows.Forms.TextBox();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.button_load = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButton_insertCoverImage = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolStripMenuItem_insertCoverImageFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_clearCoverImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_addSubject = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_nextRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prevRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_message = new System.Windows.Forms.ToolStripLabel();
            this.toolStripDropDownButton_edit = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_pasteXmlRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_edit_indentXml = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_edit_removeEmptyElements = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_item.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.tabPage_borrowHistory.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.tabPage_object.SuspendLayout();
            this.tabPage_editor.SuspendLayout();
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
            this.splitContainer_main.Size = new System.Drawing.Size(513, 258);
            this.splitContainer_main.SplitterDistance = 187;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 0;
            // 
            // tabControl_item
            // 
            this.tabControl_item.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_item.Controls.Add(this.tabPage_html);
            this.tabControl_item.Controls.Add(this.tabPage_borrowHistory);
            this.tabControl_item.Controls.Add(this.tabPage_xml);
            this.tabControl_item.Controls.Add(this.tabPage_object);
            this.tabControl_item.Controls.Add(this.tabPage_editor);
            this.tabControl_item.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_item.Location = new System.Drawing.Point(0, 0);
            this.tabControl_item.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_item.Name = "tabControl_item";
            this.tabControl_item.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_item.SelectedIndex = 0;
            this.tabControl_item.Size = new System.Drawing.Size(187, 258);
            this.tabControl_item.TabIndex = 1;
            this.tabControl_item.SelectedIndexChanged += new System.EventHandler(this.tabControl_item_SelectedIndexChanged);
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser_itemHTML);
            this.tabPage_html.Location = new System.Drawing.Point(4, 25);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Size = new System.Drawing.Size(179, 229);
            this.tabPage_html.TabIndex = 0;
            this.tabPage_html.Text = "常规";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // webBrowser_itemHTML
            // 
            this.webBrowser_itemHTML.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_itemHTML.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_itemHTML.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_itemHTML.Name = "webBrowser_itemHTML";
            this.webBrowser_itemHTML.Size = new System.Drawing.Size(179, 229);
            this.webBrowser_itemHTML.TabIndex = 0;
            // 
            // tabPage_borrowHistory
            // 
            this.tabPage_borrowHistory.Controls.Add(this.webBrowser_borrowHistory);
            this.tabPage_borrowHistory.Location = new System.Drawing.Point(4, 25);
            this.tabPage_borrowHistory.Name = "tabPage_borrowHistory";
            this.tabPage_borrowHistory.Size = new System.Drawing.Size(179, 229);
            this.tabPage_borrowHistory.TabIndex = 2;
            this.tabPage_borrowHistory.Text = "借阅历史";
            this.tabPage_borrowHistory.UseVisualStyleBackColor = true;
            // 
            // webBrowser_borrowHistory
            // 
            this.webBrowser_borrowHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_borrowHistory.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_borrowHistory.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_borrowHistory.Name = "webBrowser_borrowHistory";
            this.webBrowser_borrowHistory.Size = new System.Drawing.Size(179, 229);
            this.webBrowser_borrowHistory.TabIndex = 0;
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.Controls.Add(this.webBrowser_itemXml);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 25);
            this.tabPage_xml.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Size = new System.Drawing.Size(179, 229);
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
            this.webBrowser_itemXml.Size = new System.Drawing.Size(179, 229);
            this.webBrowser_itemXml.TabIndex = 0;
            // 
            // tabPage_object
            // 
            this.tabPage_object.Controls.Add(this.binaryResControl1);
            this.tabPage_object.Location = new System.Drawing.Point(4, 25);
            this.tabPage_object.Name = "tabPage_object";
            this.tabPage_object.Size = new System.Drawing.Size(179, 229);
            this.tabPage_object.TabIndex = 3;
            this.tabPage_object.Text = "对象";
            this.tabPage_object.UseVisualStyleBackColor = true;
            // 
            // binaryResControl1
            // 
            this.binaryResControl1.BiblioRecPath = "";
            this.binaryResControl1.Changed = false;
            this.binaryResControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryResControl1.ErrorInfo = "";
            this.binaryResControl1.Location = new System.Drawing.Point(0, 0);
            this.binaryResControl1.Margin = new System.Windows.Forms.Padding(2);
            this.binaryResControl1.Name = "binaryResControl1";
            this.binaryResControl1.RightsCfgFileName = null;
            this.binaryResControl1.Size = new System.Drawing.Size(179, 229);
            this.binaryResControl1.TabIndex = 0;
            this.binaryResControl1.TempDir = null;
            // 
            // tabPage_editor
            // 
            this.tabPage_editor.Controls.Add(this.textBox_editor);
            this.tabPage_editor.Location = new System.Drawing.Point(4, 25);
            this.tabPage_editor.Name = "tabPage_editor";
            this.tabPage_editor.Size = new System.Drawing.Size(179, 229);
            this.tabPage_editor.TabIndex = 4;
            this.tabPage_editor.Text = "编辑器";
            this.tabPage_editor.UseVisualStyleBackColor = true;
            // 
            // textBox_editor
            // 
            this.textBox_editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_editor.Location = new System.Drawing.Point(0, 0);
            this.textBox_editor.MaxLength = 0;
            this.textBox_editor.Multiline = true;
            this.textBox_editor.Name = "textBox_editor";
            this.textBox_editor.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_editor.Size = new System.Drawing.Size(179, 229);
            this.textBox_editor.TabIndex = 0;
            this.textBox_editor.TextChanged += new System.EventHandler(this.textBox_editor_TextChanged);
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(320, 258);
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
            this.textBox_queryWord.Size = new System.Drawing.Size(280, 21);
            this.textBox_queryWord.TabIndex = 4;
            // 
            // button_load
            // 
            this.button_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_load.Location = new System.Drawing.Point(450, 10);
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
            this.toolStripButton_save,
            this.toolStripSplitButton_insertCoverImage,
            this.toolStripButton_addSubject,
            this.toolStripButton_nextRecord,
            this.toolStripButton_prevRecord,
            this.toolStripLabel_message,
            this.toolStripDropDownButton_edit});
            this.toolStrip1.Location = new System.Drawing.Point(0, 298);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(513, 20);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_save
            // 
            this.toolStripButton_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_save.Enabled = false;
            this.toolStripButton_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_save.Image")));
            this.toolStripButton_save.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_save.Name = "toolStripButton_save";
            this.toolStripButton_save.Size = new System.Drawing.Size(23, 17);
            this.toolStripButton_save.Text = "保存";
            this.toolStripButton_save.Click += new System.EventHandler(this.toolStripButton_save_Click);
            // 
            // toolStripSplitButton_insertCoverImage
            // 
            this.toolStripSplitButton_insertCoverImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton_insertCoverImage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_insertCoverImageFromClipboard,
            this.ToolStripMenuItem_clearCoverImage});
            this.toolStripSplitButton_insertCoverImage.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_insertCoverImage.Image")));
            this.toolStripSplitButton_insertCoverImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_insertCoverImage.Name = "toolStripSplitButton_insertCoverImage";
            this.toolStripSplitButton_insertCoverImage.Size = new System.Drawing.Size(32, 17);
            this.toolStripSplitButton_insertCoverImage.Text = "toolStripSplitButton1";
            this.toolStripSplitButton_insertCoverImage.ButtonClick += new System.EventHandler(this.toolStripSplitButton_insertCoverImage_ButtonClick);
            // 
            // ToolStripMenuItem_insertCoverImageFromClipboard
            // 
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Name = "ToolStripMenuItem_insertCoverImageFromClipboard";
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Size = new System.Drawing.Size(196, 22);
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Text = "从剪贴板插入封面图像";
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_insertCoverImageFromClipboard_Click);
            // 
            // ToolStripMenuItem_clearCoverImage
            // 
            this.ToolStripMenuItem_clearCoverImage.Name = "ToolStripMenuItem_clearCoverImage";
            this.ToolStripMenuItem_clearCoverImage.Size = new System.Drawing.Size(196, 22);
            this.ToolStripMenuItem_clearCoverImage.Text = "清除封面图像";
            this.ToolStripMenuItem_clearCoverImage.Click += new System.EventHandler(this.ToolStripMenuItem_clearCoverImage_Click);
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
            // toolStripDropDownButton_edit
            // 
            this.toolStripDropDownButton_edit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_edit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_pasteXmlRecord,
            this.ToolStripMenuItem_edit_indentXml,
            this.ToolStripMenuItem_edit_removeEmptyElements});
            this.toolStripDropDownButton_edit.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_edit.Image")));
            this.toolStripDropDownButton_edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_edit.Name = "toolStripDropDownButton_edit";
            this.toolStripDropDownButton_edit.Size = new System.Drawing.Size(45, 17);
            this.toolStripDropDownButton_edit.Text = "编辑";
            // 
            // ToolStripMenuItem_pasteXmlRecord
            // 
            this.ToolStripMenuItem_pasteXmlRecord.Name = "ToolStripMenuItem_pasteXmlRecord";
            this.ToolStripMenuItem_pasteXmlRecord.Size = new System.Drawing.Size(206, 22);
            this.ToolStripMenuItem_pasteXmlRecord.Text = "从剪贴板粘贴 XML 记录";
            this.ToolStripMenuItem_pasteXmlRecord.Click += new System.EventHandler(this.ToolStripMenuItem_pasteXmlRecord_Click);
            // 
            // ToolStripMenuItem_edit_indentXml
            // 
            this.ToolStripMenuItem_edit_indentXml.Name = "ToolStripMenuItem_edit_indentXml";
            this.ToolStripMenuItem_edit_indentXml.Size = new System.Drawing.Size(206, 22);
            this.ToolStripMenuItem_edit_indentXml.Text = "规整 XML";
            this.ToolStripMenuItem_edit_indentXml.Click += new System.EventHandler(this.ToolStripMenuItem_edit_indentXml_Click);
            // 
            // ToolStripMenuItem_edit_removeEmptyElements
            // 
            this.ToolStripMenuItem_edit_removeEmptyElements.Name = "ToolStripMenuItem_edit_removeEmptyElements";
            this.ToolStripMenuItem_edit_removeEmptyElements.Size = new System.Drawing.Size(206, 22);
            this.ToolStripMenuItem_edit_removeEmptyElements.Text = "删除空元素";
            this.ToolStripMenuItem_edit_removeEmptyElements.Click += new System.EventHandler(this.ToolStripMenuItem_edit_removeEmptyElements_Click);
            // 
            // ItemInfoForm
            // 
            this.AcceptButton = this.button_load;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 326);
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
            this.tabPage_borrowHistory.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.tabPage_object.ResumeLayout(false);
            this.tabPage_editor.ResumeLayout(false);
            this.tabPage_editor.PerformLayout();
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
        private System.Windows.Forms.TabPage tabPage_borrowHistory;
        private System.Windows.Forms.WebBrowser webBrowser_borrowHistory;
        private System.Windows.Forms.TabPage tabPage_object;
        private DigitalPlatform.CirculationClient.BinaryResControl binaryResControl1;
        private System.Windows.Forms.ToolStripButton toolStripButton_save;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_insertCoverImage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_insertCoverImageFromClipboard;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_clearCoverImage;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_edit;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_pasteXmlRecord;
        private System.Windows.Forms.TabPage tabPage_editor;
        private System.Windows.Forms.TextBox textBox_editor;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_edit_indentXml;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_edit_removeEmptyElements;
    }
}