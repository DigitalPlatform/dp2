namespace dp2Circulation
{
    partial class QuickChangeEntityForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickChangeEntityForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_barcode = new System.Windows.Forms.TextBox();
            this.button_loadBarcode = new System.Windows.Forms.Button();
            this.contextMenuStrip_loadAction = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_loadOnly = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_loadAndAutoChange = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer_itemInfo = new System.Windows.Forms.SplitContainer();
            this.entityEditControl1 = new dp2Circulation.EntityEditControl();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_changeParam = new System.Windows.Forms.Button();
            this.button_saveCurrentRecord = new System.Windows.Forms.Button();
            this.tabControl_input = new System.Windows.Forms.TabControl();
            this.tabPage_barcodeInput = new System.Windows.Forms.TabPage();
            this.tabPage_barcodeFile = new System.Windows.Forms.TabPage();
            this.button_file_getBarcodeFilename = new System.Windows.Forms.Button();
            this.textBox_barcodeFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_beginByBarcodeFile = new System.Windows.Forms.Button();
            this.tabPage_recPathFile = new System.Windows.Forms.TabPage();
            this.button_getRecPathFileName = new System.Windows.Forms.Button();
            this.textBox_recPathFile = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_beginByRecPathFile = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.button_saveToBarcodeFile = new System.Windows.Forms.Button();
            this.textBox_outputBarcodes = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.contextMenuStrip_loadAction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemInfo)).BeginInit();
            this.splitContainer_itemInfo.Panel1.SuspendLayout();
            this.splitContainer_itemInfo.Panel2.SuspendLayout();
            this.splitContainer_itemInfo.SuspendLayout();
            this.tabControl_input.SuspendLayout();
            this.tabPage_barcodeInput.SuspendLayout();
            this.tabPage_barcodeFile.SuspendLayout();
            this.tabPage_recPathFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "册条码号(&B):";
            // 
            // textBox_barcode
            // 
            this.textBox_barcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_barcode.Location = new System.Drawing.Point(4, 22);
            this.textBox_barcode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_barcode.Name = "textBox_barcode";
            this.textBox_barcode.Size = new System.Drawing.Size(138, 21);
            this.textBox_barcode.TabIndex = 1;
            this.textBox_barcode.Enter += new System.EventHandler(this.textBox_barcode_Enter);
            // 
            // button_loadBarcode
            // 
            this.button_loadBarcode.ContextMenuStrip = this.contextMenuStrip_loadAction;
            this.button_loadBarcode.Location = new System.Drawing.Point(146, 22);
            this.button_loadBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.button_loadBarcode.Name = "button_loadBarcode";
            this.button_loadBarcode.Size = new System.Drawing.Size(143, 22);
            this.button_loadBarcode.TabIndex = 2;
            this.button_loadBarcode.Text = "装入并自动修改(&L)";
            this.button_loadBarcode.UseVisualStyleBackColor = true;
            this.button_loadBarcode.Click += new System.EventHandler(this.button_loadBarcode_Click);
            // 
            // contextMenuStrip_loadAction
            // 
            this.contextMenuStrip_loadAction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_loadOnly,
            this.ToolStripMenuItem_loadAndAutoChange});
            this.contextMenuStrip_loadAction.Name = "contextMenuStrip_loadAction";
            this.contextMenuStrip_loadAction.Size = new System.Drawing.Size(161, 48);
            // 
            // ToolStripMenuItem_loadOnly
            // 
            this.ToolStripMenuItem_loadOnly.Name = "ToolStripMenuItem_loadOnly";
            this.ToolStripMenuItem_loadOnly.Size = new System.Drawing.Size(160, 22);
            this.ToolStripMenuItem_loadOnly.Text = "只装入(不修改)";
            this.ToolStripMenuItem_loadOnly.Click += new System.EventHandler(this.ToolStripMenuItem_loadOnly_Click);
            // 
            // ToolStripMenuItem_loadAndAutoChange
            // 
            this.ToolStripMenuItem_loadAndAutoChange.Name = "ToolStripMenuItem_loadAndAutoChange";
            this.ToolStripMenuItem_loadAndAutoChange.Size = new System.Drawing.Size(160, 22);
            this.ToolStripMenuItem_loadAndAutoChange.Text = "装入并自动修改";
            this.ToolStripMenuItem_loadAndAutoChange.Click += new System.EventHandler(this.ToolStripMenuItem_loadAndAutoChange_Click);
            // 
            // splitContainer_itemInfo
            // 
            this.splitContainer_itemInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_itemInfo.Location = new System.Drawing.Point(0, 19);
            this.splitContainer_itemInfo.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_itemInfo.Name = "splitContainer_itemInfo";
            // 
            // splitContainer_itemInfo.Panel1
            // 
            this.splitContainer_itemInfo.Panel1.Controls.Add(this.entityEditControl1);
            // 
            // splitContainer_itemInfo.Panel2
            // 
            this.splitContainer_itemInfo.Panel2.Controls.Add(this.webBrowser_biblio);
            this.splitContainer_itemInfo.Size = new System.Drawing.Size(312, 178);
            this.splitContainer_itemInfo.SplitterDistance = 167;
            this.splitContainer_itemInfo.SplitterWidth = 6;
            this.splitContainer_itemInfo.TabIndex = 4;
            // 
            // entityEditControl1
            // 
            this.entityEditControl1.AccessNo = "";
            this.entityEditControl1.AutoScroll = true;
            this.entityEditControl1.BackColor = System.Drawing.SystemColors.Control;
            this.entityEditControl1.Barcode = "";
            this.entityEditControl1.BatchNo = "";
            this.entityEditControl1.Binding = "";
            this.entityEditControl1.BindingCost = "";
            this.entityEditControl1.BookType = "";
            this.entityEditControl1.BorrowDate = "";
            this.entityEditControl1.Borrower = "";
            this.entityEditControl1.BorrowPeriod = "";
            this.entityEditControl1.Changed = false;
            this.entityEditControl1.Comment = "";
            this.entityEditControl1.CreateState = dp2Circulation.ItemDisplayState.Normal;
            this.entityEditControl1.DisplayMode = "full";
            this.entityEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl1.ErrorInfo = "";
            this.entityEditControl1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl1.Initializing = true;
            this.entityEditControl1.Intact = "";
            this.entityEditControl1.Location = new System.Drawing.Point(0, 0);
            this.entityEditControl1.LocationString = "";
            this.entityEditControl1.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl1.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl1.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl1.MergeComment = "";
            this.entityEditControl1.MinimumSize = new System.Drawing.Size(75, 0);
            this.entityEditControl1.Name = "entityEditControl1";
            this.entityEditControl1.Operations = "";
            this.entityEditControl1.ParentId = "";
            this.entityEditControl1.Price = "";
            this.entityEditControl1.PublishTime = "";
            this.entityEditControl1.RecPath = "";
            this.entityEditControl1.RefID = "";
            this.entityEditControl1.RegisterNo = "";
            this.entityEditControl1.Seller = "";
            this.entityEditControl1.Size = new System.Drawing.Size(167, 178);
            this.entityEditControl1.Source = "";
            this.entityEditControl1.State = "";
            this.entityEditControl1.TabIndex = 3;
            this.entityEditControl1.TableMargin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.entityEditControl1.TablePadding = new System.Windows.Forms.Padding(12, 13, 12, 13);
            this.entityEditControl1.Volume = "";
            this.entityEditControl1.Enter += new System.EventHandler(this.entityEditControl1_Enter);
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_biblio.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(139, 178);
            this.webBrowser_biblio.TabIndex = 0;
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(0, 199);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.Size = new System.Drawing.Size(312, 14);
            this.textBox_message.TabIndex = 5;
            // 
            // button_changeParam
            // 
            this.button_changeParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_changeParam.Location = new System.Drawing.Point(129, 218);
            this.button_changeParam.Margin = new System.Windows.Forms.Padding(2);
            this.button_changeParam.Name = "button_changeParam";
            this.button_changeParam.Size = new System.Drawing.Size(105, 22);
            this.button_changeParam.TabIndex = 6;
            this.button_changeParam.Text = "动作参数(&P)...";
            this.button_changeParam.UseVisualStyleBackColor = true;
            this.button_changeParam.Click += new System.EventHandler(this.button_changeParam_Click);
            // 
            // button_saveCurrentRecord
            // 
            this.button_saveCurrentRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_saveCurrentRecord.Location = new System.Drawing.Point(0, 218);
            this.button_saveCurrentRecord.Margin = new System.Windows.Forms.Padding(2);
            this.button_saveCurrentRecord.Name = "button_saveCurrentRecord";
            this.button_saveCurrentRecord.Size = new System.Drawing.Size(116, 22);
            this.button_saveCurrentRecord.TabIndex = 7;
            this.button_saveCurrentRecord.Text = "保存当前记录(&S)";
            this.button_saveCurrentRecord.UseVisualStyleBackColor = true;
            this.button_saveCurrentRecord.Click += new System.EventHandler(this.button_saveCurrentRecord_Click);
            // 
            // tabControl_input
            // 
            this.tabControl_input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_input.Controls.Add(this.tabPage_barcodeInput);
            this.tabControl_input.Controls.Add(this.tabPage_barcodeFile);
            this.tabControl_input.Controls.Add(this.tabPage_recPathFile);
            this.tabControl_input.Location = new System.Drawing.Point(0, 10);
            this.tabControl_input.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_input.Name = "tabControl_input";
            this.tabControl_input.SelectedIndex = 0;
            this.tabControl_input.Size = new System.Drawing.Size(464, 74);
            this.tabControl_input.TabIndex = 8;
            // 
            // tabPage_barcodeInput
            // 
            this.tabPage_barcodeInput.Controls.Add(this.textBox_barcode);
            this.tabPage_barcodeInput.Controls.Add(this.label1);
            this.tabPage_barcodeInput.Controls.Add(this.button_loadBarcode);
            this.tabPage_barcodeInput.Location = new System.Drawing.Point(4, 22);
            this.tabPage_barcodeInput.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_barcodeInput.Name = "tabPage_barcodeInput";
            this.tabPage_barcodeInput.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_barcodeInput.Size = new System.Drawing.Size(456, 48);
            this.tabPage_barcodeInput.TabIndex = 0;
            this.tabPage_barcodeInput.Text = "键盘 / 条码阅读器";
            this.tabPage_barcodeInput.UseVisualStyleBackColor = true;
            // 
            // tabPage_barcodeFile
            // 
            this.tabPage_barcodeFile.Controls.Add(this.button_file_getBarcodeFilename);
            this.tabPage_barcodeFile.Controls.Add(this.textBox_barcodeFile);
            this.tabPage_barcodeFile.Controls.Add(this.label2);
            this.tabPage_barcodeFile.Controls.Add(this.button_beginByBarcodeFile);
            this.tabPage_barcodeFile.Location = new System.Drawing.Point(4, 22);
            this.tabPage_barcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_barcodeFile.Name = "tabPage_barcodeFile";
            this.tabPage_barcodeFile.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_barcodeFile.Size = new System.Drawing.Size(456, 48);
            this.tabPage_barcodeFile.TabIndex = 1;
            this.tabPage_barcodeFile.Text = "条码号文件";
            this.tabPage_barcodeFile.UseVisualStyleBackColor = true;
            // 
            // button_file_getBarcodeFilename
            // 
            this.button_file_getBarcodeFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_file_getBarcodeFilename.Location = new System.Drawing.Point(266, 22);
            this.button_file_getBarcodeFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_file_getBarcodeFilename.Name = "button_file_getBarcodeFilename";
            this.button_file_getBarcodeFilename.Size = new System.Drawing.Size(38, 22);
            this.button_file_getBarcodeFilename.TabIndex = 6;
            this.button_file_getBarcodeFilename.Text = "...";
            this.button_file_getBarcodeFilename.UseVisualStyleBackColor = true;
            this.button_file_getBarcodeFilename.Click += new System.EventHandler(this.button_file_getBarcodeFilename_Click);
            // 
            // textBox_barcodeFile
            // 
            this.textBox_barcodeFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_barcodeFile.Location = new System.Drawing.Point(4, 22);
            this.textBox_barcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_barcodeFile.Name = "textBox_barcodeFile";
            this.textBox_barcodeFile.Size = new System.Drawing.Size(258, 21);
            this.textBox_barcodeFile.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "条码号文件(&F):";
            // 
            // button_beginByBarcodeFile
            // 
            this.button_beginByBarcodeFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_beginByBarcodeFile.ContextMenuStrip = this.contextMenuStrip_loadAction;
            this.button_beginByBarcodeFile.Location = new System.Drawing.Point(308, 22);
            this.button_beginByBarcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_beginByBarcodeFile.Name = "button_beginByBarcodeFile";
            this.button_beginByBarcodeFile.Size = new System.Drawing.Size(127, 22);
            this.button_beginByBarcodeFile.TabIndex = 5;
            this.button_beginByBarcodeFile.Text = "启动自动修改(&B)";
            this.button_beginByBarcodeFile.UseVisualStyleBackColor = true;
            this.button_beginByBarcodeFile.Click += new System.EventHandler(this.button_beginByBarcodeFile_Click);
            // 
            // tabPage_recPathFile
            // 
            this.tabPage_recPathFile.Controls.Add(this.button_getRecPathFileName);
            this.tabPage_recPathFile.Controls.Add(this.textBox_recPathFile);
            this.tabPage_recPathFile.Controls.Add(this.label5);
            this.tabPage_recPathFile.Controls.Add(this.button_beginByRecPathFile);
            this.tabPage_recPathFile.Location = new System.Drawing.Point(4, 22);
            this.tabPage_recPathFile.Name = "tabPage_recPathFile";
            this.tabPage_recPathFile.Size = new System.Drawing.Size(456, 48);
            this.tabPage_recPathFile.TabIndex = 2;
            this.tabPage_recPathFile.Text = "记录路径文件";
            this.tabPage_recPathFile.UseVisualStyleBackColor = true;
            // 
            // button_getRecPathFileName
            // 
            this.button_getRecPathFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getRecPathFileName.Location = new System.Drawing.Point(269, 21);
            this.button_getRecPathFileName.Margin = new System.Windows.Forms.Padding(2);
            this.button_getRecPathFileName.Name = "button_getRecPathFileName";
            this.button_getRecPathFileName.Size = new System.Drawing.Size(38, 22);
            this.button_getRecPathFileName.TabIndex = 10;
            this.button_getRecPathFileName.Text = "...";
            this.button_getRecPathFileName.UseVisualStyleBackColor = true;
            this.button_getRecPathFileName.Click += new System.EventHandler(this.button_getRecPathFileName_Click);
            // 
            // textBox_recPathFile
            // 
            this.textBox_recPathFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recPathFile.Location = new System.Drawing.Point(7, 20);
            this.textBox_recPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_recPathFile.Name = "textBox_recPathFile";
            this.textBox_recPathFile.Size = new System.Drawing.Size(258, 21);
            this.textBox_recPathFile.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 6);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "记录路径文件(&F):";
            // 
            // button_beginByRecPathFile
            // 
            this.button_beginByRecPathFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_beginByRecPathFile.ContextMenuStrip = this.contextMenuStrip_loadAction;
            this.button_beginByRecPathFile.Location = new System.Drawing.Point(311, 21);
            this.button_beginByRecPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_beginByRecPathFile.Name = "button_beginByRecPathFile";
            this.button_beginByRecPathFile.Size = new System.Drawing.Size(127, 22);
            this.button_beginByRecPathFile.TabIndex = 9;
            this.button_beginByRecPathFile.Text = "启动自动修改(&B)";
            this.button_beginByRecPathFile.UseVisualStyleBackColor = true;
            this.button_beginByRecPathFile.Click += new System.EventHandler(this.button_beginByRecPathFile_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 88);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.label3);
            this.splitContainer_main.Panel1.Controls.Add(this.splitContainer_itemInfo);
            this.splitContainer_main.Panel1.Controls.Add(this.textBox_message);
            this.splitContainer_main.Panel1.Controls.Add(this.button_changeParam);
            this.splitContainer_main.Panel1.Controls.Add(this.button_saveCurrentRecord);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.button_saveToBarcodeFile);
            this.splitContainer_main.Panel2.Controls.Add(this.textBox_outputBarcodes);
            this.splitContainer_main.Panel2.Controls.Add(this.label4);
            this.splitContainer_main.Size = new System.Drawing.Size(464, 241);
            this.splitContainer_main.SplitterDistance = 314;
            this.splitContainer_main.SplitterWidth = 3;
            this.splitContainer_main.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(3, 3);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "册信息";
            // 
            // button_saveToBarcodeFile
            // 
            this.button_saveToBarcodeFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_saveToBarcodeFile.Location = new System.Drawing.Point(2, 218);
            this.button_saveToBarcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_saveToBarcodeFile.Name = "button_saveToBarcodeFile";
            this.button_saveToBarcodeFile.Size = new System.Drawing.Size(134, 22);
            this.button_saveToBarcodeFile.TabIndex = 8;
            this.button_saveToBarcodeFile.Text = "保存到文件(&E)";
            this.button_saveToBarcodeFile.UseVisualStyleBackColor = true;
            this.button_saveToBarcodeFile.Click += new System.EventHandler(this.button_saveToBarcodeFile_Click);
            // 
            // textBox_outputBarcodes
            // 
            this.textBox_outputBarcodes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputBarcodes.Location = new System.Drawing.Point(-1, 19);
            this.textBox_outputBarcodes.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_outputBarcodes.MaxLength = 0;
            this.textBox_outputBarcodes.Multiline = true;
            this.textBox_outputBarcodes.Name = "textBox_outputBarcodes";
            this.textBox_outputBarcodes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_outputBarcodes.Size = new System.Drawing.Size(153, 195);
            this.textBox_outputBarcodes.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(3, 3);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "已处理条码或路径";
            // 
            // QuickChangeEntityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 338);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.tabControl_input);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "QuickChangeEntityForm";
            this.ShowInTaskbar = false;
            this.Text = "批修改册窗";
            this.Activated += new System.EventHandler(this.QuickChangeEntityForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickChangeEntityForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QuickChangeEntityForm_FormClosed);
            this.Load += new System.EventHandler(this.QuickChangeEntityForm_Load);
            this.contextMenuStrip_loadAction.ResumeLayout(false);
            this.splitContainer_itemInfo.Panel1.ResumeLayout(false);
            this.splitContainer_itemInfo.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemInfo)).EndInit();
            this.splitContainer_itemInfo.ResumeLayout(false);
            this.tabControl_input.ResumeLayout(false);
            this.tabPage_barcodeInput.ResumeLayout(false);
            this.tabPage_barcodeInput.PerformLayout();
            this.tabPage_barcodeFile.ResumeLayout(false);
            this.tabPage_barcodeFile.PerformLayout();
            this.tabPage_recPathFile.ResumeLayout(false);
            this.tabPage_recPathFile.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel1.PerformLayout();
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_barcode;
        private System.Windows.Forms.Button button_loadBarcode;
        private EntityEditControl entityEditControl1;
        private System.Windows.Forms.SplitContainer splitContainer_itemInfo;
        private System.Windows.Forms.WebBrowser webBrowser_biblio;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_changeParam;
        private System.Windows.Forms.Button button_saveCurrentRecord;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_loadAction;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_loadOnly;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_loadAndAutoChange;
        private System.Windows.Forms.TabControl tabControl_input;
        private System.Windows.Forms.TabPage tabPage_barcodeInput;
        private System.Windows.Forms.TabPage tabPage_barcodeFile;
        private System.Windows.Forms.TextBox textBox_barcodeFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_beginByBarcodeFile;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_outputBarcodes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_saveToBarcodeFile;
        private System.Windows.Forms.Button button_file_getBarcodeFilename;
        private System.Windows.Forms.TabPage tabPage_recPathFile;
        private System.Windows.Forms.Button button_getRecPathFileName;
        private System.Windows.Forms.TextBox textBox_recPathFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_beginByRecPathFile;
    }
}