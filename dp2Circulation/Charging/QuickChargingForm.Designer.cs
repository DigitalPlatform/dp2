namespace dp2Circulation
{
    partial class QuickChargingForm
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

            if (_taskList != null)
                _taskList.Dispose();

            if (_summaryList != null)
                _summaryList.Dispose();

            if (m_webExternalHost_readerInfo != null)
                m_webExternalHost_readerInfo.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickChargingForm));
            this.imageList_barcodeType = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip_selectFunc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_borrow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_continueBorrow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_return = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_verifyReturn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_renew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_verifyRenew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_lost = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_verifyLost = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_loadPatronInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_inventoryBook = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_boxing = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_transfer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_read = new System.Windows.Forms.ToolStripMenuItem();
            this.dpColumn_color = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_state = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_content = new DigitalPlatform.CommonControl.DpColumn();
            this.imageList_progress = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_left = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_reader = new System.Windows.Forms.WebBrowser();
            this.label_rfidMessage = new System.Windows.Forms.Label();
            this.contextMenuStrip_rfid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_rfid_restartRfidCenter = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel_right = new System.Windows.Forms.TableLayoutPanel();
            this.panel_input = new System.Windows.Forms.Panel();
            this.colorSummaryControl1 = new dp2Circulation.ColorSummaryControl();
            this.textBox_input = new System.Windows.Forms.TextBox();
            this.label_barcode_type = new System.Windows.Forms.Label();
            this.contextMenuStrip_testFunction = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_test_simulateReservationArrive = new System.Windows.Forms.ToolStripMenuItem();
            this.label_input_message = new System.Windows.Forms.Label();
            this.pictureBox_action = new System.Windows.Forms.PictureBox();
            this.dpTable_tasks = new DigitalPlatform.CommonControl.DpTable();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel_currentPatron = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_faceInput = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_openPatronSummaryWindow = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_enableHanzi = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_upperInput = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_selectItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_inventoryFromFile = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_openEasForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton_selectLibraryCode = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripButton_selectTargetLocation = new System.Windows.Forms.ToolStripButton();
            this.ToolStripMenuItem_testSync = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_selectFunc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_left.SuspendLayout();
            this.contextMenuStrip_rfid.SuspendLayout();
            this.tableLayoutPanel_right.SuspendLayout();
            this.panel_input.SuspendLayout();
            this.contextMenuStrip_testFunction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_action)).BeginInit();
            this.toolStrip_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList_barcodeType
            // 
            this.imageList_barcodeType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_barcodeType.ImageStream")));
            this.imageList_barcodeType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_barcodeType.Images.SetKeyName(0, "book_angle.bmp");
            this.imageList_barcodeType.Images.SetKeyName(1, "user.bmp");
            // 
            // contextMenuStrip_selectFunc
            // 
            this.contextMenuStrip_selectFunc.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_selectFunc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_borrow,
            this.toolStripMenuItem_continueBorrow,
            this.toolStripSeparator1,
            this.toolStripMenuItem_return,
            this.toolStripMenuItem_verifyReturn,
            this.toolStripSeparator2,
            this.toolStripMenuItem_renew,
            this.toolStripMenuItem_verifyRenew,
            this.toolStripSeparator3,
            this.toolStripMenuItem_lost,
            this.toolStripMenuItem_verifyLost,
            this.toolStripSeparator4,
            this.toolStripMenuItem_loadPatronInfo,
            this.toolStripMenuItem_inventoryBook,
            this.toolStripMenuItem_boxing,
            this.toolStripMenuItem_transfer,
            this.toolStripSeparator5,
            this.toolStripMenuItem_read});
            this.contextMenuStrip_selectFunc.Name = "contextMenuStrip_selectFunc";
            this.contextMenuStrip_selectFunc.Size = new System.Drawing.Size(211, 476);
            this.contextMenuStrip_selectFunc.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_selectFunc_Opening);
            // 
            // toolStripMenuItem_borrow
            // 
            this.toolStripMenuItem_borrow.Name = "toolStripMenuItem_borrow";
            this.toolStripMenuItem_borrow.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_borrow.Text = "借";
            this.toolStripMenuItem_borrow.Click += new System.EventHandler(this.toolStripMenuItem_borrow_Click);
            // 
            // toolStripMenuItem_continueBorrow
            // 
            this.toolStripMenuItem_continueBorrow.Name = "toolStripMenuItem_continueBorrow";
            this.toolStripMenuItem_continueBorrow.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_continueBorrow.Text = "同一读者借";
            this.toolStripMenuItem_continueBorrow.Click += new System.EventHandler(this.toolStripMenuItem_continueBorrow_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripMenuItem_return
            // 
            this.toolStripMenuItem_return.Name = "toolStripMenuItem_return";
            this.toolStripMenuItem_return.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_return.Text = "还";
            this.toolStripMenuItem_return.Click += new System.EventHandler(this.toolStripMenuItem_return_Click);
            // 
            // toolStripMenuItem_verifyReturn
            // 
            this.toolStripMenuItem_verifyReturn.Name = "toolStripMenuItem_verifyReturn";
            this.toolStripMenuItem_verifyReturn.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_verifyReturn.Text = "验证还";
            this.toolStripMenuItem_verifyReturn.Click += new System.EventHandler(this.toolStripMenuItem_verifyReturn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripMenuItem_renew
            // 
            this.toolStripMenuItem_renew.Name = "toolStripMenuItem_renew";
            this.toolStripMenuItem_renew.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_renew.Text = "续借";
            this.toolStripMenuItem_renew.Click += new System.EventHandler(this.toolStripMenuItem_renew_Click);
            // 
            // toolStripMenuItem_verifyRenew
            // 
            this.toolStripMenuItem_verifyRenew.Name = "toolStripMenuItem_verifyRenew";
            this.toolStripMenuItem_verifyRenew.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_verifyRenew.Text = "验证续借";
            this.toolStripMenuItem_verifyRenew.Click += new System.EventHandler(this.toolStripMenuItem_verifyRenew_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripMenuItem_lost
            // 
            this.toolStripMenuItem_lost.Name = "toolStripMenuItem_lost";
            this.toolStripMenuItem_lost.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_lost.Text = "丢失";
            this.toolStripMenuItem_lost.Click += new System.EventHandler(this.toolStripMenuItem_lost_Click);
            // 
            // toolStripMenuItem_verifyLost
            // 
            this.toolStripMenuItem_verifyLost.Name = "toolStripMenuItem_verifyLost";
            this.toolStripMenuItem_verifyLost.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_verifyLost.Text = "验证丢失";
            this.toolStripMenuItem_verifyLost.Click += new System.EventHandler(this.toolStripMenuItem_verifyLost_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripMenuItem_loadPatronInfo
            // 
            this.toolStripMenuItem_loadPatronInfo.Name = "toolStripMenuItem_loadPatronInfo";
            this.toolStripMenuItem_loadPatronInfo.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_loadPatronInfo.Text = "装载读者信息";
            this.toolStripMenuItem_loadPatronInfo.Click += new System.EventHandler(this.toolStripMenuItem_loadPatronInfo_Click);
            // 
            // toolStripMenuItem_inventoryBook
            // 
            this.toolStripMenuItem_inventoryBook.Name = "toolStripMenuItem_inventoryBook";
            this.toolStripMenuItem_inventoryBook.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_inventoryBook.Text = "盘点图书";
            this.toolStripMenuItem_inventoryBook.Click += new System.EventHandler(this.ToolStripMenuItem_inventoryBook_Click);
            // 
            // toolStripMenuItem_boxing
            // 
            this.toolStripMenuItem_boxing.Name = "toolStripMenuItem_boxing";
            this.toolStripMenuItem_boxing.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_boxing.Text = "配书";
            this.toolStripMenuItem_boxing.Click += new System.EventHandler(this.ToolStripMenuItem_boxing_Click);
            // 
            // toolStripMenuItem_transfer
            // 
            this.toolStripMenuItem_transfer.Name = "toolStripMenuItem_transfer";
            this.toolStripMenuItem_transfer.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_transfer.Text = "调拨";
            this.toolStripMenuItem_transfer.Click += new System.EventHandler(this.ToolStripMenuItem_move_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripMenuItem_read
            // 
            this.toolStripMenuItem_read.Name = "toolStripMenuItem_read";
            this.toolStripMenuItem_read.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_read.Text = "读过";
            this.toolStripMenuItem_read.Click += new System.EventHandler(this.ToolStripMenuItem_read_Click);
            // 
            // dpColumn_color
            // 
            this.dpColumn_color.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_color.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_color.Font = null;
            this.dpColumn_color.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_color.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_color.Width = 10;
            // 
            // dpColumn_state
            // 
            this.dpColumn_state.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_state.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_state.Font = null;
            this.dpColumn_state.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_state.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_state.Text = "状态";
            this.dpColumn_state.Width = 70;
            // 
            // dpColumn_content
            // 
            this.dpColumn_content.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_content.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_content.Font = null;
            this.dpColumn_content.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_content.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_content.Text = "内容";
            this.dpColumn_content.Width = 250;
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
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_left);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_right);
            this.splitContainer_main.Size = new System.Drawing.Size(1093, 590);
            this.splitContainer_main.SplitterDistance = 594;
            this.splitContainer_main.SplitterWidth = 15;
            this.splitContainer_main.TabIndex = 0;
            // 
            // tableLayoutPanel_left
            // 
            this.tableLayoutPanel_left.ColumnCount = 1;
            this.tableLayoutPanel_left.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_left.Controls.Add(this.webBrowser_reader, 0, 0);
            this.tableLayoutPanel_left.Controls.Add(this.label_rfidMessage, 0, 1);
            this.tableLayoutPanel_left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_left.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_left.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel_left.Name = "tableLayoutPanel_left";
            this.tableLayoutPanel_left.RowCount = 2;
            this.tableLayoutPanel_left.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_left.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_left.Size = new System.Drawing.Size(594, 590);
            this.tableLayoutPanel_left.TabIndex = 1;
            // 
            // webBrowser_reader
            // 
            this.webBrowser_reader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_reader.Location = new System.Drawing.Point(5, 5);
            this.webBrowser_reader.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.webBrowser_reader.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_reader.Name = "webBrowser_reader";
            this.webBrowser_reader.Size = new System.Drawing.Size(584, 530);
            this.webBrowser_reader.TabIndex = 0;
            this.webBrowser_reader.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_reader_DocumentCompleted);
            // 
            // label_rfidMessage
            // 
            this.label_rfidMessage.AutoSize = true;
            this.label_rfidMessage.ContextMenuStrip = this.contextMenuStrip_rfid;
            this.label_rfidMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_rfidMessage.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_rfidMessage.Location = new System.Drawing.Point(4, 540);
            this.label_rfidMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_rfidMessage.Name = "label_rfidMessage";
            this.label_rfidMessage.Size = new System.Drawing.Size(586, 50);
            this.label_rfidMessage.TabIndex = 1;
            this.label_rfidMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contextMenuStrip_rfid
            // 
            this.contextMenuStrip_rfid.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_rfid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rfid_restartRfidCenter});
            this.contextMenuStrip_rfid.Name = "contextMenuStrip_rfid";
            this.contextMenuStrip_rfid.Size = new System.Drawing.Size(228, 38);
            // 
            // ToolStripMenuItem_rfid_restartRfidCenter
            // 
            this.ToolStripMenuItem_rfid_restartRfidCenter.Name = "ToolStripMenuItem_rfid_restartRfidCenter";
            this.ToolStripMenuItem_rfid_restartRfidCenter.Size = new System.Drawing.Size(227, 34);
            this.ToolStripMenuItem_rfid_restartRfidCenter.Text = "重启 RFID 中心";
            this.ToolStripMenuItem_rfid_restartRfidCenter.Click += new System.EventHandler(this.ToolStripMenuItem_rfid_restartRfidCenter_Click);
            // 
            // tableLayoutPanel_right
            // 
            this.tableLayoutPanel_right.ColumnCount = 1;
            this.tableLayoutPanel_right.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_right.Controls.Add(this.panel_input, 0, 1);
            this.tableLayoutPanel_right.Controls.Add(this.dpTable_tasks, 0, 0);
            this.tableLayoutPanel_right.Controls.Add(this.toolStrip_main, 0, 2);
            this.tableLayoutPanel_right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_right.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_right.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.tableLayoutPanel_right.Name = "tableLayoutPanel_right";
            this.tableLayoutPanel_right.RowCount = 4;
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.Size = new System.Drawing.Size(484, 590);
            this.tableLayoutPanel_right.TabIndex = 0;
            // 
            // panel_input
            // 
            this.panel_input.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel_input.BackColor = System.Drawing.SystemColors.Window;
            this.panel_input.Controls.Add(this.colorSummaryControl1);
            this.panel_input.Controls.Add(this.textBox_input);
            this.panel_input.Controls.Add(this.label_barcode_type);
            this.panel_input.Controls.Add(this.label_input_message);
            this.panel_input.Controls.Add(this.pictureBox_action);
            this.panel_input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_input.Location = new System.Drawing.Point(5, 407);
            this.panel_input.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.panel_input.Name = "panel_input";
            this.panel_input.Size = new System.Drawing.Size(474, 143);
            this.panel_input.TabIndex = 1;
            // 
            // colorSummaryControl1
            // 
            this.colorSummaryControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorSummaryControl1.ColorList = "";
            this.colorSummaryControl1.Location = new System.Drawing.Point(0, 121);
            this.colorSummaryControl1.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.colorSummaryControl1.Name = "colorSummaryControl1";
            this.colorSummaryControl1.Size = new System.Drawing.Size(474, 17);
            this.colorSummaryControl1.TabIndex = 5;
            this.colorSummaryControl1.Click += new System.EventHandler(this.colorSummaryControl1_Click);
            // 
            // textBox_input
            // 
            this.textBox_input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_input.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_input.Font = new System.Drawing.Font("宋体", 22F);
            this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_input.Location = new System.Drawing.Point(5, 44);
            this.textBox_input.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.textBox_input.Name = "textBox_input";
            this.textBox_input.Size = new System.Drawing.Size(326, 66);
            this.textBox_input.TabIndex = 1;
            this.textBox_input.Enter += new System.EventHandler(this.textBox_input_Enter);
            this.textBox_input.Leave += new System.EventHandler(this.textBox_input_Leave);
            // 
            // label_barcode_type
            // 
            this.label_barcode_type.ContextMenuStrip = this.contextMenuStrip_testFunction;
            this.label_barcode_type.ImageIndex = 0;
            this.label_barcode_type.ImageList = this.imageList_barcodeType;
            this.label_barcode_type.Location = new System.Drawing.Point(-4, 10);
            this.label_barcode_type.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_barcode_type.Name = "label_barcode_type";
            this.label_barcode_type.Size = new System.Drawing.Size(48, 35);
            this.label_barcode_type.TabIndex = 4;
            // 
            // contextMenuStrip_testFunction
            // 
            this.contextMenuStrip_testFunction.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_testFunction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_test_simulateReservationArrive});
            this.contextMenuStrip_testFunction.Name = "contextMenuStrip_testFunction";
            this.contextMenuStrip_testFunction.Size = new System.Drawing.Size(211, 38);
            // 
            // toolStripMenuItem_test_simulateReservationArrive
            // 
            this.toolStripMenuItem_test_simulateReservationArrive.Name = "toolStripMenuItem_test_simulateReservationArrive";
            this.toolStripMenuItem_test_simulateReservationArrive.Size = new System.Drawing.Size(210, 34);
            this.toolStripMenuItem_test_simulateReservationArrive.Text = "模拟预约到书";
            this.toolStripMenuItem_test_simulateReservationArrive.Click += new System.EventHandler(this.toolStripMenuItem_test_simulateReservationArrive_Click);
            // 
            // label_input_message
            // 
            this.label_input_message.AutoSize = true;
            this.label_input_message.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_input_message.ImageIndex = 0;
            this.label_input_message.Location = new System.Drawing.Point(46, 17);
            this.label_input_message.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_input_message.Name = "label_input_message";
            this.label_input_message.Size = new System.Drawing.Size(54, 21);
            this.label_input_message.TabIndex = 3;
            this.label_input_message.Text = "test";
            this.label_input_message.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox_action
            // 
            this.pictureBox_action.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_action.ContextMenuStrip = this.contextMenuStrip_selectFunc;
            this.pictureBox_action.Location = new System.Drawing.Point(350, 0);
            this.pictureBox_action.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pictureBox_action.Name = "pictureBox_action";
            this.pictureBox_action.Size = new System.Drawing.Size(125, 117);
            this.pictureBox_action.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_action.TabIndex = 2;
            this.pictureBox_action.TabStop = false;
            this.pictureBox_action.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox_action_Paint);
            // 
            // dpTable_tasks
            // 
            this.dpTable_tasks.AutoDocCenter = true;
            this.dpTable_tasks.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable_tasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dpTable_tasks.Columns.Add(this.dpColumn_color);
            this.dpTable_tasks.Columns.Add(this.dpColumn_state);
            this.dpTable_tasks.Columns.Add(this.dpColumn_content);
            this.dpTable_tasks.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_tasks.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_tasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable_tasks.DocumentBorderColor = System.Drawing.Color.Transparent;
            this.dpTable_tasks.DocumentOrgX = ((long)(0));
            this.dpTable_tasks.DocumentOrgY = ((long)(0));
            this.dpTable_tasks.DocumentShadowColor = System.Drawing.Color.Transparent;
            this.dpTable_tasks.FocusedItem = null;
            this.dpTable_tasks.Font = new System.Drawing.Font("宋体", 10F);
            this.dpTable_tasks.FullRowSelect = true;
            this.dpTable_tasks.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_tasks.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_tasks.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_tasks.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_tasks.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_tasks.LineDistance = 10;
            this.dpTable_tasks.Location = new System.Drawing.Point(5, 5);
            this.dpTable_tasks.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.dpTable_tasks.MaxTextHeight = 200;
            this.dpTable_tasks.Name = "dpTable_tasks";
            this.dpTable_tasks.Padding = new System.Windows.Forms.Padding(15, 14, 15, 14);
            this.dpTable_tasks.Size = new System.Drawing.Size(474, 392);
            this.dpTable_tasks.TabIndex = 1;
            this.dpTable_tasks.Text = "dpTable1";
            this.dpTable_tasks.ScrollBarTouched += new DigitalPlatform.CommonControl.ScrollBarTouchedEventHandler(this.dpTable_tasks_ScrollBarTouched);
            this.dpTable_tasks.PaintRegion += new DigitalPlatform.CommonControl.PaintRegionEventHandler(this.dpTable_tasks_PaintRegion);
            this.dpTable_tasks.Click += new System.EventHandler(this.dpTable_tasks_Click);
            this.dpTable_tasks.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dpTable_tasks_MouseUp);
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_main.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel_currentPatron,
            this.toolStripButton_faceInput,
            this.toolStripButton_openPatronSummaryWindow,
            this.toolStripButton_enableHanzi,
            this.toolStripButton_upperInput,
            this.toolStripButton_selectItem,
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton_selectLibraryCode,
            this.toolStripButton_selectTargetLocation});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 555);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_main.Size = new System.Drawing.Size(484, 35);
            this.toolStrip_main.TabIndex = 2;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolStripLabel_currentPatron
            // 
            this.toolStripLabel_currentPatron.Name = "toolStripLabel_currentPatron";
            this.toolStripLabel_currentPatron.Size = new System.Drawing.Size(0, 29);
            this.toolStripLabel_currentPatron.ToolTipText = "当前读者证条码号";
            // 
            // toolStripButton_faceInput
            // 
            this.toolStripButton_faceInput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_faceInput.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_faceInput.Image")));
            this.toolStripButton_faceInput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_faceInput.Name = "toolStripButton_faceInput";
            this.toolStripButton_faceInput.Size = new System.Drawing.Size(40, 29);
            this.toolStripButton_faceInput.Text = "人脸识别";
            this.toolStripButton_faceInput.Click += new System.EventHandler(this.toolStripButton_faceInput_Click);
            // 
            // toolStripButton_openPatronSummaryWindow
            // 
            this.toolStripButton_openPatronSummaryWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_openPatronSummaryWindow.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_openPatronSummaryWindow.Image")));
            this.toolStripButton_openPatronSummaryWindow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_openPatronSummaryWindow.Name = "toolStripButton_openPatronSummaryWindow";
            this.toolStripButton_openPatronSummaryWindow.Size = new System.Drawing.Size(40, 29);
            this.toolStripButton_openPatronSummaryWindow.Text = "打开读者摘要窗口";
            this.toolStripButton_openPatronSummaryWindow.Click += new System.EventHandler(this.toolStripButton_openPatronSummaryWindow_Click);
            // 
            // toolStripButton_enableHanzi
            // 
            this.toolStripButton_enableHanzi.CheckOnClick = true;
            this.toolStripButton_enableHanzi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_enableHanzi.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_enableHanzi.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_enableHanzi.Image")));
            this.toolStripButton_enableHanzi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_enableHanzi.Name = "toolStripButton_enableHanzi";
            this.toolStripButton_enableHanzi.Size = new System.Drawing.Size(40, 29);
            this.toolStripButton_enableHanzi.Text = "英";
            this.toolStripButton_enableHanzi.ToolTipText = "是否允许输入汉字";
            this.toolStripButton_enableHanzi.CheckedChanged += new System.EventHandler(this.toolStripButton_enableHanzi_CheckedChanged);
            // 
            // toolStripButton_upperInput
            // 
            this.toolStripButton_upperInput.CheckOnClick = true;
            this.toolStripButton_upperInput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_upperInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_upperInput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_upperInput.Name = "toolStripButton_upperInput";
            this.toolStripButton_upperInput.Size = new System.Drawing.Size(40, 29);
            this.toolStripButton_upperInput.Text = "a";
            this.toolStripButton_upperInput.ToolTipText = "是否将输入自动转为大写";
            this.toolStripButton_upperInput.CheckedChanged += new System.EventHandler(this.toolStripButton_upperInput_CheckedChanged);
            // 
            // toolStripButton_selectItem
            // 
            this.toolStripButton_selectItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectItem.Image")));
            this.toolStripButton_selectItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectItem.Name = "toolStripButton_selectItem";
            this.toolStripButton_selectItem.Size = new System.Drawing.Size(79, 29);
            this.toolStripButton_selectItem.Text = "选择册";
            this.toolStripButton_selectItem.Click += new System.EventHandler(this.toolStripButton_selectItem_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_inventoryFromFile,
            this.ToolStripMenuItem_openEasForm,
            this.ToolStripMenuItem_testSync});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(48, 29);
            this.toolStripDropDownButton1.Text = "...";
            // 
            // toolStripMenuItem_inventoryFromFile
            // 
            this.toolStripMenuItem_inventoryFromFile.Name = "toolStripMenuItem_inventoryFromFile";
            this.toolStripMenuItem_inventoryFromFile.Size = new System.Drawing.Size(315, 40);
            this.toolStripMenuItem_inventoryFromFile.Text = "从文件导入盘点 ...";
            this.toolStripMenuItem_inventoryFromFile.Click += new System.EventHandler(this.ToolStripMenuItem_inventoryFromFile_Click);
            // 
            // ToolStripMenuItem_openEasForm
            // 
            this.ToolStripMenuItem_openEasForm.Name = "ToolStripMenuItem_openEasForm";
            this.ToolStripMenuItem_openEasForm.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_openEasForm.Text = "打开 EAS 窗";
            this.ToolStripMenuItem_openEasForm.Click += new System.EventHandler(this.ToolStripMenuItem_openEasForm_Click);
            // 
            // toolStripDropDownButton_selectLibraryCode
            // 
            this.toolStripDropDownButton_selectLibraryCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_selectLibraryCode.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_selectLibraryCode.Image")));
            this.toolStripDropDownButton_selectLibraryCode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_selectLibraryCode.Name = "toolStripDropDownButton_selectLibraryCode";
            this.toolStripDropDownButton_selectLibraryCode.Size = new System.Drawing.Size(117, 29);
            this.toolStripDropDownButton_selectLibraryCode.Text = "选择分馆";
            this.toolStripDropDownButton_selectLibraryCode.ToolTipText = "选择当前操作所针对的分馆";
            this.toolStripDropDownButton_selectLibraryCode.Visible = false;
            // 
            // toolStripButton_selectTargetLocation
            // 
            this.toolStripButton_selectTargetLocation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectTargetLocation.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectTargetLocation.Image")));
            this.toolStripButton_selectTargetLocation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectTargetLocation.Name = "toolStripButton_selectTargetLocation";
            this.toolStripButton_selectTargetLocation.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_selectTargetLocation.Text = "选择调拨去向";
            this.toolStripButton_selectTargetLocation.Click += new System.EventHandler(this.toolStripButton_selectTransferTargetLocation_Click);
            // 
            // ToolStripMenuItem_testSync
            // 
            this.ToolStripMenuItem_testSync.CheckOnClick = true;
            this.ToolStripMenuItem_testSync.Name = "ToolStripMenuItem_testSync";
            this.ToolStripMenuItem_testSync.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_testSync.Text = "测试同步";
            // 
            // QuickChargingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1093, 590);
            this.Controls.Add(this.splitContainer_main);
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "QuickChargingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "快捷出纳";
            this.Activated += new System.EventHandler(this.QuickChargingForm_Activated);
            this.Deactivate += new System.EventHandler(this.QuickChargingForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickChargingForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QuickChargingForm_FormClosed);
            this.Load += new System.EventHandler(this.QuickChargingForm_Load);
            this.Enter += new System.EventHandler(this.QuickChargingForm_Enter);
            this.contextMenuStrip_selectFunc.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_left.ResumeLayout(false);
            this.tableLayoutPanel_left.PerformLayout();
            this.contextMenuStrip_rfid.ResumeLayout(false);
            this.tableLayoutPanel_right.ResumeLayout(false);
            this.tableLayoutPanel_right.PerformLayout();
            this.panel_input.ResumeLayout(false);
            this.panel_input.PerformLayout();
            this.contextMenuStrip_testFunction.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_action)).EndInit();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser_reader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_right;
        private DigitalPlatform.CommonControl.DpTable dpTable_tasks;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_state;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_content;
        private System.Windows.Forms.Panel panel_input;
        private System.Windows.Forms.PictureBox pictureBox_action;
        private System.Windows.Forms.TextBox textBox_input;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_selectFunc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_borrow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_return;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_verifyRenew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_lost;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_loadPatronInfo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_verifyReturn;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_verifyLost;
        private System.Windows.Forms.Label label_input_message;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_color;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_continueBorrow;
        private System.Windows.Forms.ImageList imageList_barcodeType;
        private System.Windows.Forms.Label label_barcode_type;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private ColorSummaryControl colorSummaryControl1;
        private System.Windows.Forms.ImageList imageList_progress;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_currentPatron;
        private System.Windows.Forms.ToolStripButton toolStripButton_openPatronSummaryWindow;
        private System.Windows.Forms.ToolStripButton toolStripButton_enableHanzi;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_renew;
        private System.Windows.Forms.ToolStripButton toolStripButton_upperInput;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_inventoryBook;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_inventoryFromFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_read;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_selectLibraryCode;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_testFunction;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_test_simulateReservationArrive;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_boxing;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_transfer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectTargetLocation;
        private System.Windows.Forms.ToolStripButton toolStripButton_faceInput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_left;
        private System.Windows.Forms.Label label_rfidMessage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_openEasForm;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_rfid;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfid_restartRfidCenter;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_testSync;
    }
}