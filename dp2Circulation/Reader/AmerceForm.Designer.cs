namespace dp2Circulation
{
    partial class AmerceForm
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

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AmerceForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_readerBarcode = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.webBrowser_readerInfo = new System.Windows.Forms.WebBrowser();
            this.splitContainer_lists = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_amerced = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_amerced = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_amerced_itemBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_price = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_borrowDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_borrowPeriod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_returnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerce_returnOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerceOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerceTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_settlementOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_settlementTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_recpath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip_amerced = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_amerced_selectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_undoAmerce = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_amercedMessage = new System.Windows.Forms.ToolStripLabel();
            this.tableLayoutPanel_amercingOverdue = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip_amercing = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_amercing_selectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_submit = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modifyPriceAndComment = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_amercingMessage = new System.Windows.Forms.ToolStripLabel();
            this.listView_overdues = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_barcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_price = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_borrowDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_borrowPeriod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_borrowOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_returnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_returnOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.panel_amercing_command = new System.Windows.Forms.Panel();
            this.button_load = new System.Windows.Forms.Button();
            this.toolTip_selectAll = new System.Windows.Forms.ToolTip(this.components);
            this.button_beginFillSummary = new System.Windows.Forms.Button();
            this.checkBox_fillSummary = new System.Windows.Forms.CheckBox();
            this.panel_load = new System.Windows.Forms.Panel();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.columnHeader_amerced_itemRefID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_itemRefID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_lists)).BeginInit();
            this.splitContainer_lists.Panel1.SuspendLayout();
            this.splitContainer_lists.Panel2.SuspendLayout();
            this.splitContainer_lists.SuspendLayout();
            this.tableLayoutPanel_amerced.SuspendLayout();
            this.toolStrip_amerced.SuspendLayout();
            this.tableLayoutPanel_amercingOverdue.SuspendLayout();
            this.toolStrip_amercing.SuspendLayout();
            this.panel_load.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号(&R):";
            // 
            // textBox_readerBarcode
            // 
            this.textBox_readerBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerBarcode.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_readerBarcode.Location = new System.Drawing.Point(207, 2);
            this.textBox_readerBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_readerBarcode.Name = "textBox_readerBarcode";
            this.textBox_readerBarcode.Size = new System.Drawing.Size(257, 39);
            this.textBox_readerBarcode.TabIndex = 1;
            this.textBox_readerBarcode.Enter += new System.EventHandler(this.textBox_readerBarcode_Enter);
            this.textBox_readerBarcode.Leave += new System.EventHandler(this.textBox_readerBarcode_Leave);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 52);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.webBrowser_readerInfo);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.splitContainer_lists);
            this.splitContainer_main.Size = new System.Drawing.Size(1131, 699);
            this.splitContainer_main.SplitterDistance = 171;
            this.splitContainer_main.SplitterWidth = 14;
            this.splitContainer_main.TabIndex = 2;
            // 
            // webBrowser_readerInfo
            // 
            this.webBrowser_readerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_readerInfo.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_readerInfo.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_readerInfo.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser_readerInfo.Name = "webBrowser_readerInfo";
            this.webBrowser_readerInfo.Size = new System.Drawing.Size(1131, 171);
            this.webBrowser_readerInfo.TabIndex = 0;
            this.webBrowser_readerInfo.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_readerInfo_DocumentCompleted);
            // 
            // splitContainer_lists
            // 
            this.splitContainer_lists.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_lists.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_lists.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.splitContainer_lists.Name = "splitContainer_lists";
            this.splitContainer_lists.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_lists.Panel1
            // 
            this.splitContainer_lists.Panel1.Controls.Add(this.tableLayoutPanel_amerced);
            // 
            // splitContainer_lists.Panel2
            // 
            this.splitContainer_lists.Panel2.Controls.Add(this.tableLayoutPanel_amercingOverdue);
            this.splitContainer_lists.Size = new System.Drawing.Size(1131, 514);
            this.splitContainer_lists.SplitterDistance = 254;
            this.splitContainer_lists.SplitterWidth = 14;
            this.splitContainer_lists.TabIndex = 1;
            // 
            // tableLayoutPanel_amerced
            // 
            this.tableLayoutPanel_amerced.BackColor = System.Drawing.Color.Honeydew;
            this.tableLayoutPanel_amerced.ColumnCount = 1;
            this.tableLayoutPanel_amerced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_amerced.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel_amerced.Controls.Add(this.listView_amerced, 0, 1);
            this.tableLayoutPanel_amerced.Controls.Add(this.toolStrip_amerced, 0, 2);
            this.tableLayoutPanel_amerced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_amerced.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_amerced.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_amerced.Name = "tableLayoutPanel_amerced";
            this.tableLayoutPanel_amerced.RowCount = 3;
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel_amerced.Size = new System.Drawing.Size(1131, 254);
            this.tableLayoutPanel_amerced.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(0, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 4, 4, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 4);
            this.label2.Size = new System.Drawing.Size(110, 30);
            this.label2.TabIndex = 0;
            this.label2.Text = "已交费用:";
            // 
            // listView_amerced
            // 
            this.listView_amerced.BackColor = System.Drawing.Color.Honeydew;
            this.listView_amerced.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView_amerced.CheckBoxes = true;
            this.listView_amerced.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_amerced_itemBarcode,
            this.columnHeader_amerced_itemRefID,
            this.columnHeader_amerced_summary,
            this.columnHeader_amerced_price,
            this.columnHeader_amerced_comment,
            this.columnHeader_amerced_reason,
            this.columnHeader_amerced_borrowDate,
            this.columnHeader_amerced_borrowPeriod,
            this.columnHeader_amerced_returnDate,
            this.columnHeader_amerced_id,
            this.columnHeader_amerce_returnOperator,
            this.columnHeader_amerced_state,
            this.columnHeader_amerceOperator,
            this.columnHeader_amerceTime,
            this.columnHeader_settlementOperator,
            this.columnHeader_settlementTime,
            this.columnHeader_recpath});
            this.listView_amerced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_amerced.FullRowSelect = true;
            this.listView_amerced.HideSelection = false;
            this.listView_amerced.LargeImageList = this.imageList_itemType;
            this.listView_amerced.Location = new System.Drawing.Point(0, 34);
            this.listView_amerced.Margin = new System.Windows.Forms.Padding(0);
            this.listView_amerced.Name = "listView_amerced";
            this.listView_amerced.Size = new System.Drawing.Size(1131, 176);
            this.listView_amerced.SmallImageList = this.imageList_itemType;
            this.listView_amerced.TabIndex = 1;
            this.listView_amerced.UseCompatibleStateImageBehavior = false;
            this.listView_amerced.View = System.Windows.Forms.View.Details;
            this.listView_amerced.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_amerced_ColumnClick);
            this.listView_amerced.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_amerced_ItemChecked);
            this.listView_amerced.SelectedIndexChanged += new System.EventHandler(this.listView_amerced_SelectedIndexChanged);
            this.listView_amerced.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_amerced_MouseUp);
            // 
            // columnHeader_amerced_itemBarcode
            // 
            this.columnHeader_amerced_itemBarcode.Text = "册条码号";
            this.columnHeader_amerced_itemBarcode.Width = 150;
            // 
            // columnHeader_amerced_summary
            // 
            this.columnHeader_amerced_summary.Text = "摘要";
            this.columnHeader_amerced_summary.Width = 120;
            // 
            // columnHeader_amerced_price
            // 
            this.columnHeader_amerced_price.Text = "金额";
            this.columnHeader_amerced_price.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_amerced_price.Width = 97;
            // 
            // columnHeader_amerced_comment
            // 
            this.columnHeader_amerced_comment.Text = "注释";
            // 
            // columnHeader_amerced_reason
            // 
            this.columnHeader_amerced_reason.Text = "原因";
            this.columnHeader_amerced_reason.Width = 91;
            // 
            // columnHeader_amerced_borrowDate
            // 
            this.columnHeader_amerced_borrowDate.Text = "起点日期";
            this.columnHeader_amerced_borrowDate.Width = 100;
            // 
            // columnHeader_amerced_borrowPeriod
            // 
            this.columnHeader_amerced_borrowPeriod.Text = "期限";
            this.columnHeader_amerced_borrowPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_amerced_borrowPeriod.Width = 100;
            // 
            // columnHeader_amerced_returnDate
            // 
            this.columnHeader_amerced_returnDate.Text = "终点日期";
            this.columnHeader_amerced_returnDate.Width = 100;
            // 
            // columnHeader_amerced_id
            // 
            this.columnHeader_amerced_id.Text = "ID";
            this.columnHeader_amerced_id.Width = 200;
            // 
            // columnHeader_amerce_returnOperator
            // 
            this.columnHeader_amerce_returnOperator.Text = "终点操作者";
            this.columnHeader_amerce_returnOperator.Width = 100;
            // 
            // columnHeader_amerced_state
            // 
            this.columnHeader_amerced_state.Text = "状态";
            this.columnHeader_amerced_state.Width = 100;
            // 
            // columnHeader_amerceOperator
            // 
            this.columnHeader_amerceOperator.Text = "收费者";
            this.columnHeader_amerceOperator.Width = 100;
            // 
            // columnHeader_amerceTime
            // 
            this.columnHeader_amerceTime.Text = "收费日期";
            this.columnHeader_amerceTime.Width = 100;
            // 
            // columnHeader_settlementOperator
            // 
            this.columnHeader_settlementOperator.Text = "结算者";
            this.columnHeader_settlementOperator.Width = 100;
            // 
            // columnHeader_settlementTime
            // 
            this.columnHeader_settlementTime.Text = "结算日期";
            this.columnHeader_settlementTime.Width = 100;
            // 
            // columnHeader_recpath
            // 
            this.columnHeader_recpath.Text = "记录路径";
            this.columnHeader_recpath.Width = 100;
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.White;
            this.imageList_itemType.Images.SetKeyName(0, "amerced_type.bmp");
            this.imageList_itemType.Images.SetKeyName(1, "settlemented_type.bmp");
            this.imageList_itemType.Images.SetKeyName(2, "old_settlemented_type.bmp");
            this.imageList_itemType.Images.SetKeyName(3, "error_type.bmp");
            // 
            // toolStrip_amerced
            // 
            this.toolStrip_amerced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_amerced.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_amerced.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip_amerced.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_amerced_selectAll,
            this.toolStripButton_undoAmerce,
            this.toolStripLabel_amercedMessage});
            this.toolStrip_amerced.Location = new System.Drawing.Point(0, 210);
            this.toolStrip_amerced.Name = "toolStrip_amerced";
            this.toolStrip_amerced.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip_amerced.Size = new System.Drawing.Size(1131, 44);
            this.toolStrip_amerced.TabIndex = 3;
            this.toolStrip_amerced.Text = "toolStrip1";
            // 
            // toolStripButton_amerced_selectAll
            // 
            this.toolStripButton_amerced_selectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_amerced_selectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_amerced_selectAll.Image")));
            this.toolStripButton_amerced_selectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_amerced_selectAll.Name = "toolStripButton_amerced_selectAll";
            this.toolStripButton_amerced_selectAll.Size = new System.Drawing.Size(58, 38);
            this.toolStripButton_amerced_selectAll.Text = "全选";
            this.toolStripButton_amerced_selectAll.Click += new System.EventHandler(this.toolStripButton_amerced_selectAll_Click);
            // 
            // toolStripButton_undoAmerce
            // 
            this.toolStripButton_undoAmerce.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_undoAmerce.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_undoAmerce.Enabled = false;
            this.toolStripButton_undoAmerce.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_undoAmerce.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_undoAmerce.Image")));
            this.toolStripButton_undoAmerce.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_undoAmerce.Name = "toolStripButton_undoAmerce";
            this.toolStripButton_undoAmerce.Size = new System.Drawing.Size(123, 38);
            this.toolStripButton_undoAmerce.Text = "撤回交费";
            this.toolStripButton_undoAmerce.Click += new System.EventHandler(this.toolStripButton_undoAmerce_Click);
            // 
            // toolStripLabel_amercedMessage
            // 
            this.toolStripLabel_amercedMessage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_amercedMessage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripLabel_amercedMessage.Name = "toolStripLabel_amercedMessage";
            this.toolStripLabel_amercedMessage.Size = new System.Drawing.Size(367, 38);
            this.toolStripLabel_amercedMessage.Text = "当费用事项被勾选后，按纽才可用 -->";
            // 
            // tableLayoutPanel_amercingOverdue
            // 
            this.tableLayoutPanel_amercingOverdue.BackColor = System.Drawing.Color.LavenderBlush;
            this.tableLayoutPanel_amercingOverdue.ColumnCount = 1;
            this.tableLayoutPanel_amercingOverdue.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_amercingOverdue.Controls.Add(this.toolStrip_amercing, 0, 2);
            this.tableLayoutPanel_amercingOverdue.Controls.Add(this.listView_overdues, 0, 1);
            this.tableLayoutPanel_amercingOverdue.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel_amercingOverdue.Controls.Add(this.panel_amercing_command, 0, 2);
            this.tableLayoutPanel_amercingOverdue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_amercingOverdue.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_amercingOverdue.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_amercingOverdue.Name = "tableLayoutPanel_amercingOverdue";
            this.tableLayoutPanel_amercingOverdue.RowCount = 3;
            this.tableLayoutPanel_amercingOverdue.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amercingOverdue.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_amercingOverdue.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amercingOverdue.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amercingOverdue.Size = new System.Drawing.Size(1131, 246);
            this.tableLayoutPanel_amercingOverdue.TabIndex = 0;
            // 
            // toolStrip_amercing
            // 
            this.toolStrip_amercing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_amercing.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_amercing.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip_amercing.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_amercing_selectAll,
            this.toolStripButton_submit,
            this.toolStripButton_modifyPriceAndComment,
            this.toolStripLabel_amercingMessage});
            this.toolStrip_amercing.Location = new System.Drawing.Point(0, 192);
            this.toolStrip_amercing.Name = "toolStrip_amercing";
            this.toolStrip_amercing.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip_amercing.Size = new System.Drawing.Size(1131, 54);
            this.toolStrip_amercing.TabIndex = 5;
            this.toolStrip_amercing.Text = "toolStrip1";
            // 
            // toolStripButton_amercing_selectAll
            // 
            this.toolStripButton_amercing_selectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_amercing_selectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_amercing_selectAll.Image")));
            this.toolStripButton_amercing_selectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_amercing_selectAll.Name = "toolStripButton_amercing_selectAll";
            this.toolStripButton_amercing_selectAll.Size = new System.Drawing.Size(58, 48);
            this.toolStripButton_amercing_selectAll.Text = "全选";
            this.toolStripButton_amercing_selectAll.Click += new System.EventHandler(this.toolStripButton_amercing_selectAll_Click);
            // 
            // toolStripButton_submit
            // 
            this.toolStripButton_submit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_submit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_submit.Enabled = false;
            this.toolStripButton_submit.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_submit.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_submit.Image")));
            this.toolStripButton_submit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_submit.Name = "toolStripButton_submit";
            this.toolStripButton_submit.Size = new System.Drawing.Size(89, 48);
            this.toolStripButton_submit.Text = "交费";
            this.toolStripButton_submit.Click += new System.EventHandler(this.toolStripButton_submit_Click);
            // 
            // toolStripButton_modifyPriceAndComment
            // 
            this.toolStripButton_modifyPriceAndComment.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_modifyPriceAndComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyPriceAndComment.Enabled = false;
            this.toolStripButton_modifyPriceAndComment.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyPriceAndComment.Image")));
            this.toolStripButton_modifyPriceAndComment.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyPriceAndComment.Name = "toolStripButton_modifyPriceAndComment";
            this.toolStripButton_modifyPriceAndComment.Size = new System.Drawing.Size(172, 48);
            this.toolStripButton_modifyPriceAndComment.Text = "只修改金额/注释";
            this.toolStripButton_modifyPriceAndComment.Click += new System.EventHandler(this.toolStripButton_modifyPriceAndComment_Click);
            // 
            // toolStripLabel_amercingMessage
            // 
            this.toolStripLabel_amercingMessage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_amercingMessage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripLabel_amercingMessage.Name = "toolStripLabel_amercingMessage";
            this.toolStripLabel_amercingMessage.Size = new System.Drawing.Size(367, 48);
            this.toolStripLabel_amercingMessage.Text = "当费用事项被勾选后，按纽才可用 -->";
            // 
            // listView_overdues
            // 
            this.listView_overdues.BackColor = System.Drawing.Color.LavenderBlush;
            this.listView_overdues.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView_overdues.CheckBoxes = true;
            this.listView_overdues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_barcode,
            this.columnHeader_itemRefID,
            this.columnHeader_summary,
            this.columnHeader_price,
            this.columnHeader_comment,
            this.columnHeader_reason,
            this.columnHeader_borrowDate,
            this.columnHeader_borrowPeriod,
            this.columnHeader_borrowOperator,
            this.columnHeader_returnDate,
            this.columnHeader_returnOperator,
            this.columnHeader_id});
            this.listView_overdues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_overdues.FullRowSelect = true;
            this.listView_overdues.HideSelection = false;
            this.listView_overdues.Location = new System.Drawing.Point(0, 34);
            this.listView_overdues.Margin = new System.Windows.Forms.Padding(0);
            this.listView_overdues.Name = "listView_overdues";
            this.listView_overdues.Size = new System.Drawing.Size(1131, 158);
            this.listView_overdues.TabIndex = 1;
            this.listView_overdues.UseCompatibleStateImageBehavior = false;
            this.listView_overdues.View = System.Windows.Forms.View.Details;
            this.listView_overdues.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_overdues_ColumnClick);
            this.listView_overdues.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_overdues_ItemChecked);
            this.listView_overdues.SelectedIndexChanged += new System.EventHandler(this.listView_overdues_SelectedIndexChanged);
            this.listView_overdues.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_overdues_MouseUp);
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "册条码号";
            this.columnHeader_barcode.Width = 150;
            // 
            // columnHeader_summary
            // 
            this.columnHeader_summary.Text = "摘要";
            this.columnHeader_summary.Width = 120;
            // 
            // columnHeader_price
            // 
            this.columnHeader_price.Text = "金额";
            this.columnHeader_price.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_price.Width = 97;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            // 
            // columnHeader_reason
            // 
            this.columnHeader_reason.Text = "原因";
            this.columnHeader_reason.Width = 91;
            // 
            // columnHeader_borrowDate
            // 
            this.columnHeader_borrowDate.Text = "起点日期";
            this.columnHeader_borrowDate.Width = 100;
            // 
            // columnHeader_borrowPeriod
            // 
            this.columnHeader_borrowPeriod.Text = "期限";
            this.columnHeader_borrowPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_borrowPeriod.Width = 100;
            // 
            // columnHeader_borrowOperator
            // 
            this.columnHeader_borrowOperator.Text = "起点操作者";
            this.columnHeader_borrowOperator.Width = 100;
            // 
            // columnHeader_returnDate
            // 
            this.columnHeader_returnDate.Text = "终点日期";
            this.columnHeader_returnDate.Width = 100;
            // 
            // columnHeader_returnOperator
            // 
            this.columnHeader_returnOperator.Text = "终点操作者";
            this.columnHeader_returnOperator.Width = 100;
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "ID";
            this.columnHeader_id.Width = 200;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(0, 4);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 4, 4, 0);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 5, 0, 4);
            this.label3.Size = new System.Drawing.Size(110, 30);
            this.label3.TabIndex = 0;
            this.label3.Text = "未交费用:";
            // 
            // panel_amercing_command
            // 
            this.panel_amercing_command.AutoSize = true;
            this.panel_amercing_command.BackColor = System.Drawing.SystemColors.Control;
            this.panel_amercing_command.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_amercing_command.Location = new System.Drawing.Point(0, 192);
            this.panel_amercing_command.Margin = new System.Windows.Forms.Padding(0);
            this.panel_amercing_command.Name = "panel_amercing_command";
            this.panel_amercing_command.Size = new System.Drawing.Size(1131, 1);
            this.panel_amercing_command.TabIndex = 1;
            // 
            // button_load
            // 
            this.button_load.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_load.Location = new System.Drawing.Point(473, 0);
            this.button_load.Margin = new System.Windows.Forms.Padding(4);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(136, 38);
            this.button_load.TabIndex = 2;
            this.button_load.Text = "装载(&L)";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // toolTip_selectAll
            // 
            this.toolTip_selectAll.IsBalloon = true;
            this.toolTip_selectAll.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip_selectAll.ToolTipTitle = "操作提示";
            // 
            // button_beginFillSummary
            // 
            this.button_beginFillSummary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_beginFillSummary.Location = new System.Drawing.Point(836, 0);
            this.button_beginFillSummary.Margin = new System.Windows.Forms.Padding(4);
            this.button_beginFillSummary.Name = "button_beginFillSummary";
            this.button_beginFillSummary.Size = new System.Drawing.Size(226, 38);
            this.button_beginFillSummary.TabIndex = 4;
            this.button_beginFillSummary.Text = "填充书目摘要(&F)";
            this.button_beginFillSummary.UseVisualStyleBackColor = true;
            this.button_beginFillSummary.Visible = false;
            this.button_beginFillSummary.Click += new System.EventHandler(this.button_beginFillSummary_Click);
            // 
            // checkBox_fillSummary
            // 
            this.checkBox_fillSummary.AutoSize = true;
            this.checkBox_fillSummary.Location = new System.Drawing.Point(618, 7);
            this.checkBox_fillSummary.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_fillSummary.Name = "checkBox_fillSummary";
            this.checkBox_fillSummary.Size = new System.Drawing.Size(195, 25);
            this.checkBox_fillSummary.TabIndex = 3;
            this.checkBox_fillSummary.Text = "装载书目摘要(&S)";
            this.checkBox_fillSummary.UseVisualStyleBackColor = true;
            this.checkBox_fillSummary.CheckedChanged += new System.EventHandler(this.checkBox_fillSummary_CheckedChanged);
            // 
            // panel_load
            // 
            this.panel_load.Controls.Add(this.textBox_readerBarcode);
            this.panel_load.Controls.Add(this.button_beginFillSummary);
            this.panel_load.Controls.Add(this.label1);
            this.panel_load.Controls.Add(this.checkBox_fillSummary);
            this.panel_load.Controls.Add(this.button_load);
            this.panel_load.Location = new System.Drawing.Point(6, 5);
            this.panel_load.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_load.Name = "panel_load";
            this.panel_load.Size = new System.Drawing.Size(1109, 42);
            this.panel_load.TabIndex = 5;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.panel_load, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_main, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(1131, 751);
            this.tableLayoutPanel_main.TabIndex = 6;
            // 
            // columnHeader_amerced_itemRefID
            // 
            this.columnHeader_amerced_itemRefID.Text = "册参考ID";
            this.columnHeader_amerced_itemRefID.Width = 150;
            // 
            // columnHeader_itemRefID
            // 
            this.columnHeader_itemRefID.Text = "册参考ID";
            this.columnHeader_itemRefID.Width = 150;
            // 
            // AmerceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1131, 751);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AmerceForm";
            this.ShowInTaskbar = false;
            this.Text = "交费";
            this.Activated += new System.EventHandler(this.AmerceForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AmerceForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AmerceForm_FormClosed);
            this.Load += new System.EventHandler(this.AmerceForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_lists.Panel1.ResumeLayout(false);
            this.splitContainer_lists.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_lists)).EndInit();
            this.splitContainer_lists.ResumeLayout(false);
            this.tableLayoutPanel_amerced.ResumeLayout(false);
            this.tableLayoutPanel_amerced.PerformLayout();
            this.toolStrip_amerced.ResumeLayout(false);
            this.toolStrip_amerced.PerformLayout();
            this.tableLayoutPanel_amercingOverdue.ResumeLayout(false);
            this.tableLayoutPanel_amercingOverdue.PerformLayout();
            this.toolStrip_amercing.ResumeLayout(false);
            this.toolStrip_amercing.PerformLayout();
            this.panel_load.ResumeLayout(false);
            this.panel_load.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_readerBarcode;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Button button_load;
        private DigitalPlatform.GUI.ListViewNF listView_overdues;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_summary;
        private System.Windows.Forms.ColumnHeader columnHeader_reason;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowDate;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowPeriod;
        private System.Windows.Forms.ColumnHeader columnHeader_returnDate;
        private System.Windows.Forms.ColumnHeader columnHeader_price;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_amerced;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.GUI.ListViewNF listView_amerced;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_amercingOverdue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_itemBarcode;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_summary;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_price;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_reason;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_borrowDate;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_borrowPeriod;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_returnDate;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_id;
        private System.Windows.Forms.ColumnHeader columnHeader_amerce_returnOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_state;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_amerceOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_amerceTime;
        private System.Windows.Forms.ColumnHeader columnHeader_settlementOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_settlementTime;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.ToolTip toolTip_selectAll;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_returnOperator;
        private System.Windows.Forms.Button button_beginFillSummary;
        private System.Windows.Forms.CheckBox checkBox_fillSummary;
        private System.Windows.Forms.WebBrowser webBrowser_readerInfo;
        private System.Windows.Forms.SplitContainer splitContainer_lists;
        private System.Windows.Forms.Panel panel_load;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.ToolStrip toolStrip_amerced;
        private System.Windows.Forms.ToolStripButton toolStripButton_amerced_selectAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_undoAmerce;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_amercedMessage;
        private System.Windows.Forms.ToolStrip toolStrip_amercing;
        private System.Windows.Forms.ToolStripButton toolStripButton_amercing_selectAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_submit;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyPriceAndComment;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_amercingMessage;
        private System.Windows.Forms.Panel panel_amercing_command;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_itemRefID;
        private System.Windows.Forms.ColumnHeader columnHeader_itemRefID;
    }
}