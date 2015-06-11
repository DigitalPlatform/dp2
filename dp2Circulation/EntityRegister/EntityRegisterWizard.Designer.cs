namespace dp2Circulation
{
    partial class EntityRegisterWizard
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
            // 205/6/7
            if (this._imageManager != null)
            {
                this._imageManager.StopThread(true);
                try
                {
                    this._imageManager.ClearList();
                }
                catch
                {

                }
                this._imageManager.DeleteTempFiles();
                this._imageManager = null;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityRegisterWizard));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_settings = new System.Windows.Forms.TabPage();
            this.button_settings_bilbioDefault = new System.Windows.Forms.Button();
            this.button_settings_reCreateServersXml = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_settings_colorStyle = new System.Windows.Forms.ComboBox();
            this.checkBox_settings_keyboardWizard = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_settings_needBatchNo = new System.Windows.Forms.CheckBox();
            this.checkBox_settings_needPrice = new System.Windows.Forms.CheckBox();
            this.checkBox_settings_needBookType = new System.Windows.Forms.CheckBox();
            this.checkBox_settings_needLocation = new System.Windows.Forms.CheckBox();
            this.checkBox_settings_needItemBarcode = new System.Windows.Forms.CheckBox();
            this.checkBox_settings_needAccessNo = new System.Windows.Forms.CheckBox();
            this.button_settings_entityDefault = new System.Windows.Forms.Button();
            this.tabPage_searchBiblio = new System.Windows.Forms.TabPage();
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.button_search = new System.Windows.Forms.Button();
            this.dpTable_browseLines = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_no = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_recPath = new DigitalPlatform.CommonControl.DpColumn();
            this.tabPage_biblioAndItems = new System.Windows.Forms.TabPage();
            this.splitContainer_biblioAndItems = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.imageList_progress = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_start = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_next = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prev = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
            this.easyMarcControl1 = new DigitalPlatform.EasyMarc.EasyMarcControl();
            this.tabControl_main.SuspendLayout();
            this.tabPage_settings.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage_searchBiblio.SuspendLayout();
            this.tabPage_biblioAndItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_biblioAndItems)).BeginInit();
            this.splitContainer_biblioAndItems.Panel1.SuspendLayout();
            this.splitContainer_biblioAndItems.Panel2.SuspendLayout();
            this.splitContainer_biblioAndItems.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_settings);
            this.tabControl_main.Controls.Add(this.tabPage_searchBiblio);
            this.tabControl_main.Controls.Add(this.tabPage_biblioAndItems);
            this.tabControl_main.Location = new System.Drawing.Point(0, 2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(474, 277);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl_main_DrawItem);
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_settings
            // 
            this.tabPage_settings.AutoScroll = true;
            this.tabPage_settings.BackColor = System.Drawing.Color.DimGray;
            this.tabPage_settings.Controls.Add(this.button_settings_bilbioDefault);
            this.tabPage_settings.Controls.Add(this.button_settings_reCreateServersXml);
            this.tabPage_settings.Controls.Add(this.label2);
            this.tabPage_settings.Controls.Add(this.comboBox_settings_colorStyle);
            this.tabPage_settings.Controls.Add(this.checkBox_settings_keyboardWizard);
            this.tabPage_settings.Controls.Add(this.groupBox1);
            this.tabPage_settings.Controls.Add(this.button_settings_entityDefault);
            this.tabPage_settings.Location = new System.Drawing.Point(4, 22);
            this.tabPage_settings.Name = "tabPage_settings";
            this.tabPage_settings.Size = new System.Drawing.Size(466, 251);
            this.tabPage_settings.TabIndex = 2;
            this.tabPage_settings.Text = "参数设定";
            // 
            // button_settings_bilbioDefault
            // 
            this.button_settings_bilbioDefault.Location = new System.Drawing.Point(9, 79);
            this.button_settings_bilbioDefault.Name = "button_settings_bilbioDefault";
            this.button_settings_bilbioDefault.Size = new System.Drawing.Size(241, 23);
            this.button_settings_bilbioDefault.TabIndex = 8;
            this.button_settings_bilbioDefault.Text = "书目记录缺省值";
            this.button_settings_bilbioDefault.UseVisualStyleBackColor = true;
            this.button_settings_bilbioDefault.Click += new System.EventHandler(this.button_settings_bilbioDefault_Click);
            // 
            // button_settings_reCreateServersXml
            // 
            this.button_settings_reCreateServersXml.Location = new System.Drawing.Point(9, 128);
            this.button_settings_reCreateServersXml.Name = "button_settings_reCreateServersXml";
            this.button_settings_reCreateServersXml.Size = new System.Drawing.Size(241, 23);
            this.button_settings_reCreateServersXml.TabIndex = 7;
            this.button_settings_reCreateServersXml.Text = "重新创建 servers.xml 配置文件";
            this.button_settings_reCreateServersXml.UseVisualStyleBackColor = true;
            this.button_settings_reCreateServersXml.Click += new System.EventHandler(this.button_setting_reCreateServersXml_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(281, 218);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "颜色风格(&C):";
            // 
            // comboBox_settings_colorStyle
            // 
            this.comboBox_settings_colorStyle.FormattingEnabled = true;
            this.comboBox_settings_colorStyle.Items.AddRange(new object[] {
            "dark",
            "light"});
            this.comboBox_settings_colorStyle.Location = new System.Drawing.Point(376, 215);
            this.comboBox_settings_colorStyle.Name = "comboBox_settings_colorStyle";
            this.comboBox_settings_colorStyle.Size = new System.Drawing.Size(135, 20);
            this.comboBox_settings_colorStyle.TabIndex = 5;
            this.comboBox_settings_colorStyle.SelectedIndexChanged += new System.EventHandler(this.comboBox_settings_colorStyle_SelectedIndexChanged);
            // 
            // checkBox_settings_keyboardWizard
            // 
            this.checkBox_settings_keyboardWizard.AutoSize = true;
            this.checkBox_settings_keyboardWizard.Location = new System.Drawing.Point(9, 16);
            this.checkBox_settings_keyboardWizard.Name = "checkBox_settings_keyboardWizard";
            this.checkBox_settings_keyboardWizard.Size = new System.Drawing.Size(114, 16);
            this.checkBox_settings_keyboardWizard.TabIndex = 4;
            this.checkBox_settings_keyboardWizard.Text = "打开输入面板(&K)";
            this.checkBox_settings_keyboardWizard.UseVisualStyleBackColor = true;
            this.checkBox_settings_keyboardWizard.CheckedChanged += new System.EventHandler(this.checkBox_settings_keyboardWizard_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_settings_needBatchNo);
            this.groupBox1.Controls.Add(this.checkBox_settings_needPrice);
            this.groupBox1.Controls.Add(this.checkBox_settings_needBookType);
            this.groupBox1.Controls.Add(this.checkBox_settings_needLocation);
            this.groupBox1.Controls.Add(this.checkBox_settings_needItemBarcode);
            this.groupBox1.Controls.Add(this.checkBox_settings_needAccessNo);
            this.groupBox1.Location = new System.Drawing.Point(281, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(230, 184);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "册记录格式检查";
            // 
            // checkBox_settings_needBatchNo
            // 
            this.checkBox_settings_needBatchNo.AutoSize = true;
            this.checkBox_settings_needBatchNo.Location = new System.Drawing.Point(28, 141);
            this.checkBox_settings_needBatchNo.Name = "checkBox_settings_needBatchNo";
            this.checkBox_settings_needBatchNo.Size = new System.Drawing.Size(126, 16);
            this.checkBox_settings_needBatchNo.TabIndex = 5;
            this.checkBox_settings_needBatchNo.Text = "必须具备批次号(&B)";
            this.checkBox_settings_needBatchNo.UseVisualStyleBackColor = true;
            // 
            // checkBox_settings_needPrice
            // 
            this.checkBox_settings_needPrice.AutoSize = true;
            this.checkBox_settings_needPrice.Location = new System.Drawing.Point(28, 97);
            this.checkBox_settings_needPrice.Name = "checkBox_settings_needPrice";
            this.checkBox_settings_needPrice.Size = new System.Drawing.Size(114, 16);
            this.checkBox_settings_needPrice.TabIndex = 3;
            this.checkBox_settings_needPrice.Text = "必须具备价格(&P)";
            this.checkBox_settings_needPrice.UseVisualStyleBackColor = true;
            // 
            // checkBox_settings_needBookType
            // 
            this.checkBox_settings_needBookType.AutoSize = true;
            this.checkBox_settings_needBookType.Location = new System.Drawing.Point(28, 31);
            this.checkBox_settings_needBookType.Name = "checkBox_settings_needBookType";
            this.checkBox_settings_needBookType.Size = new System.Drawing.Size(126, 16);
            this.checkBox_settings_needBookType.TabIndex = 0;
            this.checkBox_settings_needBookType.Text = "必须具备册类型(&T)";
            this.checkBox_settings_needBookType.UseVisualStyleBackColor = true;
            // 
            // checkBox_settings_needLocation
            // 
            this.checkBox_settings_needLocation.AutoSize = true;
            this.checkBox_settings_needLocation.Location = new System.Drawing.Point(28, 53);
            this.checkBox_settings_needLocation.Name = "checkBox_settings_needLocation";
            this.checkBox_settings_needLocation.Size = new System.Drawing.Size(126, 16);
            this.checkBox_settings_needLocation.TabIndex = 1;
            this.checkBox_settings_needLocation.Text = "必须具备馆藏地(&L)";
            this.checkBox_settings_needLocation.UseVisualStyleBackColor = true;
            // 
            // checkBox_settings_needItemBarcode
            // 
            this.checkBox_settings_needItemBarcode.AutoSize = true;
            this.checkBox_settings_needItemBarcode.Location = new System.Drawing.Point(28, 119);
            this.checkBox_settings_needItemBarcode.Name = "checkBox_settings_needItemBarcode";
            this.checkBox_settings_needItemBarcode.Size = new System.Drawing.Size(138, 16);
            this.checkBox_settings_needItemBarcode.TabIndex = 4;
            this.checkBox_settings_needItemBarcode.Text = "必须具备册条码号(&B)";
            this.checkBox_settings_needItemBarcode.UseVisualStyleBackColor = true;
            // 
            // checkBox_settings_needAccessNo
            // 
            this.checkBox_settings_needAccessNo.AutoSize = true;
            this.checkBox_settings_needAccessNo.Location = new System.Drawing.Point(28, 75);
            this.checkBox_settings_needAccessNo.Name = "checkBox_settings_needAccessNo";
            this.checkBox_settings_needAccessNo.Size = new System.Drawing.Size(126, 16);
            this.checkBox_settings_needAccessNo.TabIndex = 2;
            this.checkBox_settings_needAccessNo.Text = "必须具备索取号(&A)";
            this.checkBox_settings_needAccessNo.UseVisualStyleBackColor = true;
            // 
            // button_settings_entityDefault
            // 
            this.button_settings_entityDefault.Location = new System.Drawing.Point(9, 47);
            this.button_settings_entityDefault.Name = "button_settings_entityDefault";
            this.button_settings_entityDefault.Size = new System.Drawing.Size(241, 23);
            this.button_settings_entityDefault.TabIndex = 0;
            this.button_settings_entityDefault.Text = "册记录缺省值";
            this.button_settings_entityDefault.UseVisualStyleBackColor = true;
            this.button_settings_entityDefault.Click += new System.EventHandler(this.button_settings_entityDefault_Click);
            // 
            // tabPage_searchBiblio
            // 
            this.tabPage_searchBiblio.BackColor = System.Drawing.Color.DimGray;
            this.tabPage_searchBiblio.Controls.Add(this.comboBox_from);
            this.tabPage_searchBiblio.Controls.Add(this.textBox_queryWord);
            this.tabPage_searchBiblio.Controls.Add(this.button_search);
            this.tabPage_searchBiblio.Controls.Add(this.dpTable_browseLines);
            this.tabPage_searchBiblio.ForeColor = System.Drawing.Color.White;
            this.tabPage_searchBiblio.Location = new System.Drawing.Point(4, 22);
            this.tabPage_searchBiblio.Name = "tabPage_searchBiblio";
            this.tabPage_searchBiblio.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_searchBiblio.Size = new System.Drawing.Size(466, 251);
            this.tabPage_searchBiblio.TabIndex = 0;
            this.tabPage_searchBiblio.Text = "检索书目";
            // 
            // comboBox_from
            // 
            this.comboBox_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.Items.AddRange(new object[] {
            "ISBN",
            "书名",
            "作者",
            "出版社"});
            this.comboBox_from.Location = new System.Drawing.Point(6, 6);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(121, 20);
            this.comboBox_from.TabIndex = 9;
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_queryWord.Font = new System.Drawing.Font("宋体", 12F);
            this.textBox_queryWord.Location = new System.Drawing.Point(133, 5);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(249, 26);
            this.textBox_queryWord.TabIndex = 10;
            this.textBox_queryWord.Enter += new System.EventHandler(this.textBox_queryWord_Enter);
            this.textBox_queryWord.Leave += new System.EventHandler(this.textBox_queryWord_Leave);
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Image = ((System.Drawing.Image)(resources.GetObject("button_search.Image")));
            this.button_search.Location = new System.Drawing.Point(388, 4);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(75, 23);
            this.button_search.TabIndex = 11;
            this.button_search.Text = "检索(&S)";
            this.button_search.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // dpTable_browseLines
            // 
            this.dpTable_browseLines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dpTable_browseLines.AutoDocCenter = true;
            this.dpTable_browseLines.BackColor = System.Drawing.Color.DimGray;
            this.dpTable_browseLines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dpTable_browseLines.Columns.Add(this.dpColumn_no);
            this.dpTable_browseLines.Columns.Add(this.dpColumn_recPath);
            this.dpTable_browseLines.ColumnsBackColor = System.Drawing.Color.Gray;
            this.dpTable_browseLines.ColumnsForeColor = System.Drawing.Color.White;
            this.dpTable_browseLines.DocumentBorderColor = System.Drawing.Color.DarkGray;
            this.dpTable_browseLines.DocumentMargin = new System.Windows.Forms.Padding(8);
            this.dpTable_browseLines.DocumentOrgX = ((long)(0));
            this.dpTable_browseLines.DocumentOrgY = ((long)(0));
            this.dpTable_browseLines.DocumentShadowColor = System.Drawing.Color.Black;
            this.dpTable_browseLines.FocusedItem = null;
            this.dpTable_browseLines.FullRowSelect = true;
            this.dpTable_browseLines.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_browseLines.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_browseLines.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_browseLines.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_browseLines.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_browseLines.Location = new System.Drawing.Point(3, 33);
            this.dpTable_browseLines.Name = "dpTable_browseLines";
            this.dpTable_browseLines.Padding = new System.Windows.Forms.Padding(12);
            this.dpTable_browseLines.Size = new System.Drawing.Size(460, 215);
            this.dpTable_browseLines.TabIndex = 8;
            this.dpTable_browseLines.Text = "dpTable1";
            this.dpTable_browseLines.PaintRegion += new DigitalPlatform.CommonControl.PaintRegionEventHandler(this.dpTable_browseLines_PaintRegion);
            this.dpTable_browseLines.DoubleClick += new System.EventHandler(this.dpTable_browseLines_DoubleClick);
            this.dpTable_browseLines.Enter += new System.EventHandler(this.dpTable_browseLines_Enter);
            this.dpTable_browseLines.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dpTable_browseLines_KeyDown);
            // 
            // dpColumn_no
            // 
            this.dpColumn_no.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_no.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_no.Font = null;
            this.dpColumn_no.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_no.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_no.Text = "序号";
            this.dpColumn_no.Width = 50;
            // 
            // dpColumn_recPath
            // 
            this.dpColumn_recPath.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_recPath.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_recPath.Font = null;
            this.dpColumn_recPath.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_recPath.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_recPath.Text = "记录路径";
            // 
            // tabPage_biblioAndItems
            // 
            this.tabPage_biblioAndItems.BackColor = System.Drawing.Color.DimGray;
            this.tabPage_biblioAndItems.Controls.Add(this.splitContainer_biblioAndItems);
            this.tabPage_biblioAndItems.ForeColor = System.Drawing.Color.White;
            this.tabPage_biblioAndItems.Location = new System.Drawing.Point(4, 22);
            this.tabPage_biblioAndItems.Name = "tabPage_biblioAndItems";
            this.tabPage_biblioAndItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_biblioAndItems.Size = new System.Drawing.Size(466, 251);
            this.tabPage_biblioAndItems.TabIndex = 1;
            this.tabPage_biblioAndItems.Text = "种和册";
            // 
            // splitContainer_biblioAndItems
            // 
            this.splitContainer_biblioAndItems.BackColor = System.Drawing.Color.DimGray;
            this.splitContainer_biblioAndItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_biblioAndItems.Location = new System.Drawing.Point(3, 3);
            this.splitContainer_biblioAndItems.Name = "splitContainer_biblioAndItems";
            // 
            // splitContainer_biblioAndItems.Panel1
            // 
            this.splitContainer_biblioAndItems.Panel1.Controls.Add(this.easyMarcControl1);
            // 
            // splitContainer_biblioAndItems.Panel2
            // 
            this.splitContainer_biblioAndItems.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer_biblioAndItems.Size = new System.Drawing.Size(460, 245);
            this.splitContainer_biblioAndItems.SplitterDistance = 231;
            this.splitContainer_biblioAndItems.SplitterWidth = 8;
            this.splitContainer_biblioAndItems.TabIndex = 0;
            this.splitContainer_biblioAndItems.DoubleClick += new System.EventHandler(this.splitContainer_biblioAndItems_DoubleClick);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(221, 245);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.SizeChanged += new System.EventHandler(this.flowLayoutPanel1_SizeChanged);
            this.flowLayoutPanel1.Enter += new System.EventHandler(this.flowLayoutPanel1_Enter);
            this.flowLayoutPanel1.Leave += new System.EventHandler(this.flowLayoutPanel1_Leave);
            // 
            // imageList_progress
            // 
            this.imageList_progress.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_progress.ImageStream")));
            this.imageList_progress.TransparentColor = System.Drawing.Color.White;
            this.imageList_progress.Images.SetKeyName(0, "process_32.png");
            this.imageList_progress.Images.SetKeyName(1, "action_success_24.png");
            this.imageList_progress.Images.SetKeyName(2, "dialog_error_24.png");
            this.imageList_progress.Images.SetKeyName(3, "progress_information.bmp");
            this.imageList_progress.Images.SetKeyName(4, "circle_24.png");
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.Gray;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_start,
            this.toolStripButton_next,
            this.toolStripButton_prev,
            this.toolStripSeparator1,
            this.toolStripButton_new,
            this.toolStripButton_save,
            this.toolStripSeparator2,
            this.toolStripButton_delete});
            this.toolStrip1.Location = new System.Drawing.Point(0, 282);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(473, 25);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_start
            // 
            this.toolStripButton_start.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_start.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_start.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_start.Image")));
            this.toolStripButton_start.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_start.Name = "toolStripButton_start";
            this.toolStripButton_start.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_start.Text = "重新开始";
            this.toolStripButton_start.Click += new System.EventHandler(this.toolStripButton_start_Click);
            // 
            // toolStripButton_next
            // 
            this.toolStripButton_next.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_next.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_next.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_next.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_next.Image")));
            this.toolStripButton_next.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_next.Name = "toolStripButton_next";
            this.toolStripButton_next.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton_next.Text = "下一步";
            this.toolStripButton_next.Click += new System.EventHandler(this.toolStripButton_next_Click);
            // 
            // toolStripButton_prev
            // 
            this.toolStripButton_prev.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_prev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_prev.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_prev.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prev.Image")));
            this.toolStripButton_prev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_prev.Name = "toolStripButton_prev";
            this.toolStripButton_prev.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton_prev.Text = "上一步";
            this.toolStripButton_prev.Click += new System.EventHandler(this.toolStripButton_prev_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_new
            // 
            this.toolStripButton_new.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_new.Image")));
            this.toolStripButton_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_new.Name = "toolStripButton_new";
            this.toolStripButton_new.Size = new System.Drawing.Size(52, 22);
            this.toolStripButton_new.Text = "新建";
            this.toolStripButton_new.ToolTipText = "新建书目记录";
            this.toolStripButton_new.Click += new System.EventHandler(this.toolStripButton_new_Click);
            // 
            // toolStripButton_save
            // 
            this.toolStripButton_save.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_save.Image")));
            this.toolStripButton_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_save.Name = "toolStripButton_save";
            this.toolStripButton_save.Size = new System.Drawing.Size(52, 22);
            this.toolStripButton_save.Text = "保存";
            this.toolStripButton_save.Click += new System.EventHandler(this.toolStripButton_save_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_delete
            // 
            this.toolStripButton_delete.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_delete.Image")));
            this.toolStripButton_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_delete.Name = "toolStripButton_delete";
            this.toolStripButton_delete.Size = new System.Drawing.Size(52, 22);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // easyMarcControl1
            // 
            this.easyMarcControl1.AutoScroll = true;
            this.easyMarcControl1.CaptionWidth = 106;
            this.easyMarcControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyMarcControl1.HideIndicator = true;
            this.easyMarcControl1.HideSelection = false;
            this.easyMarcControl1.IncludeNumber = false;
            this.easyMarcControl1.Location = new System.Drawing.Point(0, 0);
            this.easyMarcControl1.MarcDefDom = null;
            this.easyMarcControl1.Name = "easyMarcControl1";
            this.easyMarcControl1.Size = new System.Drawing.Size(231, 245);
            this.easyMarcControl1.TabIndex = 0;
            this.easyMarcControl1.SelectionChanged += new System.EventHandler(this.easyMarcControl1_SelectionChanged);
            this.easyMarcControl1.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.easyMarcControl1_GetConfigDom);
            this.easyMarcControl1.Enter += new System.EventHandler(this.easyMarcControl1_Enter);
            this.easyMarcControl1.Leave += new System.EventHandler(this.easyMarcControl1_Leave);
            // 
            // EntityRegisterWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(473, 307);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tabControl_main);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "EntityRegisterWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "册登记向导";
            this.Activated += new System.EventHandler(this.EntityRegisterWizard_Activated);
            this.Deactivate += new System.EventHandler(this.EntityRegisterWizard_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EntityRegisterWizard_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EntityRegisterWizard_FormClosed);
            this.Load += new System.EventHandler(this.EntityRegisterWizard_Load);
            this.Enter += new System.EventHandler(this.EntityRegisterWizard_Enter);
            this.Leave += new System.EventHandler(this.EntityRegisterWizard_Leave);
            this.Move += new System.EventHandler(this.EntityRegisterWizard_Move);
            this.Resize += new System.EventHandler(this.EntityRegisterWizard_Resize);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_settings.ResumeLayout(false);
            this.tabPage_settings.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage_searchBiblio.ResumeLayout(false);
            this.tabPage_searchBiblio.PerformLayout();
            this.tabPage_biblioAndItems.ResumeLayout(false);
            this.splitContainer_biblioAndItems.Panel1.ResumeLayout(false);
            this.splitContainer_biblioAndItems.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_biblioAndItems)).EndInit();
            this.splitContainer_biblioAndItems.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_searchBiblio;
        private System.Windows.Forms.TabPage tabPage_biblioAndItems;
        private DigitalPlatform.CommonControl.DpTable dpTable_browseLines;
        private System.Windows.Forms.ComboBox comboBox_from;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.SplitContainer splitContainer_biblioAndItems;
        private DigitalPlatform.EasyMarc.EasyMarcControl easyMarcControl1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_no;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_recPath;
        private System.Windows.Forms.ImageList imageList_progress;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_start;
        private System.Windows.Forms.ToolStripButton toolStripButton_next;
        private System.Windows.Forms.ToolStripButton toolStripButton_prev;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_new;
        private System.Windows.Forms.TabPage tabPage_settings;
        private System.Windows.Forms.Button button_settings_entityDefault;
        private System.Windows.Forms.CheckBox checkBox_settings_needAccessNo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_settings_needItemBarcode;
        private System.Windows.Forms.CheckBox checkBox_settings_needLocation;
        private System.Windows.Forms.CheckBox checkBox_settings_needBookType;
        private System.Windows.Forms.CheckBox checkBox_settings_needPrice;
        private System.Windows.Forms.CheckBox checkBox_settings_needBatchNo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
        private System.Windows.Forms.CheckBox checkBox_settings_keyboardWizard;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_settings_colorStyle;
        private System.Windows.Forms.Button button_settings_reCreateServersXml;
        private System.Windows.Forms.Button button_settings_bilbioDefault;
    }
}