namespace dp2Catalog
{
    partial class XmlDetailForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmlDetailForm));
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_savePath = new System.Windows.Forms.TextBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_xmlEditor = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_plainText = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip_xmlEditor = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_indentXmlText = new System.Windows.Forms.ToolStripButton();
            this.textBox_xml = new System.Windows.Forms.TextBox();
            this.tabPage_xmlDisplay = new System.Windows.Forms.TabPage();
            this.webBrowser_xml = new System.Windows.Forms.WebBrowser();
            this.tabPage_originData = new System.Windows.Forms.TabPage();
            this.splitContainer_originDataMain = new System.Windows.Forms.SplitContainer();
            this.panel_originDataText = new System.Windows.Forms.Panel();
            this.panel_origin_up = new System.Windows.Forms.Panel();
            this.textBox_originDatabaseName = new System.Windows.Forms.TextBox();
            this.textBox_originMarcSyntaxOID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_originDataEncoding = new System.Windows.Forms.ComboBox();
            this.textBox_originData = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.binaryEditor_originData = new DigitalPlatform.CommonControl.BinaryEditor();
            this.textBox_tempRecPath = new System.Windows.Forms.TextBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_xmlEditor.SuspendLayout();
            this.tableLayoutPanel_plainText.SuspendLayout();
            this.toolStrip_xmlEditor.SuspendLayout();
            this.tabPage_xmlDisplay.SuspendLayout();
            this.tabPage_originData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_originDataMain)).BeginInit();
            this.splitContainer_originDataMain.Panel1.SuspendLayout();
            this.splitContainer_originDataMain.Panel2.SuspendLayout();
            this.splitContainer_originDataMain.SuspendLayout();
            this.panel_originDataText.SuspendLayout();
            this.panel_origin_up.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2, 7);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "记录路径(&P):";
            // 
            // textBox_savePath
            // 
            this.textBox_savePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_savePath.Location = new System.Drawing.Point(77, 5);
            this.textBox_savePath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_savePath.Name = "textBox_savePath";
            this.textBox_savePath.Size = new System.Drawing.Size(330, 21);
            this.textBox_savePath.TabIndex = 5;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_xmlEditor);
            this.tabControl_main.Controls.Add(this.tabPage_xmlDisplay);
            this.tabControl_main.Controls.Add(this.tabPage_originData);
            this.tabControl_main.Location = new System.Drawing.Point(1, 30);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(410, 270);
            this.tabControl_main.TabIndex = 6;
            // 
            // tabPage_xmlEditor
            // 
            this.tabPage_xmlEditor.Controls.Add(this.tableLayoutPanel_plainText);
            this.tabPage_xmlEditor.Location = new System.Drawing.Point(4, 22);
            this.tabPage_xmlEditor.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_xmlEditor.Name = "tabPage_xmlEditor";
            this.tabPage_xmlEditor.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_xmlEditor.Size = new System.Drawing.Size(402, 244);
            this.tabPage_xmlEditor.TabIndex = 0;
            this.tabPage_xmlEditor.Text = "XML编辑";
            this.tabPage_xmlEditor.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_plainText
            // 
            this.tableLayoutPanel_plainText.ColumnCount = 1;
            this.tableLayoutPanel_plainText.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_plainText.Controls.Add(this.toolStrip_xmlEditor, 0, 0);
            this.tableLayoutPanel_plainText.Controls.Add(this.textBox_xml, 0, 1);
            this.tableLayoutPanel_plainText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_plainText.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel_plainText.Name = "tableLayoutPanel_plainText";
            this.tableLayoutPanel_plainText.RowCount = 2;
            this.tableLayoutPanel_plainText.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_plainText.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_plainText.Size = new System.Drawing.Size(398, 240);
            this.tableLayoutPanel_plainText.TabIndex = 2;
            // 
            // toolStrip_xmlEditor
            // 
            this.toolStrip_xmlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_xmlEditor.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip_xmlEditor.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_xmlEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_indentXmlText});
            this.toolStrip_xmlEditor.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_xmlEditor.Name = "toolStrip_xmlEditor";
            this.toolStrip_xmlEditor.Size = new System.Drawing.Size(398, 25);
            this.toolStrip_xmlEditor.TabIndex = 1;
            this.toolStrip_xmlEditor.Text = "toolStrip1";
            // 
            // toolStripButton_indentXmlText
            // 
            this.toolStripButton_indentXmlText.CheckOnClick = true;
            this.toolStripButton_indentXmlText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_indentXmlText.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_indentXmlText.Image")));
            this.toolStripButton_indentXmlText.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_indentXmlText.Name = "toolStripButton_indentXmlText";
            this.toolStripButton_indentXmlText.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_indentXmlText.Text = "整理XML格式";
            this.toolStripButton_indentXmlText.Click += new System.EventHandler(this.toolStripButton_indentXmlText_Click);
            // 
            // textBox_xml
            // 
            this.textBox_xml.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_xml.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_xml.Location = new System.Drawing.Point(2, 27);
            this.textBox_xml.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_xml.Multiline = true;
            this.textBox_xml.Name = "textBox_xml";
            this.textBox_xml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_xml.Size = new System.Drawing.Size(394, 211);
            this.textBox_xml.TabIndex = 0;
            // 
            // tabPage_xmlDisplay
            // 
            this.tabPage_xmlDisplay.Controls.Add(this.webBrowser_xml);
            this.tabPage_xmlDisplay.Location = new System.Drawing.Point(4, 22);
            this.tabPage_xmlDisplay.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_xmlDisplay.Name = "tabPage_xmlDisplay";
            this.tabPage_xmlDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_xmlDisplay.Size = new System.Drawing.Size(402, 244);
            this.tabPage_xmlDisplay.TabIndex = 2;
            this.tabPage_xmlDisplay.Text = "XML只读";
            this.tabPage_xmlDisplay.UseVisualStyleBackColor = true;
            // 
            // webBrowser_xml
            // 
            this.webBrowser_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_xml.Location = new System.Drawing.Point(3, 3);
            this.webBrowser_xml.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_xml.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_xml.Name = "webBrowser_xml";
            this.webBrowser_xml.Size = new System.Drawing.Size(396, 238);
            this.webBrowser_xml.TabIndex = 0;
            // 
            // tabPage_originData
            // 
            this.tabPage_originData.Controls.Add(this.splitContainer_originDataMain);
            this.tabPage_originData.Location = new System.Drawing.Point(4, 22);
            this.tabPage_originData.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_originData.Name = "tabPage_originData";
            this.tabPage_originData.Size = new System.Drawing.Size(402, 244);
            this.tabPage_originData.TabIndex = 1;
            this.tabPage_originData.Text = "原始数据";
            this.tabPage_originData.UseVisualStyleBackColor = true;
            // 
            // splitContainer_originDataMain
            // 
            this.splitContainer_originDataMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_originDataMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_originDataMain.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_originDataMain.Name = "splitContainer_originDataMain";
            this.splitContainer_originDataMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_originDataMain.Panel1
            // 
            this.splitContainer_originDataMain.Panel1.Controls.Add(this.panel_originDataText);
            // 
            // splitContainer_originDataMain.Panel2
            // 
            this.splitContainer_originDataMain.Panel2.Controls.Add(this.binaryEditor_originData);
            this.splitContainer_originDataMain.Size = new System.Drawing.Size(402, 244);
            this.splitContainer_originDataMain.SplitterDistance = 173;
            this.splitContainer_originDataMain.SplitterWidth = 3;
            this.splitContainer_originDataMain.TabIndex = 0;
            // 
            // panel_originDataText
            // 
            this.panel_originDataText.Controls.Add(this.panel_origin_up);
            this.panel_originDataText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_originDataText.Location = new System.Drawing.Point(0, 0);
            this.panel_originDataText.Margin = new System.Windows.Forms.Padding(2);
            this.panel_originDataText.Name = "panel_originDataText";
            this.panel_originDataText.Size = new System.Drawing.Size(402, 173);
            this.panel_originDataText.TabIndex = 1;
            // 
            // panel_origin_up
            // 
            this.panel_origin_up.Controls.Add(this.textBox_originDatabaseName);
            this.panel_origin_up.Controls.Add(this.textBox_originMarcSyntaxOID);
            this.panel_origin_up.Controls.Add(this.label3);
            this.panel_origin_up.Controls.Add(this.label2);
            this.panel_origin_up.Controls.Add(this.comboBox_originDataEncoding);
            this.panel_origin_up.Controls.Add(this.textBox_originData);
            this.panel_origin_up.Controls.Add(this.label1);
            this.panel_origin_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_origin_up.Location = new System.Drawing.Point(0, 0);
            this.panel_origin_up.Name = "panel_origin_up";
            this.panel_origin_up.Size = new System.Drawing.Size(402, 173);
            this.panel_origin_up.TabIndex = 7;
            // 
            // textBox_originDatabaseName
            // 
            this.textBox_originDatabaseName.Location = new System.Drawing.Point(84, 2);
            this.textBox_originDatabaseName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_originDatabaseName.Name = "textBox_originDatabaseName";
            this.textBox_originDatabaseName.ReadOnly = true;
            this.textBox_originDatabaseName.Size = new System.Drawing.Size(118, 21);
            this.textBox_originDatabaseName.TabIndex = 4;
            // 
            // textBox_originMarcSyntaxOID
            // 
            this.textBox_originMarcSyntaxOID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_originMarcSyntaxOID.Location = new System.Drawing.Point(311, 2);
            this.textBox_originMarcSyntaxOID.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_originMarcSyntaxOID.Name = "textBox_originMarcSyntaxOID";
            this.textBox_originMarcSyntaxOID.ReadOnly = true;
            this.textBox_originMarcSyntaxOID.Size = new System.Drawing.Size(88, 21);
            this.textBox_originMarcSyntaxOID.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(212, 4);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "MARC格式OID(&S):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "数据库名(&D):";
            // 
            // comboBox_originDataEncoding
            // 
            this.comboBox_originDataEncoding.DropDownHeight = 300;
            this.comboBox_originDataEncoding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_originDataEncoding.FormattingEnabled = true;
            this.comboBox_originDataEncoding.IntegralHeight = false;
            this.comboBox_originDataEncoding.Location = new System.Drawing.Point(84, 24);
            this.comboBox_originDataEncoding.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_originDataEncoding.Name = "comboBox_originDataEncoding";
            this.comboBox_originDataEncoding.Size = new System.Drawing.Size(118, 20);
            this.comboBox_originDataEncoding.TabIndex = 1;
            // 
            // textBox_originData
            // 
            this.textBox_originData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_originData.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_originData.HideSelection = false;
            this.textBox_originData.Location = new System.Drawing.Point(2, 48);
            this.textBox_originData.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_originData.Multiline = true;
            this.textBox_originData.Name = "textBox_originData";
            this.textBox_originData.ReadOnly = true;
            this.textBox_originData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_originData.Size = new System.Drawing.Size(400, 123);
            this.textBox_originData.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "编码方式(&E):";
            // 
            // binaryEditor_originData
            // 
            this.binaryEditor_originData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.binaryEditor_originData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryEditor_originData.DocumentOrgX = ((long)(0));
            this.binaryEditor_originData.DocumentOrgY = ((long)(0));
            this.binaryEditor_originData.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.binaryEditor_originData.Location = new System.Drawing.Point(0, 0);
            this.binaryEditor_originData.Margin = new System.Windows.Forms.Padding(2);
            this.binaryEditor_originData.Name = "binaryEditor_originData";
            this.binaryEditor_originData.Size = new System.Drawing.Size(402, 68);
            this.binaryEditor_originData.TabIndex = 0;
            this.binaryEditor_originData.Text = "binaryEditor1";
            // 
            // textBox_tempRecPath
            // 
            this.textBox_tempRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tempRecPath.Location = new System.Drawing.Point(265, -8);
            this.textBox_tempRecPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_tempRecPath.Name = "textBox_tempRecPath";
            this.textBox_tempRecPath.Size = new System.Drawing.Size(74, 21);
            this.textBox_tempRecPath.TabIndex = 7;
            this.textBox_tempRecPath.Visible = false;
            // 
            // XmlDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 306);
            this.Controls.Add(this.textBox_tempRecPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_savePath);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "XmlDetailForm";
            this.ShowInTaskbar = false;
            this.Text = "Xml记录窗";
            this.Activated += new System.EventHandler(this.XmlDetailForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XmlDetailForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.XmlDetailForm_FormClosed);
            this.Load += new System.EventHandler(this.XmlDetailForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_xmlEditor.ResumeLayout(false);
            this.tableLayoutPanel_plainText.ResumeLayout(false);
            this.tableLayoutPanel_plainText.PerformLayout();
            this.toolStrip_xmlEditor.ResumeLayout(false);
            this.toolStrip_xmlEditor.PerformLayout();
            this.tabPage_xmlDisplay.ResumeLayout(false);
            this.tabPage_originData.ResumeLayout(false);
            this.splitContainer_originDataMain.Panel1.ResumeLayout(false);
            this.splitContainer_originDataMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_originDataMain)).EndInit();
            this.splitContainer_originDataMain.ResumeLayout(false);
            this.panel_originDataText.ResumeLayout(false);
            this.panel_origin_up.ResumeLayout(false);
            this.panel_origin_up.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_savePath;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_xmlEditor;
        private System.Windows.Forms.TabPage tabPage_originData;
        private System.Windows.Forms.SplitContainer splitContainer_originDataMain;
        private System.Windows.Forms.Panel panel_originDataText;
        private System.Windows.Forms.TextBox textBox_originMarcSyntaxOID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_originDatabaseName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_originDataEncoding;
        private System.Windows.Forms.TextBox textBox_originData;
        private DigitalPlatform.CommonControl.BinaryEditor binaryEditor_originData;
        private System.Windows.Forms.TextBox textBox_xml;
        private System.Windows.Forms.TextBox textBox_tempRecPath;
        private System.Windows.Forms.TabPage tabPage_xmlDisplay;
        private System.Windows.Forms.WebBrowser webBrowser_xml;
        private System.Windows.Forms.ToolStrip toolStrip_xmlEditor;
        private System.Windows.Forms.ToolStripButton toolStripButton_indentXmlText;
        private System.Windows.Forms.Panel panel_origin_up;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_plainText;
    }
}