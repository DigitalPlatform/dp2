namespace dp2Catalog
{
    partial class DcForm
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
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DcForm));
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_savePath = new System.Windows.Forms.TextBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_dcEditor = new System.Windows.Forms.TabPage();
            this.toolStrip_xmlEditor = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dispXmlText = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_newElement = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_deleteEelement = new System.Windows.Forms.ToolStripButton();
            this.DcEditor = new DigitalPlatform.CommonControl.DcEditor();
            this.tabPage_xmlDisplay = new System.Windows.Forms.TabPage();
            this.webBrowser_xml = new System.Windows.Forms.WebBrowser();
            this.textBox_tempRecPath = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_objectAndOther = new System.Windows.Forms.TabControl();
            this.tabPage_object = new System.Windows.Forms.TabPage();
            this.binaryResControl1 = new DigitalPlatform.CirculationClient.BinaryResControl();
            this.tabControl_main.SuspendLayout();
            this.tabPage_dcEditor.SuspendLayout();
            this.toolStrip_xmlEditor.SuspendLayout();
            this.tabPage_xmlDisplay.SuspendLayout();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_objectAndOther.SuspendLayout();
            this.tabPage_object.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-1, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "记录路径(&P):";
            // 
            // textBox_savePath
            // 
            this.textBox_savePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_savePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_savePath.Location = new System.Drawing.Point(104, 6);
            this.textBox_savePath.Name = "textBox_savePath";
            this.textBox_savePath.Size = new System.Drawing.Size(445, 25);
            this.textBox_savePath.TabIndex = 7;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_main.Controls.Add(this.tabPage_dcEditor);
            this.tabControl_main.Controls.Add(this.tabPage_xmlDisplay);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(547, 212);
            this.tabControl_main.TabIndex = 8;
            this.tabControl_main.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_main_Selected);
            // 
            // tabPage_dcEditor
            // 
            this.tabPage_dcEditor.Controls.Add(this.toolStrip_xmlEditor);
            this.tabPage_dcEditor.Controls.Add(this.DcEditor);
            this.tabPage_dcEditor.Location = new System.Drawing.Point(4, 27);
            this.tabPage_dcEditor.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_dcEditor.Name = "tabPage_dcEditor";
            this.tabPage_dcEditor.Size = new System.Drawing.Size(539, 181);
            this.tabPage_dcEditor.TabIndex = 0;
            this.tabPage_dcEditor.Text = "DC编辑";
            this.tabPage_dcEditor.UseVisualStyleBackColor = true;
            // 
            // toolStrip_xmlEditor
            // 
            this.toolStrip_xmlEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dispXmlText,
            this.toolStripButton_newElement,
            this.toolStripButton_deleteEelement});
            this.toolStrip_xmlEditor.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_xmlEditor.Name = "toolStrip_xmlEditor";
            this.toolStrip_xmlEditor.Size = new System.Drawing.Size(539, 25);
            this.toolStrip_xmlEditor.TabIndex = 1;
            this.toolStrip_xmlEditor.Text = "toolStrip1";
            // 
            // toolStripButton_dispXmlText
            // 
            this.toolStripButton_dispXmlText.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dispXmlText.Image")));
            this.toolStripButton_dispXmlText.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dispXmlText.Name = "toolStripButton_dispXmlText";
            this.toolStripButton_dispXmlText.Size = new System.Drawing.Size(116, 22);
            this.toolStripButton_dispXmlText.Text = "显示XML格式";
            this.toolStripButton_dispXmlText.Click += new System.EventHandler(this.toolStripButton_dispXmlText_Click);
            // 
            // toolStripButton_newElement
            // 
            this.toolStripButton_newElement.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newElement.Image")));
            this.toolStripButton_newElement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newElement.Name = "toolStripButton_newElement";
            this.toolStripButton_newElement.Size = new System.Drawing.Size(88, 22);
            this.toolStripButton_newElement.Text = "新增元素";
            this.toolStripButton_newElement.Click += new System.EventHandler(this.toolStripButton_newElement_Click);
            // 
            // toolStripButton_deleteEelement
            // 
            this.toolStripButton_deleteEelement.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_deleteEelement.Image")));
            this.toolStripButton_deleteEelement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_deleteEelement.Name = "toolStripButton_deleteEelement";
            this.toolStripButton_deleteEelement.Size = new System.Drawing.Size(88, 22);
            this.toolStripButton_deleteEelement.Text = "删除元素";
            this.toolStripButton_deleteEelement.Click += new System.EventHandler(this.toolStripButton_deleteEelement_Click);
            // 
            // DcEditor
            // 
            this.DcEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DcEditor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DcEditor.Changed = false;
            this.DcEditor.Location = new System.Drawing.Point(0, 28);
            this.DcEditor.Margin = new System.Windows.Forms.Padding(0);
            this.DcEditor.Name = "DcEditor";
            this.DcEditor.Size = new System.Drawing.Size(539, 153);
            this.DcEditor.TabIndex = 0;
            this.DcEditor.SelectedIndexChanged += new System.EventHandler(this.DcEditor_SelectedIndexChanged);
            // 
            // tabPage_xmlDisplay
            // 
            this.tabPage_xmlDisplay.Controls.Add(this.webBrowser_xml);
            this.tabPage_xmlDisplay.Location = new System.Drawing.Point(4, 27);
            this.tabPage_xmlDisplay.Name = "tabPage_xmlDisplay";
            this.tabPage_xmlDisplay.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_xmlDisplay.Size = new System.Drawing.Size(539, 181);
            this.tabPage_xmlDisplay.TabIndex = 2;
            this.tabPage_xmlDisplay.Text = "XML";
            this.tabPage_xmlDisplay.UseVisualStyleBackColor = true;
            // 
            // webBrowser_xml
            // 
            this.webBrowser_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_xml.Location = new System.Drawing.Point(4, 4);
            this.webBrowser_xml.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_xml.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_xml.Name = "webBrowser_xml";
            this.webBrowser_xml.Size = new System.Drawing.Size(531, 173);
            this.webBrowser_xml.TabIndex = 0;
            // 
            // textBox_tempRecPath
            // 
            this.textBox_tempRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tempRecPath.Location = new System.Drawing.Point(302, -1);
            this.textBox_tempRecPath.Name = "textBox_tempRecPath";
            this.textBox_tempRecPath.Size = new System.Drawing.Size(97, 25);
            this.textBox_tempRecPath.TabIndex = 9;
            this.textBox_tempRecPath.Visible = false;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(2, 37);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_main);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tabControl_objectAndOther);
            this.splitContainer_main.Size = new System.Drawing.Size(547, 325);
            this.splitContainer_main.SplitterDistance = 212;
            this.splitContainer_main.TabIndex = 10;
            // 
            // tabControl_objectAndOther
            // 
            this.tabControl_objectAndOther.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_objectAndOther.Controls.Add(this.tabPage_object);
            this.tabControl_objectAndOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_objectAndOther.Location = new System.Drawing.Point(0, 0);
            this.tabControl_objectAndOther.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_objectAndOther.Name = "tabControl_objectAndOther";
            this.tabControl_objectAndOther.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_objectAndOther.SelectedIndex = 0;
            this.tabControl_objectAndOther.Size = new System.Drawing.Size(547, 109);
            this.tabControl_objectAndOther.TabIndex = 0;
            // 
            // tabPage_object
            // 
            this.tabPage_object.Controls.Add(this.binaryResControl1);
            this.tabPage_object.Location = new System.Drawing.Point(4, 27);
            this.tabPage_object.Name = "tabPage_object";
            this.tabPage_object.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_object.Size = new System.Drawing.Size(539, 78);
            this.tabPage_object.TabIndex = 0;
            this.tabPage_object.Text = "对象";
            this.tabPage_object.UseVisualStyleBackColor = true;
            // 
            // binaryResControl1
            // 
            this.binaryResControl1.BiblioRecPath = "";
            this.binaryResControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.binaryResControl1.Changed = false;
            this.binaryResControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryResControl1.Location = new System.Drawing.Point(3, 3);
            this.binaryResControl1.Name = "binaryResControl1";
            this.binaryResControl1.Size = new System.Drawing.Size(533, 72);
            this.binaryResControl1.TabIndex = 0;
            // 
            // DcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 374);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.textBox_tempRecPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_savePath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DcForm";
            this.ShowInTaskbar = false;
            this.Text = "DC记录窗";
            this.Load += new System.EventHandler(this.DcForm_Load);
            this.Activated += new System.EventHandler(this.DcForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DcForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DcForm_FormClosing);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_dcEditor.ResumeLayout(false);
            this.tabPage_dcEditor.PerformLayout();
            this.toolStrip_xmlEditor.ResumeLayout(false);
            this.toolStrip_xmlEditor.PerformLayout();
            this.tabPage_xmlDisplay.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_objectAndOther.ResumeLayout(false);
            this.tabPage_object.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public DigitalPlatform.CommonControl.DcEditor DcEditor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_savePath;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_dcEditor;
        private System.Windows.Forms.ToolStrip toolStrip_xmlEditor;
        private System.Windows.Forms.ToolStripButton toolStripButton_dispXmlText;
        private System.Windows.Forms.TabPage tabPage_xmlDisplay;
        private System.Windows.Forms.WebBrowser webBrowser_xml;
        private System.Windows.Forms.TextBox textBox_tempRecPath;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TabControl tabControl_objectAndOther;
        private System.Windows.Forms.TabPage tabPage_object;
        private DigitalPlatform.CirculationClient.BinaryResControl binaryResControl1;
        private System.Windows.Forms.ToolStripButton toolStripButton_newElement;
        private System.Windows.Forms.ToolStripButton toolStripButton_deleteEelement;
    }
}