namespace dp2Circulation
{
    partial class EntityForm
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

            if (this._genData != null)
                this._genData.Dispose();

            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityForm));
            this.contextMenuStrip_selectRegisterType = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_SearchOnly = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_quickRegister = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_register = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_option = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted = new System.Windows.Forms.ToolStripMenuItem();
            this.button_save = new System.Windows.Forms.Button();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel_query = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_hideSearchPanel = new System.Windows.Forms.ToolStripButton();
            this.checkedComboBox_biblioDbNames = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.comboBox_matchStyle = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.button_search = new System.Windows.Forms.Button();
            this.checkBox_autoDetectQueryBarcode = new System.Windows.Forms.CheckBox();
            this.checkBox_autoSavePrev = new System.Windows.Forms.CheckBox();
            this.splitContainer_recordAndItems = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_record = new System.Windows.Forms.TableLayoutPanel();
            this.panel_biblioInfo = new System.Windows.Forms.Panel();
            this.tabControl_biblioInfo = new DigitalPlatform.FixedTabControl();
            this.tabPage_html = new System.Windows.Forms.TabPage();
            this.webBrowser_biblioRecord = new System.Windows.Forms.WebBrowser();
            this.tabPage_marc = new System.Windows.Forms.TabPage();
            this.m_marcEditor = new DigitalPlatform.Marc.MarcEditor();
            this.tabPage_template = new System.Windows.Forms.TabPage();
            this.easyMarcControl1 = new DigitalPlatform.EasyMarc.EasyMarcControl();
            this.toolStrip_marcEditor = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.textBox_biblioRecPath = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_saveAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_marcEditor_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_marcEditor_loadTemplate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_marcEditor_delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clear = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_marcEditor_someFunc = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripButton_marcEditor_saveTemplate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_marcEditor_setActiveCatalogingRule = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_viewXml = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_viewOriginXml = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_exportAllInfoToXmlFile = new System.Windows.Forms.ToolStripMenuItem();
            this.StripMenuItem_importFromXmlFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_loadTargetBiblioRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_viewMarcJidaoData = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_enableSaveAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_getKeys = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_getSummary = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_editMacroTable = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_fixed = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_marcEditor_loadRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_marcEditor_saveTo = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_setTargetRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_marcEditor_moveTo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButton_searchDup = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolStripMenuItem_searchDupInExistWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_searchDupInNewWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_checkUnique = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_verifyData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_next = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prev = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_option = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSplitButton_insertCoverImage = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolStripMenuItem_insertCoverImageFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_insertCoverImageFromCamera = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl_itemAndIssue = new System.Windows.Forms.TabControl();
            this.tabPage_item = new System.Windows.Forms.TabPage();
            this.entityControl1 = new dp2Circulation.EntityControl();
            this.tabPage_issue = new System.Windows.Forms.TabPage();
            this.issueControl1 = new dp2Circulation.IssueControl();
            this.tabPage_order = new System.Windows.Forms.TabPage();
            this.orderControl1 = new dp2Circulation.OrderControl();
            this.tabPage_object = new System.Windows.Forms.TabPage();
            this.binaryResControl1 = new DigitalPlatform.CirculationClient.BinaryResControl();
            this.tabPage_comment = new System.Windows.Forms.TabPage();
            this.commentControl1 = new dp2Circulation.CommentControl();
            this.panel_itemQuickInput = new System.Windows.Forms.Panel();
            this.button_register = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_itemBarcode = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_hideItemQuickInput = new System.Windows.Forms.ToolStripButton();
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ToolStripMenuItem_removeCoverImage = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_selectRegisterType.SuspendLayout();
            this.contextMenuStrip_option.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.flowLayoutPanel_query.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_recordAndItems)).BeginInit();
            this.splitContainer_recordAndItems.Panel1.SuspendLayout();
            this.splitContainer_recordAndItems.Panel2.SuspendLayout();
            this.splitContainer_recordAndItems.SuspendLayout();
            this.tableLayoutPanel_record.SuspendLayout();
            this.panel_biblioInfo.SuspendLayout();
            this.tabControl_biblioInfo.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.tabPage_marc.SuspendLayout();
            this.tabPage_template.SuspendLayout();
            this.toolStrip_marcEditor.SuspendLayout();
            this.tabControl_itemAndIssue.SuspendLayout();
            this.tabPage_item.SuspendLayout();
            this.tabPage_issue.SuspendLayout();
            this.tabPage_order.SuspendLayout();
            this.tabPage_object.SuspendLayout();
            this.tabPage_comment.SuspendLayout();
            this.panel_itemQuickInput.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip_selectRegisterType
            // 
            this.contextMenuStrip_selectRegisterType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_SearchOnly,
            this.toolStripMenuItem_quickRegister,
            this.toolStripMenuItem_register});
            this.contextMenuStrip_selectRegisterType.Name = "contextMenuStrip_selectRegisterType";
            this.contextMenuStrip_selectRegisterType.Size = new System.Drawing.Size(125, 70);
            // 
            // toolStripMenuItem_SearchOnly
            // 
            this.toolStripMenuItem_SearchOnly.Name = "toolStripMenuItem_SearchOnly";
            this.toolStripMenuItem_SearchOnly.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_SearchOnly.Text = "只检索";
            this.toolStripMenuItem_SearchOnly.Click += new System.EventHandler(this.toolStripMenuItem_SearchOnly_Click);
            // 
            // toolStripMenuItem_quickRegister
            // 
            this.toolStripMenuItem_quickRegister.Name = "toolStripMenuItem_quickRegister";
            this.toolStripMenuItem_quickRegister.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_quickRegister.Text = "快速登记";
            this.toolStripMenuItem_quickRegister.Click += new System.EventHandler(this.toolStripMenuItem_quickRegister_Click);
            // 
            // toolStripMenuItem_register
            // 
            this.toolStripMenuItem_register.Name = "toolStripMenuItem_register";
            this.toolStripMenuItem_register.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_register.Text = "登记";
            this.toolStripMenuItem_register.Click += new System.EventHandler(this.toolStripMenuItem_register_Click);
            // 
            // contextMenuStrip_option
            // 
            this.contextMenuStrip_option.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted});
            this.contextMenuStrip_option.Name = "contextMenuStrip_option";
            this.contextMenuStrip_option.Size = new System.Drawing.Size(269, 26);
            // 
            // ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted
            // 
            this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted.Name = "ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted";
            this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted.Size = new System.Drawing.Size(268, 22);
            this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted.Text = "使能记录删除后的“全部保存”按钮";
            this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted.Click += new System.EventHandler(this.ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted_Click);
            // 
            // button_save
            // 
            this.button_save.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button_save.AutoSize = true;
            this.button_save.Image = ((System.Drawing.Image)(resources.GetObject("button_save.Image")));
            this.button_save.Location = new System.Drawing.Point(603, 2);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(123, 28);
            this.button_save.TabIndex = 4;
            this.button_save.Text = "全部保存(&S)";
            this.button_save.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.flowLayoutPanel_query, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_recordAndItems, 0, 1);
            this.tableLayoutPanel_main.Controls.Add(this.panel_itemQuickInput, 0, 2);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.tableLayoutPanel_main.RowCount = 3;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(732, 393);
            this.tableLayoutPanel_main.TabIndex = 11;
            // 
            // flowLayoutPanel_query
            // 
            this.flowLayoutPanel_query.AutoSize = true;
            this.flowLayoutPanel_query.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel_query.Controls.Add(this.toolStrip2);
            this.flowLayoutPanel_query.Controls.Add(this.checkedComboBox_biblioDbNames);
            this.flowLayoutPanel_query.Controls.Add(this.comboBox_from);
            this.flowLayoutPanel_query.Controls.Add(this.comboBox_matchStyle);
            this.flowLayoutPanel_query.Controls.Add(this.label1);
            this.flowLayoutPanel_query.Controls.Add(this.textBox_queryWord);
            this.flowLayoutPanel_query.Controls.Add(this.button_search);
            this.flowLayoutPanel_query.Controls.Add(this.checkBox_autoDetectQueryBarcode);
            this.flowLayoutPanel_query.Controls.Add(this.checkBox_autoSavePrev);
            this.flowLayoutPanel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_query.Location = new System.Drawing.Point(0, 10);
            this.flowLayoutPanel_query.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_query.Name = "flowLayoutPanel_query";
            this.flowLayoutPanel_query.Size = new System.Drawing.Size(732, 59);
            this.flowLayoutPanel_query.TabIndex = 4;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_hideSearchPanel});
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.Size = new System.Drawing.Size(29, 31);
            this.toolStrip2.TabIndex = 9;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButton_hideSearchPanel
            // 
            this.toolStripButton_hideSearchPanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_hideSearchPanel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_hideSearchPanel.Image")));
            this.toolStripButton_hideSearchPanel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_hideSearchPanel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_hideSearchPanel.Name = "toolStripButton_hideSearchPanel";
            this.toolStripButton_hideSearchPanel.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton_hideSearchPanel.Text = "隐藏检索面板";
            this.toolStripButton_hideSearchPanel.Click += new System.EventHandler(this.toolStripButton_hideSearchPanel_Click);
            // 
            // checkedComboBox_biblioDbNames
            // 
            this.checkedComboBox_biblioDbNames.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkedComboBox_biblioDbNames.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_biblioDbNames.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_biblioDbNames.Location = new System.Drawing.Point(29, 4);
            this.checkedComboBox_biblioDbNames.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_biblioDbNames.Name = "checkedComboBox_biblioDbNames";
            this.checkedComboBox_biblioDbNames.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_biblioDbNames.Size = new System.Drawing.Size(135, 22);
            this.checkedComboBox_biblioDbNames.TabIndex = 12;
            this.checkedComboBox_biblioDbNames.DropDown += new System.EventHandler(this.checkedComboBox_dbName_DropDown);
            this.checkedComboBox_biblioDbNames.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.checkedComboBox_dbName_ItemChecked);
            // 
            // comboBox_from
            // 
            this.comboBox_from.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBox_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.Location = new System.Drawing.Point(167, 5);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(121, 20);
            this.comboBox_from.TabIndex = 1;
            // 
            // comboBox_matchStyle
            // 
            this.comboBox_matchStyle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBox_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_matchStyle.FormattingEnabled = true;
            this.comboBox_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致",
            "空值"});
            this.comboBox_matchStyle.Location = new System.Drawing.Point(294, 5);
            this.comboBox_matchStyle.Name = "comboBox_matchStyle";
            this.comboBox_matchStyle.Size = new System.Drawing.Size(121, 20);
            this.comboBox_matchStyle.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(421, 6);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.label1.Size = new System.Drawing.Size(11, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "-";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_queryWord.Location = new System.Drawing.Point(438, 5);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(165, 21);
            this.textBox_queryWord.TabIndex = 3;
            this.textBox_queryWord.Enter += new System.EventHandler(this.textBox_queryWord_Enter);
            this.textBox_queryWord.Leave += new System.EventHandler(this.textBox_queryWord_Leave);
            // 
            // button_search
            // 
            this.button_search.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_search.Image = ((System.Drawing.Image)(resources.GetObject("button_search.Image")));
            this.button_search.Location = new System.Drawing.Point(609, 4);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(75, 23);
            this.button_search.TabIndex = 4;
            this.button_search.Text = "检索(&S)";
            this.button_search.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // checkBox_autoDetectQueryBarcode
            // 
            this.checkBox_autoDetectQueryBarcode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBox_autoDetectQueryBarcode.AutoSize = true;
            this.checkBox_autoDetectQueryBarcode.Checked = true;
            this.checkBox_autoDetectQueryBarcode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoDetectQueryBarcode.Location = new System.Drawing.Point(3, 34);
            this.checkBox_autoDetectQueryBarcode.Name = "checkBox_autoDetectQueryBarcode";
            this.checkBox_autoDetectQueryBarcode.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.checkBox_autoDetectQueryBarcode.Size = new System.Drawing.Size(124, 22);
            this.checkBox_autoDetectQueryBarcode.TabIndex = 5;
            this.checkBox_autoDetectQueryBarcode.Text = "适应ISBN条码(&A)";
            this.checkBox_autoDetectQueryBarcode.UseVisualStyleBackColor = true;
            this.checkBox_autoDetectQueryBarcode.Visible = false;
            // 
            // checkBox_autoSavePrev
            // 
            this.checkBox_autoSavePrev.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBox_autoSavePrev.AutoSize = true;
            this.checkBox_autoSavePrev.Location = new System.Drawing.Point(132, 37);
            this.checkBox_autoSavePrev.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_autoSavePrev.Name = "checkBox_autoSavePrev";
            this.checkBox_autoSavePrev.Size = new System.Drawing.Size(150, 16);
            this.checkBox_autoSavePrev.TabIndex = 6;
            this.checkBox_autoSavePrev.Text = "自动保存先前的修改(&S)";
            this.checkBox_autoSavePrev.UseVisualStyleBackColor = true;
            // 
            // splitContainer_recordAndItems
            // 
            this.splitContainer_recordAndItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_recordAndItems.Location = new System.Drawing.Point(0, 69);
            this.splitContainer_recordAndItems.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_recordAndItems.Name = "splitContainer_recordAndItems";
            this.splitContainer_recordAndItems.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_recordAndItems.Panel1
            // 
            this.splitContainer_recordAndItems.Panel1.Controls.Add(this.tableLayoutPanel_record);
            // 
            // splitContainer_recordAndItems.Panel2
            // 
            this.splitContainer_recordAndItems.Panel2.Controls.Add(this.tabControl_itemAndIssue);
            this.splitContainer_recordAndItems.Size = new System.Drawing.Size(732, 279);
            this.splitContainer_recordAndItems.SplitterDistance = 140;
            this.splitContainer_recordAndItems.SplitterWidth = 6;
            this.splitContainer_recordAndItems.TabIndex = 5;
            // 
            // tableLayoutPanel_record
            // 
            this.tableLayoutPanel_record.AutoSize = true;
            this.tableLayoutPanel_record.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_record.ColumnCount = 1;
            this.tableLayoutPanel_record.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_record.Controls.Add(this.panel_biblioInfo, 0, 1);
            this.tableLayoutPanel_record.Controls.Add(this.toolStrip_marcEditor, 0, 0);
            this.tableLayoutPanel_record.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_record.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_record.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_record.Name = "tableLayoutPanel_record";
            this.tableLayoutPanel_record.RowCount = 2;
            this.tableLayoutPanel_record.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_record.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_record.Size = new System.Drawing.Size(732, 140);
            this.tableLayoutPanel_record.TabIndex = 6;
            // 
            // panel_biblioInfo
            // 
            this.panel_biblioInfo.AutoSize = true;
            this.panel_biblioInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel_biblioInfo.Controls.Add(this.tabControl_biblioInfo);
            this.panel_biblioInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_biblioInfo.Location = new System.Drawing.Point(3, 32);
            this.panel_biblioInfo.Name = "panel_biblioInfo";
            this.panel_biblioInfo.Size = new System.Drawing.Size(726, 105);
            this.panel_biblioInfo.TabIndex = 12;
            // 
            // tabControl_biblioInfo
            // 
            this.tabControl_biblioInfo.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl_biblioInfo.Controls.Add(this.tabPage_html);
            this.tabControl_biblioInfo.Controls.Add(this.tabPage_marc);
            this.tabControl_biblioInfo.Controls.Add(this.tabPage_template);
            this.tabControl_biblioInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_biblioInfo.Location = new System.Drawing.Point(0, 0);
            this.tabControl_biblioInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_biblioInfo.Multiline = true;
            this.tabControl_biblioInfo.Name = "tabControl_biblioInfo";
            this.tabControl_biblioInfo.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_biblioInfo.SelectedIndex = 0;
            this.tabControl_biblioInfo.Size = new System.Drawing.Size(726, 105);
            this.tabControl_biblioInfo.TabIndex = 3;
            this.tabControl_biblioInfo.SelectedIndexChanged += new System.EventHandler(this.tabControl_biblioInfo_SelectedIndexChanged);
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser_biblioRecord);
            this.tabPage_html.Location = new System.Drawing.Point(40, 4);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Size = new System.Drawing.Size(682, 97);
            this.tabPage_html.TabIndex = 0;
            this.tabPage_html.Text = "OPAC";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // webBrowser_biblioRecord
            // 
            this.webBrowser_biblioRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblioRecord.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_biblioRecord.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_biblioRecord.Name = "webBrowser_biblioRecord";
            this.webBrowser_biblioRecord.Size = new System.Drawing.Size(682, 97);
            this.webBrowser_biblioRecord.TabIndex = 2;
            // 
            // tabPage_marc
            // 
            this.tabPage_marc.Controls.Add(this.m_marcEditor);
            this.tabPage_marc.Location = new System.Drawing.Point(40, 4);
            this.tabPage_marc.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_marc.Name = "tabPage_marc";
            this.tabPage_marc.Size = new System.Drawing.Size(682, 97);
            this.tabPage_marc.TabIndex = 1;
            this.tabPage_marc.Text = "MARC";
            this.tabPage_marc.UseVisualStyleBackColor = true;
            this.tabPage_marc.Enter += new System.EventHandler(this.tabPage_marc_Enter);
            // 
            // m_marcEditor
            // 
            this.m_marcEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_marcEditor.CaptionFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_marcEditor.Changed = true;
            this.m_marcEditor.ContentBackColor = System.Drawing.SystemColors.Window;
            this.m_marcEditor.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.m_marcEditor.CurrentImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_marcEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_marcEditor.DocumentOrgX = 0;
            this.m_marcEditor.DocumentOrgY = 0;
            this.m_marcEditor.FieldNameCaptionWidth = 100;
            this.m_marcEditor.FixedSizeFont = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Bold);
            this.m_marcEditor.FocusedField = null;
            this.m_marcEditor.FocusedFieldIndex = -1;
            this.m_marcEditor.HorzGridColor = System.Drawing.Color.LightGray;
            this.m_marcEditor.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.m_marcEditor.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.m_marcEditor.IndicatorTextColor = System.Drawing.Color.Green;
            this.m_marcEditor.Location = new System.Drawing.Point(0, 0);
            this.m_marcEditor.Marc = "????????????????????????";
            this.m_marcEditor.MarcDefDom = null;
            this.m_marcEditor.Margin = new System.Windows.Forms.Padding(0);
            this.m_marcEditor.Name = "m_marcEditor";
            this.m_marcEditor.NameBackColor = System.Drawing.SystemColors.Window;
            this.m_marcEditor.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.m_marcEditor.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.m_marcEditor.NameTextColor = System.Drawing.Color.Blue;
            this.m_marcEditor.ReadOnly = false;
            this.m_marcEditor.SelectionStart = -1;
            this.m_marcEditor.Size = new System.Drawing.Size(682, 97);
            this.m_marcEditor.TabIndex = 0;
            this.m_marcEditor.Text = "marcEditor1";
            this.m_marcEditor.VertGridColor = System.Drawing.Color.LightGray;
            this.m_marcEditor.GetTemplateDef += new DigitalPlatform.Marc.GetTemplateDefEventHandler(this.MarcEditor_GetTemplateDef);
            this.m_marcEditor.SelectedFieldChanged += new System.EventHandler(this.MarcEditor_SelectedFieldChanged);
            this.m_marcEditor.GetConfigFile += new DigitalPlatform.Marc.GetConfigFileEventHandle(this.MarcEditor_GetConfigFile);
            this.m_marcEditor.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.MarcEditor_GetConfigDom);
            this.m_marcEditor.GenerateData += new DigitalPlatform.GenerateDataEventHandler(this.MarcEditor_GenerateData);
            this.m_marcEditor.VerifyData += new DigitalPlatform.GenerateDataEventHandler(this.MarcEditor_VerifyData);
            this.m_marcEditor.ParseMacro += new DigitalPlatform.Marc.ParseMacroEventHandler(this.MarcEditor_ParseMacro);
            this.m_marcEditor.ControlLetterKeyPress += new DigitalPlatform.ControlLetterKeyPressEventHandler(this.MarcEditor_ControlLetterKeyPress);
            this.m_marcEditor.TextChanged += new System.EventHandler(this.MarcEditor_TextChanged);
            this.m_marcEditor.Enter += new System.EventHandler(this.MarcEditor_Enter);
            this.m_marcEditor.Leave += new System.EventHandler(this.MarcEditor_Leave);
            // 
            // tabPage_template
            // 
            this.tabPage_template.Controls.Add(this.easyMarcControl1);
            this.tabPage_template.Location = new System.Drawing.Point(40, 4);
            this.tabPage_template.Name = "tabPage_template";
            this.tabPage_template.Size = new System.Drawing.Size(682, 97);
            this.tabPage_template.TabIndex = 2;
            this.tabPage_template.Text = "模板";
            this.tabPage_template.UseVisualStyleBackColor = true;
            // 
            // easyMarcControl1
            // 
            this.easyMarcControl1.AutoScroll = true;
            this.easyMarcControl1.CaptionWidth = 116;
            this.easyMarcControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyMarcControl1.HideIndicator = true;
            this.easyMarcControl1.IncludeNumber = false;
            this.easyMarcControl1.Location = new System.Drawing.Point(0, 0);
            this.easyMarcControl1.MarcDefDom = null;
            this.easyMarcControl1.Name = "easyMarcControl1";
            this.easyMarcControl1.Size = new System.Drawing.Size(682, 97);
            this.easyMarcControl1.TabIndex = 0;
            this.easyMarcControl1.TextChanged += new System.EventHandler(this.easyMarcControl_TextChanged);
            this.easyMarcControl1.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.MarcEditor_GetConfigDom);
            // 
            // toolStrip_marcEditor
            // 
            this.toolStrip_marcEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_marcEditor.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_marcEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.textBox_biblioRecPath,
            this.toolStripSeparator1,
            this.toolStripButton_saveAll,
            this.toolStripButton_marcEditor_save,
            this.toolStripButton_marcEditor_loadTemplate,
            this.toolStripSeparator9,
            this.toolStripButton_marcEditor_delete,
            this.toolStripButton_clear,
            this.toolStripDropDownButton_marcEditor_someFunc,
            this.toolStripSeparator2,
            this.toolStripButton_marcEditor_saveTo,
            this.toolStripButton_setTargetRecord,
            this.toolStripButton_marcEditor_moveTo,
            this.toolStripSplitButton_searchDup,
            this.toolStripButton_verifyData,
            this.toolStripSeparator3,
            this.toolStripButton_next,
            this.toolStripButton_prev,
            this.toolStripButton_option,
            this.toolStripSeparator8,
            this.toolStripSplitButton_insertCoverImage});
            this.toolStrip_marcEditor.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_marcEditor.Name = "toolStrip_marcEditor";
            this.toolStrip_marcEditor.Size = new System.Drawing.Size(732, 29);
            this.toolStrip_marcEditor.TabIndex = 5;
            this.toolStrip_marcEditor.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(71, 26);
            this.toolStripLabel1.Text = "书目:";
            this.toolStripLabel1.MouseDown += toolStripLabel1_MouseDown;
            // 
            // textBox_biblioRecPath
            // 
            this.textBox_biblioRecPath.Name = "textBox_biblioRecPath";
            this.textBox_biblioRecPath.ReadOnly = true;
            this.textBox_biblioRecPath.Size = new System.Drawing.Size(151, 29);
            this.textBox_biblioRecPath.ToolTipText = "种(书目)记录路径";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 29);
            // 
            // toolStripButton_saveAll
            // 
            this.toolStripButton_saveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_saveAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_saveAll.Image")));
            this.toolStripButton_saveAll.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_saveAll.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_saveAll.Name = "toolStripButton_saveAll";
            this.toolStripButton_saveAll.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_saveAll.Text = "全部保存";
            this.toolStripButton_saveAll.Click += new System.EventHandler(this.toolStripButton_saveAll_Click);
            // 
            // toolStripButton_marcEditor_save
            // 
            this.toolStripButton_marcEditor_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_marcEditor_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_marcEditor_save.Image")));
            this.toolStripButton_marcEditor_save.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_marcEditor_save.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_marcEditor_save.Name = "toolStripButton_marcEditor_save";
            this.toolStripButton_marcEditor_save.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_marcEditor_save.Text = "保存书目记录(不保存册信息)";
            this.toolStripButton_marcEditor_save.Click += new System.EventHandler(this.toolStripButton_marcEditor_save_Click);
            // 
            // toolStripButton_marcEditor_loadTemplate
            // 
            this.toolStripButton_marcEditor_loadTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_marcEditor_loadTemplate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_marcEditor_loadTemplate.Image")));
            this.toolStripButton_marcEditor_loadTemplate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_marcEditor_loadTemplate.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_marcEditor_loadTemplate.Name = "toolStripButton_marcEditor_loadTemplate";
            this.toolStripButton_marcEditor_loadTemplate.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_marcEditor_loadTemplate.Text = "装载书目模板";
            this.toolStripButton_marcEditor_loadTemplate.ToolTipText = "装载书目模板(Ctrl+T)";
            this.toolStripButton_marcEditor_loadTemplate.Click += new System.EventHandler(this.toolStripButton_marcEditor_loadTemplate_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 29);
            // 
            // toolStripButton_marcEditor_delete
            // 
            this.toolStripButton_marcEditor_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_marcEditor_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_marcEditor_delete.Image")));
            this.toolStripButton_marcEditor_delete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_marcEditor_delete.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_marcEditor_delete.Name = "toolStripButton_marcEditor_delete";
            this.toolStripButton_marcEditor_delete.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_marcEditor_delete.Text = "删除书目记录及下属的册记录";
            this.toolStripButton_marcEditor_delete.ToolTipText = "删除书目记录及下属的册、期、订购记录";
            this.toolStripButton_marcEditor_delete.Click += new System.EventHandler(this.toolStripButton_marcEditor_delete_Click);
            // 
            // toolStripButton_clear
            // 
            this.toolStripButton_clear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_clear.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clear.Image")));
            this.toolStripButton_clear.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_clear.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_clear.Name = "toolStripButton_clear";
            this.toolStripButton_clear.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_clear.Text = "清除";
            this.toolStripButton_clear.Click += new System.EventHandler(this.toolStripButton_clear_Click);
            // 
            // toolStripDropDownButton_marcEditor_someFunc
            // 
            this.toolStripDropDownButton_marcEditor_someFunc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_marcEditor_someFunc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_marcEditor_saveTemplate,
            this.toolStripSeparator4,
            this.toolStripMenuItem_marcEditor_setActiveCatalogingRule,
            this.MenuItem_marcEditor_viewXml,
            this.MenuItem_marcEditor_viewOriginXml,
            this.toolStripSeparator6,
            this.ToolStripMenuItem_exportAllInfoToXmlFile,
            this.StripMenuItem_importFromXmlFile,
            this.toolStripSeparator5,
            this.ToolStripMenuItem_loadTargetBiblioRecord,
            this.toolStripSeparator7,
            this.ToolStripMenuItem_viewMarcJidaoData,
            this.ToolStripMenuItem_enableSaveAllButton,
            this.MenuItem_marcEditor_getKeys,
            this.MenuItem_marcEditor_getSummary,
            this.MenuItem_marcEditor_editMacroTable,
            this.MenuItem_marcEditor_fixed,
            this.MenuItem_marcEditor_loadRecord});
            this.toolStripDropDownButton_marcEditor_someFunc.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_marcEditor_someFunc.Image")));
            this.toolStripDropDownButton_marcEditor_someFunc.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_marcEditor_someFunc.Name = "toolStripDropDownButton_marcEditor_someFunc";
            this.toolStripDropDownButton_marcEditor_someFunc.Size = new System.Drawing.Size(30, 26);
            this.toolStripDropDownButton_marcEditor_someFunc.Text = "...";
            this.toolStripDropDownButton_marcEditor_someFunc.ToolTipText = "更多命令...";
            this.toolStripDropDownButton_marcEditor_someFunc.DropDownOpening += new System.EventHandler(this.toolStripDropDownButton_marcEditor_someFunc_DropDownOpening);
            // 
            // toolStripButton_marcEditor_saveTemplate
            // 
            this.toolStripButton_marcEditor_saveTemplate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_marcEditor_saveTemplate.Image")));
            this.toolStripButton_marcEditor_saveTemplate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_marcEditor_saveTemplate.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_marcEditor_saveTemplate.Name = "toolStripButton_marcEditor_saveTemplate";
            this.toolStripButton_marcEditor_saveTemplate.Size = new System.Drawing.Size(142, 26);
            this.toolStripButton_marcEditor_saveTemplate.Text = "保存书目记录到模板";
            this.toolStripButton_marcEditor_saveTemplate.Click += new System.EventHandler(this.toolStripButton_marcEditor_saveTemplate_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(254, 6);
            // 
            // toolStripMenuItem_marcEditor_setActiveCatalogingRule
            // 
            this.toolStripMenuItem_marcEditor_setActiveCatalogingRule.Name = "toolStripMenuItem_marcEditor_setActiveCatalogingRule";
            this.toolStripMenuItem_marcEditor_setActiveCatalogingRule.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItem_marcEditor_setActiveCatalogingRule.Text = "编目规则[查看时用]";
            // 
            // MenuItem_marcEditor_viewXml
            // 
            this.MenuItem_marcEditor_viewXml.Name = "MenuItem_marcEditor_viewXml";
            this.MenuItem_marcEditor_viewXml.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_viewXml.Text = "查看当前书目XML数据(&X)";
            this.MenuItem_marcEditor_viewXml.Click += new System.EventHandler(this.MenuItem_marcEditor_viewXml_Click);
            // 
            // MenuItem_marcEditor_viewOriginXml
            // 
            this.MenuItem_marcEditor_viewOriginXml.Name = "MenuItem_marcEditor_viewOriginXml";
            this.MenuItem_marcEditor_viewOriginXml.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_viewOriginXml.Text = "查看最初调入的书目XML数据(&F)...";
            this.MenuItem_marcEditor_viewOriginXml.Click += new System.EventHandler(this.MenuItem_marcEditor_viewOriginXml_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(254, 6);
            // 
            // ToolStripMenuItem_exportAllInfoToXmlFile
            // 
            this.ToolStripMenuItem_exportAllInfoToXmlFile.Name = "ToolStripMenuItem_exportAllInfoToXmlFile";
            this.ToolStripMenuItem_exportAllInfoToXmlFile.Size = new System.Drawing.Size(257, 22);
            this.ToolStripMenuItem_exportAllInfoToXmlFile.Text = "导出全部信息到XML文件(&E)...";
            this.ToolStripMenuItem_exportAllInfoToXmlFile.Click += new System.EventHandler(this.ToolStripMenuItem_exportAllInfoToXmlFile_Click);
            // 
            // StripMenuItem_importFromXmlFile
            // 
            this.StripMenuItem_importFromXmlFile.Name = "StripMenuItem_importFromXmlFile";
            this.StripMenuItem_importFromXmlFile.Size = new System.Drawing.Size(257, 22);
            this.StripMenuItem_importFromXmlFile.Text = "从XML文件中导入(&I)...";
            this.StripMenuItem_importFromXmlFile.Click += new System.EventHandler(this.StripMenuItem_importFromXmlFile_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(254, 6);
            // 
            // ToolStripMenuItem_loadTargetBiblioRecord
            // 
            this.ToolStripMenuItem_loadTargetBiblioRecord.Name = "ToolStripMenuItem_loadTargetBiblioRecord";
            this.ToolStripMenuItem_loadTargetBiblioRecord.Size = new System.Drawing.Size(257, 22);
            this.ToolStripMenuItem_loadTargetBiblioRecord.Text = "跳转到目标记录(&T)";
            this.ToolStripMenuItem_loadTargetBiblioRecord.Click += new System.EventHandler(this.ToolStripMenuItem_loadTargetBiblioRecord_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(254, 6);
            // 
            // ToolStripMenuItem_viewMarcJidaoData
            // 
            this.ToolStripMenuItem_viewMarcJidaoData.Name = "ToolStripMenuItem_viewMarcJidaoData";
            this.ToolStripMenuItem_viewMarcJidaoData.Size = new System.Drawing.Size(257, 22);
            this.ToolStripMenuItem_viewMarcJidaoData.Text = "观察MARC记到数据(&J)...";
            this.ToolStripMenuItem_viewMarcJidaoData.Click += new System.EventHandler(this.ToolStripMenuItem_viewMarcJidaoData_Click);
            // 
            // ToolStripMenuItem_enableSaveAllButton
            // 
            this.ToolStripMenuItem_enableSaveAllButton.Name = "ToolStripMenuItem_enableSaveAllButton";
            this.ToolStripMenuItem_enableSaveAllButton.Size = new System.Drawing.Size(257, 22);
            this.ToolStripMenuItem_enableSaveAllButton.Text = "[记录删除后]使能编辑保存(&E)";
            this.ToolStripMenuItem_enableSaveAllButton.Click += new System.EventHandler(this.ToolStripMenuItem_enableSaveAllButton_Click);
            // 
            // MenuItem_marcEditor_getKeys
            // 
            this.MenuItem_marcEditor_getKeys.Name = "MenuItem_marcEditor_getKeys";
            this.MenuItem_marcEditor_getKeys.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_getKeys.Text = "查看书目记录的检索点(&K)";
            this.MenuItem_marcEditor_getKeys.Click += new System.EventHandler(this.MenuItem_marcEditor_getKeys_Click);
            // 
            // MenuItem_marcEditor_getSummary
            // 
            this.MenuItem_marcEditor_getSummary.Name = "MenuItem_marcEditor_getSummary";
            this.MenuItem_marcEditor_getSummary.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_getSummary.Text = "查看书目记录摘要(&S)";
            this.MenuItem_marcEditor_getSummary.Click += new System.EventHandler(this.MenuItem_marcEditor_getSummary_Click);
            // 
            // MenuItem_marcEditor_editMacroTable
            // 
            this.MenuItem_marcEditor_editMacroTable.Name = "MenuItem_marcEditor_editMacroTable";
            this.MenuItem_marcEditor_editMacroTable.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_editMacroTable.Text = "宏定义(&M)...";
            this.MenuItem_marcEditor_editMacroTable.Click += new System.EventHandler(this.MenuItem_marcEditor_editMacroTable_Click);
            // 
            // MenuItem_marcEditor_fixed
            // 
            this.MenuItem_marcEditor_fixed.Name = "MenuItem_marcEditor_fixed";
            this.MenuItem_marcEditor_fixed.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_fixed.Text = "固定到左侧(&L)";
            this.MenuItem_marcEditor_fixed.Click += new System.EventHandler(this.MenuItem_marcEditor_toggleFixed_Click);
            // 
            // MenuItem_marcEditor_loadRecord
            // 
            this.MenuItem_marcEditor_loadRecord.Name = "MenuItem_marcEditor_loadRecord";
            this.MenuItem_marcEditor_loadRecord.Size = new System.Drawing.Size(257, 22);
            this.MenuItem_marcEditor_loadRecord.Text = "装载记录(&L)";
            this.MenuItem_marcEditor_loadRecord.Click += new System.EventHandler(this.MenuItem_marcEditor_loadRecord_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 29);
            // 
            // toolStripButton_marcEditor_saveTo
            // 
            this.toolStripButton_marcEditor_saveTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_marcEditor_saveTo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_marcEditor_saveTo.Image")));
            this.toolStripButton_marcEditor_saveTo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_marcEditor_saveTo.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_marcEditor_saveTo.Name = "toolStripButton_marcEditor_saveTo";
            this.toolStripButton_marcEditor_saveTo.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_marcEditor_saveTo.Text = "复制书目记录(包括册、期、订购、实体信息)到其他库 F3";
            this.toolStripButton_marcEditor_saveTo.ToolTipText = "复制书目记录(包括册、期、订购、实体信息)到其他库 F3";
            this.toolStripButton_marcEditor_saveTo.Click += new System.EventHandler(this.toolStripButton1_marcEditor_saveTo_Click);
            // 
            // toolStripButton_setTargetRecord
            // 
            this.toolStripButton_setTargetRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_setTargetRecord.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_setTargetRecord.Image")));
            this.toolStripButton_setTargetRecord.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_setTargetRecord.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_setTargetRecord.Name = "toolStripButton_setTargetRecord";
            this.toolStripButton_setTargetRecord.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_setTargetRecord.Text = "设置目标记录";
            this.toolStripButton_setTargetRecord.Click += new System.EventHandler(this.toolStripButton_setTargetRecord_Click);
            // 
            // toolStripButton_marcEditor_moveTo
            // 
            this.toolStripButton_marcEditor_moveTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_marcEditor_moveTo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_marcEditor_moveTo.Image")));
            this.toolStripButton_marcEditor_moveTo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_marcEditor_moveTo.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_marcEditor_moveTo.Name = "toolStripButton_marcEditor_moveTo";
            this.toolStripButton_marcEditor_moveTo.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_marcEditor_moveTo.Text = "移动书目记录";
            this.toolStripButton_marcEditor_moveTo.Click += new System.EventHandler(this.toolStripButton_marcEditor_moveTo_Click);
            // 
            // toolStripSplitButton_searchDup
            // 
            this.toolStripSplitButton_searchDup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton_searchDup.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_searchDupInExistWindow,
            this.ToolStripMenuItem_searchDupInNewWindow,
            this.ToolStripMenuItem_checkUnique});
            this.toolStripSplitButton_searchDup.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_searchDup.Image")));
            this.toolStripSplitButton_searchDup.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripSplitButton_searchDup.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripSplitButton_searchDup.Name = "toolStripSplitButton_searchDup";
            this.toolStripSplitButton_searchDup.Size = new System.Drawing.Size(38, 26);
            this.toolStripSplitButton_searchDup.Text = "查重 (Ctrl+D)";
            this.toolStripSplitButton_searchDup.ButtonClick += new System.EventHandler(this.toolStripSplitButton_searchDup_ButtonClick);
            // 
            // ToolStripMenuItem_searchDupInExistWindow
            // 
            this.ToolStripMenuItem_searchDupInExistWindow.Name = "ToolStripMenuItem_searchDupInExistWindow";
            this.ToolStripMenuItem_searchDupInExistWindow.Size = new System.Drawing.Size(235, 22);
            this.ToolStripMenuItem_searchDupInExistWindow.Text = "在已经打开的查重窗中查重(&E)";
            this.ToolStripMenuItem_searchDupInExistWindow.Click += new System.EventHandler(this.ToolStripMenuItem_searchDupInExistWindow_Click);
            // 
            // ToolStripMenuItem_searchDupInNewWindow
            // 
            this.ToolStripMenuItem_searchDupInNewWindow.Name = "ToolStripMenuItem_searchDupInNewWindow";
            this.ToolStripMenuItem_searchDupInNewWindow.Size = new System.Drawing.Size(235, 22);
            this.ToolStripMenuItem_searchDupInNewWindow.Text = "在新开的查重窗中查重(&N)";
            this.ToolStripMenuItem_searchDupInNewWindow.Click += new System.EventHandler(this.ToolStripMenuItem_searchDupInNewWindow_Click);
            // 
            // ToolStripMenuItem_chechUnique
            // 
            this.ToolStripMenuItem_checkUnique.Name = "ToolStripMenuItem_chechUnique";
            this.ToolStripMenuItem_checkUnique.Size = new System.Drawing.Size(235, 22);
            this.ToolStripMenuItem_checkUnique.Text = "检查唯一性(&U)";
            this.ToolStripMenuItem_checkUnique.Click += new System.EventHandler(this.ToolStripMenuItem_checkUnique_Click);
            // 
            // toolStripButton_verifyData
            // 
            this.toolStripButton_verifyData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_verifyData.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_verifyData.Image")));
            this.toolStripButton_verifyData.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_verifyData.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_verifyData.Name = "toolStripButton_verifyData";
            this.toolStripButton_verifyData.Size = new System.Drawing.Size(26, 26);
            this.toolStripButton_verifyData.Text = "校验数据 (Ctrl+Y)";
            this.toolStripButton_verifyData.Click += new System.EventHandler(this.toolStripButton_verifyData_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 29);
            // 
            // toolStripButton_next
            // 
            this.toolStripButton_next.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_next.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_next.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_next.Image")));
            this.toolStripButton_next.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_next.Name = "toolStripButton_next";
            this.toolStripButton_next.Size = new System.Drawing.Size(23, 26);
            this.toolStripButton_next.Text = "下一记录";
            this.toolStripButton_next.Click += new System.EventHandler(this.toolStripButton_next_Click);
            // 
            // toolStripButton_prev
            // 
            this.toolStripButton_prev.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_prev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_prev.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prev.Image")));
            this.toolStripButton_prev.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_prev.Name = "toolStripButton_prev";
            this.toolStripButton_prev.Size = new System.Drawing.Size(23, 26);
            this.toolStripButton_prev.Text = "上一记录";
            this.toolStripButton_prev.Click += new System.EventHandler(this.toolStripButton_prev_Click);
            // 
            // toolStripButton_option
            // 
            this.toolStripButton_option.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_option.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_option.Image")));
            this.toolStripButton_option.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_option.Name = "toolStripButton_option";
            this.toolStripButton_option.Size = new System.Drawing.Size(23, 26);
            this.toolStripButton_option.Text = "选项";
            this.toolStripButton_option.Click += new System.EventHandler(this.toolStripButton_option_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 29);
            // 
            // toolStripSplitButton_insertCoverImage
            // 
            this.toolStripSplitButton_insertCoverImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton_insertCoverImage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_insertCoverImageFromClipboard,
            this.ToolStripMenuItem_removeCoverImage,
            this.ToolStripMenuItem_insertCoverImageFromCamera});
            this.toolStripSplitButton_insertCoverImage.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_insertCoverImage.Image")));
            this.toolStripSplitButton_insertCoverImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_insertCoverImage.Name = "toolStripSplitButton_insertCoverImage";
            this.toolStripSplitButton_insertCoverImage.Size = new System.Drawing.Size(32, 26);
            this.toolStripSplitButton_insertCoverImage.Text = "插入封面图像";
            this.toolStripSplitButton_insertCoverImage.ButtonClick += new System.EventHandler(this.toolStripSplitButton_insertCoverImage_ButtonClick);
            // 
            // ToolStripMenuItem_insertCoverImageFromClipboard
            // 
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Name = "ToolStripMenuItem_insertCoverImageFromClipboard";
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Size = new System.Drawing.Size(212, 22);
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Text = "从剪贴板插入封面图像(&C)";
            this.ToolStripMenuItem_insertCoverImageFromClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_insertCoverImageFromClipboard_Click);
            // 
            // ToolStripMenuItem_insertCoverImageFromCamera
            // 
            this.ToolStripMenuItem_insertCoverImageFromCamera.Name = "ToolStripMenuItem_insertCoverImageFromCamera";
            this.ToolStripMenuItem_insertCoverImageFromCamera.Size = new System.Drawing.Size(212, 22);
            this.ToolStripMenuItem_insertCoverImageFromCamera.Text = "从摄像头插入封面图像(&A)";
            this.ToolStripMenuItem_insertCoverImageFromCamera.Click += new System.EventHandler(this.ToolStripMenuItem_insertCoverImageFromCamera_Click);
            // 
            // tabControl_itemAndIssue
            // 
            this.tabControl_itemAndIssue.Controls.Add(this.tabPage_item);
            this.tabControl_itemAndIssue.Controls.Add(this.tabPage_issue);
            this.tabControl_itemAndIssue.Controls.Add(this.tabPage_order);
            this.tabControl_itemAndIssue.Controls.Add(this.tabPage_object);
            this.tabControl_itemAndIssue.Controls.Add(this.tabPage_comment);
            this.tabControl_itemAndIssue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_itemAndIssue.Location = new System.Drawing.Point(0, 0);
            this.tabControl_itemAndIssue.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_itemAndIssue.Name = "tabControl_itemAndIssue";
            this.tabControl_itemAndIssue.SelectedIndex = 0;
            this.tabControl_itemAndIssue.Size = new System.Drawing.Size(732, 133);
            this.tabControl_itemAndIssue.TabIndex = 1;
            // 
            // tabPage_item
            // 
            this.tabPage_item.Controls.Add(this.entityControl1);
            this.tabPage_item.Location = new System.Drawing.Point(4, 22);
            this.tabPage_item.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_item.Name = "tabPage_item";
            this.tabPage_item.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_item.Size = new System.Drawing.Size(724, 107);
            this.tabPage_item.TabIndex = 0;
            this.tabPage_item.Text = "册";
            this.tabPage_item.UseVisualStyleBackColor = true;
            // 
            // entityControl1
            // 
            this.entityControl1.AutoSize = true;
            this.entityControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.entityControl1.BiblioRecPath = "";
            this.entityControl1.Changed = false;
            this.entityControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityControl1.Location = new System.Drawing.Point(2, 2);
            this.entityControl1.Margin = new System.Windows.Forms.Padding(2);
            this.entityControl1.Name = "entityControl1";
            this.entityControl1.Size = new System.Drawing.Size(720, 103);
            this.entityControl1.TabIndex = 3;
            this.entityControl1.Enter += new System.EventHandler(this.entityControl1_Enter);
            this.entityControl1.Leave += new System.EventHandler(this.entityControl1_Leave);
            // 
            // tabPage_issue
            // 
            this.tabPage_issue.Controls.Add(this.issueControl1);
            this.tabPage_issue.Location = new System.Drawing.Point(4, 22);
            this.tabPage_issue.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_issue.Name = "tabPage_issue";
            this.tabPage_issue.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_issue.Size = new System.Drawing.Size(724, 107);
            this.tabPage_issue.TabIndex = 1;
            this.tabPage_issue.Text = "期";
            this.tabPage_issue.UseVisualStyleBackColor = true;
            // 
            // issueControl1
            // 
            this.issueControl1.AutoSize = true;
            this.issueControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.issueControl1.BiblioRecPath = "";
            this.issueControl1.Changed = false;
            this.issueControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.issueControl1.Location = new System.Drawing.Point(2, 2);
            this.issueControl1.Margin = new System.Windows.Forms.Padding(2);
            this.issueControl1.Name = "issueControl1";
            this.issueControl1.Size = new System.Drawing.Size(720, 103);
            this.issueControl1.TabIndex = 0;
            // 
            // tabPage_order
            // 
            this.tabPage_order.Controls.Add(this.orderControl1);
            this.tabPage_order.Location = new System.Drawing.Point(4, 22);
            this.tabPage_order.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_order.Name = "tabPage_order";
            this.tabPage_order.Size = new System.Drawing.Size(724, 107);
            this.tabPage_order.TabIndex = 3;
            this.tabPage_order.Text = "采购";
            this.tabPage_order.UseVisualStyleBackColor = true;
            // 
            // orderControl1
            // 
            this.orderControl1.AutoSize = true;
            this.orderControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.orderControl1.BiblioRecPath = "";
            this.orderControl1.Changed = false;
            this.orderControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderControl1.Location = new System.Drawing.Point(0, 0);
            this.orderControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.orderControl1.Name = "orderControl1";
            this.orderControl1.Size = new System.Drawing.Size(724, 107);
            this.orderControl1.TabIndex = 0;
            // 
            // tabPage_object
            // 
            this.tabPage_object.Controls.Add(this.binaryResControl1);
            this.tabPage_object.Location = new System.Drawing.Point(4, 22);
            this.tabPage_object.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_object.Name = "tabPage_object";
            this.tabPage_object.Size = new System.Drawing.Size(724, 107);
            this.tabPage_object.TabIndex = 2;
            this.tabPage_object.Text = "对象";
            this.tabPage_object.UseVisualStyleBackColor = true;
            // 
            // binaryResControl1
            // 
            this.binaryResControl1.AutoSize = true;
            this.binaryResControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.binaryResControl1.BiblioRecPath = "";
            this.binaryResControl1.Changed = false;
            this.binaryResControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryResControl1.Location = new System.Drawing.Point(0, 0);
            this.binaryResControl1.Margin = new System.Windows.Forms.Padding(2);
            this.binaryResControl1.Name = "binaryResControl1";
            this.binaryResControl1.Size = new System.Drawing.Size(724, 107);
            this.binaryResControl1.TabIndex = 0;
            this.binaryResControl1.Enter += new System.EventHandler(this.binaryResControl1_Enter);
            // 
            // tabPage_comment
            // 
            this.tabPage_comment.Controls.Add(this.commentControl1);
            this.tabPage_comment.Location = new System.Drawing.Point(4, 22);
            this.tabPage_comment.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_comment.Name = "tabPage_comment";
            this.tabPage_comment.Size = new System.Drawing.Size(724, 107);
            this.tabPage_comment.TabIndex = 4;
            this.tabPage_comment.Text = "评注";
            this.tabPage_comment.UseVisualStyleBackColor = true;
            // 
            // commentControl1
            // 
            this.commentControl1.AutoSize = true;
            this.commentControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commentControl1.BiblioRecPath = "";
            this.commentControl1.Changed = false;
            this.commentControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commentControl1.Location = new System.Drawing.Point(0, 0);
            this.commentControl1.Margin = new System.Windows.Forms.Padding(2);
            this.commentControl1.Name = "commentControl1";
            this.commentControl1.Size = new System.Drawing.Size(724, 107);
            this.commentControl1.TabIndex = 0;
            // 
            // panel_itemQuickInput
            // 
            this.panel_itemQuickInput.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel_itemQuickInput.Controls.Add(this.button_register);
            this.panel_itemQuickInput.Controls.Add(this.label3);
            this.panel_itemQuickInput.Controls.Add(this.textBox_itemBarcode);
            this.panel_itemQuickInput.Controls.Add(this.button_save);
            this.panel_itemQuickInput.Controls.Add(this.toolStrip1);
            this.panel_itemQuickInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_itemQuickInput.Location = new System.Drawing.Point(3, 351);
            this.panel_itemQuickInput.Name = "panel_itemQuickInput";
            this.panel_itemQuickInput.Size = new System.Drawing.Size(726, 29);
            this.panel_itemQuickInput.TabIndex = 6;
            // 
            // button_register
            // 
            this.button_register.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button_register.ContextMenuStrip = this.contextMenuStrip_selectRegisterType;
            this.button_register.Image = ((System.Drawing.Image)(resources.GetObject("button_register.Image")));
            this.button_register.Location = new System.Drawing.Point(500, 2);
            this.button_register.Name = "button_register";
            this.button_register.Size = new System.Drawing.Size(92, 23);
            this.button_register.TabIndex = 2;
            this.button_register.Text = "登记";
            this.button_register.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_register.UseVisualStyleBackColor = true;
            this.button_register.Click += new System.EventHandler(this.button_register_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "册条码号(&B):";
            // 
            // textBox_itemBarcode
            // 
            this.textBox_itemBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_itemBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_itemBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_itemBarcode.Location = new System.Drawing.Point(129, 4);
            this.textBox_itemBarcode.Name = "textBox_itemBarcode";
            this.textBox_itemBarcode.Size = new System.Drawing.Size(365, 21);
            this.textBox_itemBarcode.TabIndex = 1;
            this.textBox_itemBarcode.Enter += new System.EventHandler(this.textBox_itemBarcode_Enter);
            this.textBox_itemBarcode.Leave += new System.EventHandler(this.textBox_itemBarcode_Leave);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_hideItemQuickInput});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip1.Location = new System.Drawing.Point(1, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(29, 31);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_hideItemQuickInput
            // 
            this.toolStripButton_hideItemQuickInput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_hideItemQuickInput.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_hideItemQuickInput.Image")));
            this.toolStripButton_hideItemQuickInput.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_hideItemQuickInput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_hideItemQuickInput.Name = "toolStripButton_hideItemQuickInput";
            this.toolStripButton_hideItemQuickInput.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton_hideItemQuickInput.Text = "隐藏册条码快速输入面板";
            this.toolStripButton_hideItemQuickInput.Click += new System.EventHandler(this.toolStripButton_hideItemQuickInput_Click);
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList_itemType.Images.SetKeyName(0, "normal_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(1, "new_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(2, "changed_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(3, "deleted_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(4, "error_entity.bmp");
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ToolStripMenuItem_removeCoverImage
            // 
            this.ToolStripMenuItem_removeCoverImage.Name = "ToolStripMenuItem_removeCoverImage";
            this.ToolStripMenuItem_removeCoverImage.Size = new System.Drawing.Size(212, 22);
            this.ToolStripMenuItem_removeCoverImage.Text = "清除封面图像(&R)";
            this.ToolStripMenuItem_removeCoverImage.Click += new System.EventHandler(this.ToolStripMenuItem_removeCoverImage_Click);
            // 
            // EntityForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 393);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EntityForm";
            this.Text = "种册";
            this.Activated += new System.EventHandler(this.EntityForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EntityForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EntityForm_FormClosed);
            this.Load += new System.EventHandler(this.EntityForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.EntityForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.EntityForm_DragEnter);
            this.Enter += new System.EventHandler(this.EntityForm_Enter);
            this.Leave += new System.EventHandler(this.EntityForm_Leave);
            this.contextMenuStrip_selectRegisterType.ResumeLayout(false);
            this.contextMenuStrip_option.ResumeLayout(false);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.flowLayoutPanel_query.ResumeLayout(false);
            this.flowLayoutPanel_query.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.splitContainer_recordAndItems.Panel1.ResumeLayout(false);
            this.splitContainer_recordAndItems.Panel1.PerformLayout();
            this.splitContainer_recordAndItems.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_recordAndItems)).EndInit();
            this.splitContainer_recordAndItems.ResumeLayout(false);
            this.tableLayoutPanel_record.ResumeLayout(false);
            this.tableLayoutPanel_record.PerformLayout();
            this.panel_biblioInfo.ResumeLayout(false);
            this.tabControl_biblioInfo.ResumeLayout(false);
            this.tabPage_html.ResumeLayout(false);
            this.tabPage_marc.ResumeLayout(false);
            this.tabPage_template.ResumeLayout(false);
            this.toolStrip_marcEditor.ResumeLayout(false);
            this.toolStrip_marcEditor.PerformLayout();
            this.tabControl_itemAndIssue.ResumeLayout(false);
            this.tabPage_item.ResumeLayout(false);
            this.tabPage_item.PerformLayout();
            this.tabPage_issue.ResumeLayout(false);
            this.tabPage_issue.PerformLayout();
            this.tabPage_order.ResumeLayout(false);
            this.tabPage_order.PerformLayout();
            this.tabPage_object.ResumeLayout(false);
            this.tabPage_object.PerformLayout();
            this.tabPage_comment.ResumeLayout(false);
            this.tabPage_comment.PerformLayout();
            this.panel_itemQuickInput.ResumeLayout(false);
            this.panel_itemQuickInput.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_query;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.ComboBox comboBox_from;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.CheckBox checkBox_autoDetectQueryBarcode;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_selectRegisterType;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SearchOnly;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_quickRegister;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_register;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_option;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted;
        private System.Windows.Forms.CheckBox checkBox_autoSavePrev;
        private System.Windows.Forms.SplitContainer splitContainer_recordAndItems;
        private System.Windows.Forms.ToolStrip toolStrip_marcEditor;
        private System.Windows.Forms.ToolStripButton toolStripButton_marcEditor_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_marcEditor_loadTemplate;
        private System.Windows.Forms.ToolStripButton toolStripButton_marcEditor_delete;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_marcEditor_someFunc;
        private System.Windows.Forms.ToolStripButton toolStripButton_marcEditor_saveTemplate;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_viewXml;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_viewOriginXml;
        private System.Windows.Forms.ToolStripButton toolStripButton_marcEditor_saveTo;
        private System.Windows.Forms.ToolStripButton toolStripButton_prev;
        private System.Windows.Forms.ToolStripButton toolStripButton_next;
        private DigitalPlatform.FixedTabControl tabControl_biblioInfo;
        private System.Windows.Forms.TabPage tabPage_html;
        private System.Windows.Forms.WebBrowser webBrowser_biblioRecord;
        private System.Windows.Forms.TabPage tabPage_marc;
        private DigitalPlatform.Marc.MarcEditor m_marcEditor;
        private System.Windows.Forms.TabControl tabControl_itemAndIssue;
        private System.Windows.Forms.TabPage tabPage_item;
        private EntityControl entityControl1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_itemBarcode;
        private System.Windows.Forms.Button button_register;
        private System.Windows.Forms.TabPage tabPage_issue;
        private IssueControl issueControl1;
        private System.Windows.Forms.TabPage tabPage_order;
        private OrderControl orderControl1;
        private System.Windows.Forms.TabPage tabPage_object;
        private DigitalPlatform.CirculationClient.BinaryResControl binaryResControl1;
        private System.Windows.Forms.ToolStripTextBox textBox_biblioRecPath;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_record;
        private System.Windows.Forms.ToolStripButton toolStripButton_setTargetRecord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_loadTargetBiblioRecord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exportAllInfoToXmlFile;
        private System.Windows.Forms.ToolStripMenuItem StripMenuItem_importFromXmlFile;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_viewMarcJidaoData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.TabPage tabPage_comment;
        private CommentControl commentControl1;
        private System.Windows.Forms.Panel panel_itemQuickInput;
        private System.Windows.Forms.ToolStripButton toolStripButton_option;
        private System.Windows.Forms.ToolStripButton toolStripButton_clear;
        private System.Windows.Forms.Panel panel_biblioInfo;
        private System.Windows.Forms.ToolStripButton toolStripButton_saveAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_verifyData;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_hideItemQuickInput;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButton_hideSearchPanel;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_enableSaveAllButton;
        private System.Windows.Forms.ToolStripButton toolStripButton_marcEditor_moveTo;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_searchDup;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_searchDupInExistWindow;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_searchDupInNewWindow;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_checkUnique;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_marcEditor_setActiveCatalogingRule;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_insertCoverImage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_insertCoverImageFromClipboard;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_insertCoverImageFromCamera;
        private System.Windows.Forms.ComboBox comboBox_matchStyle;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_biblioDbNames;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_getKeys;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_getSummary;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.TabPage tabPage_template;
        private DigitalPlatform.EasyMarc.EasyMarcControl easyMarcControl1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_editMacroTable;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_removeCoverImage;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_fixed;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_marcEditor_loadRecord;

    }
}