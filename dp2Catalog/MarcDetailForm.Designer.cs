namespace dp2Catalog
{
    partial class MarcDetailForm
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

            if (m_detailHostObj != null)
                m_detailHostObj.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MarcDetailForm));
            this.textBox_tempRecPath = new System.Windows.Forms.TextBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_html = new System.Windows.Forms.TabPage();
            this.webBrowser_html = new System.Windows.Forms.WebBrowser();
            this.tabPage_marcEditor = new System.Windows.Forms.TabPage();
            this.MarcEditor = new DigitalPlatform.Marc.MarcEditor();
            this.tabPage_originData = new System.Windows.Forms.TabPage();
            this.splitContainer_originDataMain = new System.Windows.Forms.SplitContainer();
            this.panel_originDataText = new System.Windows.Forms.Panel();
            this.textBox_originDatabaseName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_originMarcSyntaxOID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_originDataEncoding = new System.Windows.Forms.ComboBox();
            this.textBox_originData = new System.Windows.Forms.TextBox();
            this.label_originDataWarning = new System.Windows.Forms.Label();
            this.binaryEditor_originData = new DigitalPlatform.CommonControl.BinaryEditor();
            this.textBox_savePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel_up = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl_main.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.tabPage_marcEditor.SuspendLayout();
            this.tabPage_originData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_originDataMain)).BeginInit();
            this.splitContainer_originDataMain.Panel1.SuspendLayout();
            this.splitContainer_originDataMain.Panel2.SuspendLayout();
            this.splitContainer_originDataMain.SuspendLayout();
            this.panel_originDataText.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel_up.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_tempRecPath
            // 
            this.textBox_tempRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tempRecPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_tempRecPath.Location = new System.Drawing.Point(6, 471);
            this.textBox_tempRecPath.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_tempRecPath.Name = "textBox_tempRecPath";
            this.textBox_tempRecPath.ReadOnly = true;
            this.textBox_tempRecPath.Size = new System.Drawing.Size(742, 31);
            this.textBox_tempRecPath.TabIndex = 2;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_main.Controls.Add(this.tabPage_html);
            this.tabControl_main.Controls.Add(this.tabPage_marcEditor);
            this.tabControl_main.Controls.Add(this.tabPage_originData);
            this.tabControl_main.Location = new System.Drawing.Point(0, 57);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(755, 412);
            this.tabControl_main.TabIndex = 3;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser_html);
            this.tabPage_html.Location = new System.Drawing.Point(4, 34);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Size = new System.Drawing.Size(1019, 447);
            this.tabPage_html.TabIndex = 2;
            this.tabPage_html.Text = "HTML";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // webBrowser_html
            // 
            this.webBrowser_html.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_html.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_html.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser_html.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_html.Name = "webBrowser_html";
            this.webBrowser_html.Size = new System.Drawing.Size(1019, 447);
            this.webBrowser_html.TabIndex = 0;
            // 
            // tabPage_marcEditor
            // 
            this.tabPage_marcEditor.Controls.Add(this.MarcEditor);
            this.tabPage_marcEditor.Location = new System.Drawing.Point(4, 34);
            this.tabPage_marcEditor.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_marcEditor.Name = "tabPage_marcEditor";
            this.tabPage_marcEditor.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_marcEditor.Size = new System.Drawing.Size(1019, 447);
            this.tabPage_marcEditor.TabIndex = 0;
            this.tabPage_marcEditor.Text = "MARC";
            this.tabPage_marcEditor.UseVisualStyleBackColor = true;
            // 
            // MarcEditor
            // 
            this.MarcEditor.CaptionFont = new System.Drawing.Font("微软雅黑", 9F);
            this.MarcEditor.ContentBackColor = System.Drawing.SystemColors.Window;
            this.MarcEditor.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.MarcEditor.CurrentImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MarcEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MarcEditor.DocumentOrgX = 0;
            this.MarcEditor.DocumentOrgY = 0;
            this.MarcEditor.FixedSizeFont = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Bold);
            this.MarcEditor.FocusedField = null;
            this.MarcEditor.FocusedFieldIndex = 0;
            this.MarcEditor.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MarcEditor.HorzGridColor = System.Drawing.Color.LightGray;
            this.MarcEditor.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.MarcEditor.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.MarcEditor.IndicatorTextColor = System.Drawing.Color.Green;
            this.MarcEditor.Lang = "zh";
            this.MarcEditor.Location = new System.Drawing.Point(4, 4);
            this.MarcEditor.Marc = "????????????????????????";
            this.MarcEditor.MarcDefDom = null;
            this.MarcEditor.Margin = new System.Windows.Forms.Padding(0);
            this.MarcEditor.Name = "MarcEditor";
            this.MarcEditor.NameBackColor = System.Drawing.SystemColors.Window;
            this.MarcEditor.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.MarcEditor.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.MarcEditor.NameTextColor = System.Drawing.Color.Blue;
            this.MarcEditor.ReadOnly = false;
            this.MarcEditor.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MarcEditor.SelectionStart = -1;
            this.MarcEditor.Size = new System.Drawing.Size(1011, 439);
            this.MarcEditor.TabIndex = 0;
            this.MarcEditor.Text = "marcEditor1";
            this.MarcEditor.UiState = "{\"FieldNameCaptionWidth\":100}";
            this.MarcEditor.VertGridColor = System.Drawing.Color.LightGray;
            this.MarcEditor.GetTemplateDef += new DigitalPlatform.Marc.GetTemplateDefEventHandler(this.MarcEditor_GetTemplateDef);
            this.MarcEditor.GetConfigFile += new DigitalPlatform.Marc.GetConfigFileEventHandle(this.MarcEditor_GetConfigFile);
            this.MarcEditor.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.MarcEditor_GetConfigDom);
            this.MarcEditor.GenerateData += new DigitalPlatform.GenerateDataEventHandler(this.MarcEditor_GenerateData);
            this.MarcEditor.VerifyData += new DigitalPlatform.GenerateDataEventHandler(this.MarcEditor_VerifyData);
            this.MarcEditor.ParseMacro += new DigitalPlatform.Marc.ParseMacroEventHandler(this.MarcEditor_ParseMacro);
            this.MarcEditor.ControlLetterKeyPress += new DigitalPlatform.ControlLetterKeyPressEventHandler(this.MarcEditor_ControlLetterKeyPress);
            this.MarcEditor.TextChanged += new System.EventHandler(this.MarcEditor_TextChanged);
            this.MarcEditor.Enter += new System.EventHandler(this.MarcEditor_Enter);
            // 
            // tabPage_originData
            // 
            this.tabPage_originData.Controls.Add(this.splitContainer_originDataMain);
            this.tabPage_originData.Location = new System.Drawing.Point(4, 34);
            this.tabPage_originData.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_originData.Name = "tabPage_originData";
            this.tabPage_originData.Size = new System.Drawing.Size(747, 374);
            this.tabPage_originData.TabIndex = 1;
            this.tabPage_originData.Text = "原始数据";
            this.tabPage_originData.UseVisualStyleBackColor = true;
            // 
            // splitContainer_originDataMain
            // 
            this.splitContainer_originDataMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_originDataMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_originDataMain.Margin = new System.Windows.Forms.Padding(4);
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
            this.splitContainer_originDataMain.Size = new System.Drawing.Size(747, 374);
            this.splitContainer_originDataMain.SplitterDistance = 263;
            this.splitContainer_originDataMain.SplitterWidth = 5;
            this.splitContainer_originDataMain.TabIndex = 0;
            // 
            // panel_originDataText
            // 
            this.panel_originDataText.Controls.Add(this.tableLayoutPanel_up);
            this.panel_originDataText.Controls.Add(this.label_originDataWarning);
            this.panel_originDataText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_originDataText.Location = new System.Drawing.Point(0, 0);
            this.panel_originDataText.Margin = new System.Windows.Forms.Padding(0);
            this.panel_originDataText.Name = "panel_originDataText";
            this.panel_originDataText.Size = new System.Drawing.Size(747, 263);
            this.panel_originDataText.TabIndex = 1;
            // 
            // textBox_originDatabaseName
            // 
            this.textBox_originDatabaseName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_originDatabaseName.Location = new System.Drawing.Point(150, 4);
            this.textBox_originDatabaseName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_originDatabaseName.Name = "textBox_originDatabaseName";
            this.textBox_originDatabaseName.ReadOnly = true;
            this.textBox_originDatabaseName.Size = new System.Drawing.Size(215, 31);
            this.textBox_originDatabaseName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "数据库名(&D):";
            // 
            // textBox_originMarcSyntaxOID
            // 
            this.textBox_originMarcSyntaxOID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_originMarcSyntaxOID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_originMarcSyntaxOID.Location = new System.Drawing.Point(4, 43);
            this.textBox_originMarcSyntaxOID.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_originMarcSyntaxOID.Name = "textBox_originMarcSyntaxOID";
            this.textBox_originMarcSyntaxOID.ReadOnly = true;
            this.textBox_originMarcSyntaxOID.Size = new System.Drawing.Size(438, 31);
            this.textBox_originMarcSyntaxOID.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(373, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 21);
            this.label3.TabIndex = 5;
            this.label3.Text = "MARC格式OID(&S):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(450, 39);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "编码方式(&E):";
            // 
            // comboBox_originDataEncoding
            // 
            this.comboBox_originDataEncoding.DropDownHeight = 300;
            this.comboBox_originDataEncoding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_originDataEncoding.FormattingEnabled = true;
            this.comboBox_originDataEncoding.IntegralHeight = false;
            this.comboBox_originDataEncoding.Location = new System.Drawing.Point(4, 82);
            this.comboBox_originDataEncoding.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_originDataEncoding.Name = "comboBox_originDataEncoding";
            this.comboBox_originDataEncoding.Size = new System.Drawing.Size(213, 29);
            this.comboBox_originDataEncoding.TabIndex = 1;
            this.comboBox_originDataEncoding.SelectedIndexChanged += new System.EventHandler(this.comboBox_originDataEncoding_SelectedIndexChanged);
            this.comboBox_originDataEncoding.TextChanged += new System.EventHandler(this.comboBox_originDataEncoding_TextChanged);
            // 
            // textBox_originData
            // 
            this.textBox_originData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_originData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_originData.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_originData.HideSelection = false;
            this.textBox_originData.Location = new System.Drawing.Point(0, 121);
            this.textBox_originData.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_originData.Multiline = true;
            this.textBox_originData.Name = "textBox_originData";
            this.textBox_originData.ReadOnly = true;
            this.textBox_originData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_originData.Size = new System.Drawing.Size(747, 142);
            this.textBox_originData.TabIndex = 0;
            // 
            // label_originDataWarning
            // 
            this.label_originDataWarning.AutoSize = true;
            this.label_originDataWarning.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_originDataWarning.ForeColor = System.Drawing.Color.Red;
            this.label_originDataWarning.Location = new System.Drawing.Point(368, 54);
            this.label_originDataWarning.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_originDataWarning.Name = "label_originDataWarning";
            this.label_originDataWarning.Size = new System.Drawing.Size(0, 21);
            this.label_originDataWarning.TabIndex = 7;
            // 
            // binaryEditor_originData
            // 
            this.binaryEditor_originData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.binaryEditor_originData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryEditor_originData.DocumentOrgX = ((long)(0));
            this.binaryEditor_originData.DocumentOrgY = ((long)(0));
            this.binaryEditor_originData.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.binaryEditor_originData.Location = new System.Drawing.Point(0, 0);
            this.binaryEditor_originData.Margin = new System.Windows.Forms.Padding(4);
            this.binaryEditor_originData.Name = "binaryEditor_originData";
            this.binaryEditor_originData.Size = new System.Drawing.Size(747, 106);
            this.binaryEditor_originData.TabIndex = 0;
            this.binaryEditor_originData.Text = "binaryEditor1";
            // 
            // textBox_savePath
            // 
            this.textBox_savePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_savePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_savePath.Location = new System.Drawing.Point(147, 18);
            this.textBox_savePath.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_savePath.Name = "textBox_savePath";
            this.textBox_savePath.Size = new System.Drawing.Size(608, 31);
            this.textBox_savePath.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-4, 20);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 21);
            this.label4.TabIndex = 0;
            this.label4.Text = "记录路径(&P):";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.textBox_originDatabaseName);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.textBox_originMarcSyntaxOID);
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.comboBox_originDataEncoding);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(741, 115);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel_up
            // 
            this.tableLayoutPanel_up.ColumnCount = 1;
            this.tableLayoutPanel_up.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_up.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel_up.Controls.Add(this.textBox_originData, 0, 1);
            this.tableLayoutPanel_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_up.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_up.Name = "tableLayoutPanel_up";
            this.tableLayoutPanel_up.RowCount = 2;
            this.tableLayoutPanel_up.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_up.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_up.Size = new System.Drawing.Size(747, 263);
            this.tableLayoutPanel_up.TabIndex = 8;
            // 
            // MarcDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(755, 513);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_savePath);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.textBox_tempRecPath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MarcDetailForm";
            this.Text = "MARC记录窗";
            this.Activated += new System.EventHandler(this.MarcDetailForm_Activated);
            this.Deactivate += new System.EventHandler(this.MarcDetailForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MarcDetailForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MarcDetailForm_FormClosed);
            this.Load += new System.EventHandler(this.MarcDetailForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_html.ResumeLayout(false);
            this.tabPage_marcEditor.ResumeLayout(false);
            this.tabPage_originData.ResumeLayout(false);
            this.splitContainer_originDataMain.Panel1.ResumeLayout(false);
            this.splitContainer_originDataMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_originDataMain)).EndInit();
            this.splitContainer_originDataMain.ResumeLayout(false);
            this.panel_originDataText.ResumeLayout(false);
            this.panel_originDataText.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel_up.ResumeLayout(false);
            this.tableLayoutPanel_up.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_tempRecPath;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_marcEditor;
        private System.Windows.Forms.TabPage tabPage_originData;
        private System.Windows.Forms.SplitContainer splitContainer_originDataMain;
        private System.Windows.Forms.TextBox textBox_originData;
        private DigitalPlatform.CommonControl.BinaryEditor binaryEditor_originData;
        private System.Windows.Forms.Panel panel_originDataText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_originDataEncoding;
        private System.Windows.Forms.TextBox textBox_originDatabaseName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_originMarcSyntaxOID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_savePath;
        private System.Windows.Forms.Label label4;
        public DigitalPlatform.Marc.MarcEditor MarcEditor;
        private System.Windows.Forms.Label label_originDataWarning;
        private System.Windows.Forms.TabPage tabPage_html;
        private System.Windows.Forms.WebBrowser webBrowser_html;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_up;
    }
}