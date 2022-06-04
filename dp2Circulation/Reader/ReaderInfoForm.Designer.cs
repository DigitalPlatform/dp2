namespace dp2Circulation
{
    partial class ReaderInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderInfoForm));
            this.splitContainer_normal = new System.Windows.Forms.SplitContainer();
            this.readerEditControl1 = new dp2Circulation.ReaderEditControl();
            this.webBrowser_readerInfo = new System.Windows.Forms.WebBrowser();
            this.tabControl_readerInfo = new System.Windows.Forms.TabControl();
            this.tabPage_normal = new System.Windows.Forms.TabPage();
            this.tabPage_borrowHistory = new System.Windows.Forms.TabPage();
            this.webBrowser_borrowHistory = new System.Windows.Forms.WebBrowser();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.webBrowser_xml = new System.Windows.Forms.WebBrowser();
            this.tabPage_objects = new System.Windows.Forms.TabPage();
            this.binaryResControl1 = new DigitalPlatform.CirculationClient.BinaryResControl();
            this.tabPage_qrCode = new System.Windows.Forms.TabPage();
            this.textBox_pqr = new System.Windows.Forms.TextBox();
            this.pictureBox_qrCode = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_loadFromIdcard = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_loadBlank = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_loadBlank = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_loadBlankFromLocal = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_loadBlankFromServer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_webCamera = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButton_registerFace = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItem_registerFaceByFile = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_pasteCardPhoto = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_pasteCardPhoto = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButton_registerFingerprint = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolStripMenuItem_fingerprintPracticeMode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSplitButton_registerPalmprint = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItem_clearPalmprint1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_registerFingerprint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_createMoneyRecord = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_hire = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_foregift = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_returnForegift = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_saveTo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButton_save = new System.Windows.Forms.ToolStripSplitButton();
            this.ToolStripMenuItem_saveChangeBarcode = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_saveChangeState = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_saveForce = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_next = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prev = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_stopSummaryLoop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_addFriends = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clearOutofReservationCount = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_otherFunc = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripButton_saveTemplate = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenuItem_loadBlankRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_notifyOverdue = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_createRfidCard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_bindCardNumber = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_editXML = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_exportDetailToExcelFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_exportExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_exportBorrowingBarcode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_moveRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_clearFaceFeature = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_clearFingerprint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_clearPalmprint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_option = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip_load = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox_barcode = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton_load = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.toolStripMenuItem_notifyRecall = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_normal)).BeginInit();
            this.splitContainer_normal.Panel1.SuspendLayout();
            this.splitContainer_normal.Panel2.SuspendLayout();
            this.splitContainer_normal.SuspendLayout();
            this.tabControl_readerInfo.SuspendLayout();
            this.tabPage_normal.SuspendLayout();
            this.tabPage_borrowHistory.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.tabPage_objects.SuspendLayout();
            this.tabPage_qrCode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_qrCode)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.toolStrip_load.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_normal
            // 
            this.splitContainer_normal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_normal.Location = new System.Drawing.Point(4, 3);
            this.splitContainer_normal.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer_normal.Name = "splitContainer_normal";
            // 
            // splitContainer_normal.Panel1
            // 
            this.splitContainer_normal.Panel1.Controls.Add(this.readerEditControl1);
            // 
            // splitContainer_normal.Panel2
            // 
            this.splitContainer_normal.Panel2.Controls.Add(this.webBrowser_readerInfo);
            this.splitContainer_normal.Size = new System.Drawing.Size(1094, 377);
            this.splitContainer_normal.SplitterDistance = 585;
            this.splitContainer_normal.SplitterWidth = 5;
            this.splitContainer_normal.TabIndex = 5;
            // 
            // readerEditControl1
            // 
            this.readerEditControl1.Access = "";
            this.readerEditControl1.Address = "";
            this.readerEditControl1.Barcode = "";
            this.readerEditControl1.CardNumber = "";
            this.readerEditControl1.Changed = false;
            this.readerEditControl1.Comment = "";
            this.readerEditControl1.CreateDate = "";
            this.readerEditControl1.CreateState = dp2Circulation.ItemDisplayState.Normal;
            this.readerEditControl1.DateOfBirth = "";
            this.readerEditControl1.Department = "";
            this.readerEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.readerEditControl1.Email = "";
            this.readerEditControl1.ExpireDate = "";
            this.readerEditControl1.FaceFeature = "";
            this.readerEditControl1.FaceFeatureVersion = "";
            this.readerEditControl1.FingerprintFeature = "";
            this.readerEditControl1.FingerprintFeatureVersion = "";
            this.readerEditControl1.Foregift = "";
            this.readerEditControl1.Friends = "";
            this.readerEditControl1.Gender = "";
            this.readerEditControl1.HireExpireDate = "";
            this.readerEditControl1.HirePeriod = "";
            this.readerEditControl1.IdCardNumber = "";
            this.readerEditControl1.Initializing = true;
            this.readerEditControl1.Location = new System.Drawing.Point(0, 0);
            this.readerEditControl1.Margin = new System.Windows.Forms.Padding(5);
            this.readerEditControl1.Name = "readerEditControl1";
            this.readerEditControl1.NamePinyin = "";
            this.readerEditControl1.NameString = "";
            this.readerEditControl1.PalmprintFeature = "";
            this.readerEditControl1.PalmprintFeatureVersion = "";
            this.readerEditControl1.ParentId = "";
            this.readerEditControl1.PersonalLibrary = "";
            this.readerEditControl1.Post = "";
            this.readerEditControl1.ReaderType = "";
            this.readerEditControl1.RecPath = "";
            this.readerEditControl1.RefID = "";
            this.readerEditControl1.Rights = "";
            this.readerEditControl1.Size = new System.Drawing.Size(585, 377);
            this.readerEditControl1.State = "";
            this.readerEditControl1.TabIndex = 0;
            this.readerEditControl1.Tel = "";
            this.readerEditControl1.GetLibraryCode += new dp2Circulation.GetLibraryCodeEventHandler(this.readerEditControl1_GetLibraryCode);
            this.readerEditControl1.CreatePinyin += new System.EventHandler(this.readerEditControl1_CreatePinyin);
            this.readerEditControl1.EditRights += new System.EventHandler(this.readerEditControl1_EditRights);
            // 
            // webBrowser_readerInfo
            // 
            this.webBrowser_readerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_readerInfo.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_readerInfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.webBrowser_readerInfo.MinimumSize = new System.Drawing.Size(27, 28);
            this.webBrowser_readerInfo.Name = "webBrowser_readerInfo";
            this.webBrowser_readerInfo.Size = new System.Drawing.Size(504, 377);
            this.webBrowser_readerInfo.TabIndex = 0;
            this.webBrowser_readerInfo.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_readerInfo_DocumentCompleted);
            // 
            // tabControl_readerInfo
            // 
            this.tabControl_readerInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_readerInfo.Controls.Add(this.tabPage_normal);
            this.tabControl_readerInfo.Controls.Add(this.tabPage_borrowHistory);
            this.tabControl_readerInfo.Controls.Add(this.tabPage_xml);
            this.tabControl_readerInfo.Controls.Add(this.tabPage_objects);
            this.tabControl_readerInfo.Controls.Add(this.tabPage_qrCode);
            this.tabControl_readerInfo.Location = new System.Drawing.Point(4, 41);
            this.tabControl_readerInfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl_readerInfo.Name = "tabControl_readerInfo";
            this.tabControl_readerInfo.SelectedIndex = 0;
            this.tabControl_readerInfo.Size = new System.Drawing.Size(1110, 418);
            this.tabControl_readerInfo.TabIndex = 0;
            this.tabControl_readerInfo.SelectedIndexChanged += new System.EventHandler(this.tabControl_readerInfo_SelectedIndexChanged);
            // 
            // tabPage_normal
            // 
            this.tabPage_normal.Controls.Add(this.splitContainer_normal);
            this.tabPage_normal.Location = new System.Drawing.Point(4, 31);
            this.tabPage_normal.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_normal.Name = "tabPage_normal";
            this.tabPage_normal.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_normal.Size = new System.Drawing.Size(1102, 383);
            this.tabPage_normal.TabIndex = 0;
            this.tabPage_normal.Text = "常规";
            this.tabPage_normal.UseVisualStyleBackColor = true;
            // 
            // tabPage_borrowHistory
            // 
            this.tabPage_borrowHistory.Controls.Add(this.webBrowser_borrowHistory);
            this.tabPage_borrowHistory.Location = new System.Drawing.Point(4, 31);
            this.tabPage_borrowHistory.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_borrowHistory.Name = "tabPage_borrowHistory";
            this.tabPage_borrowHistory.Size = new System.Drawing.Size(1102, 383);
            this.tabPage_borrowHistory.TabIndex = 3;
            this.tabPage_borrowHistory.Text = "借阅历史";
            this.tabPage_borrowHistory.UseVisualStyleBackColor = true;
            // 
            // webBrowser_borrowHistory
            // 
            this.webBrowser_borrowHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_borrowHistory.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_borrowHistory.Margin = new System.Windows.Forms.Padding(5);
            this.webBrowser_borrowHistory.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser_borrowHistory.Name = "webBrowser_borrowHistory";
            this.webBrowser_borrowHistory.Size = new System.Drawing.Size(1102, 383);
            this.webBrowser_borrowHistory.TabIndex = 0;
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.Controls.Add(this.webBrowser_xml);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 31);
            this.tabPage_xml.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_xml.Size = new System.Drawing.Size(1102, 383);
            this.tabPage_xml.TabIndex = 1;
            this.tabPage_xml.Text = "XML";
            this.tabPage_xml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_xml
            // 
            this.webBrowser_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_xml.Location = new System.Drawing.Point(4, 3);
            this.webBrowser_xml.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.webBrowser_xml.MinimumSize = new System.Drawing.Size(27, 28);
            this.webBrowser_xml.Name = "webBrowser_xml";
            this.webBrowser_xml.Size = new System.Drawing.Size(1094, 377);
            this.webBrowser_xml.TabIndex = 0;
            // 
            // tabPage_objects
            // 
            this.tabPage_objects.Controls.Add(this.binaryResControl1);
            this.tabPage_objects.Location = new System.Drawing.Point(4, 31);
            this.tabPage_objects.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_objects.Name = "tabPage_objects";
            this.tabPage_objects.Size = new System.Drawing.Size(1102, 383);
            this.tabPage_objects.TabIndex = 2;
            this.tabPage_objects.Text = "对象";
            this.tabPage_objects.UseVisualStyleBackColor = true;
            // 
            // binaryResControl1
            // 
            this.binaryResControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.binaryResControl1.BiblioRecPath = "";
            this.binaryResControl1.Changed = false;
            this.binaryResControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryResControl1.ErrorInfo = "";
            this.binaryResControl1.Location = new System.Drawing.Point(0, 0);
            this.binaryResControl1.Margin = new System.Windows.Forms.Padding(5);
            this.binaryResControl1.Name = "binaryResControl1";
            this.binaryResControl1.RightsCfgFileName = null;
            this.binaryResControl1.Size = new System.Drawing.Size(1102, 383);
            this.binaryResControl1.TabIndex = 1;
            this.binaryResControl1.TempDir = null;
            // 
            // tabPage_qrCode
            // 
            this.tabPage_qrCode.Controls.Add(this.textBox_pqr);
            this.tabPage_qrCode.Controls.Add(this.pictureBox_qrCode);
            this.tabPage_qrCode.Location = new System.Drawing.Point(4, 31);
            this.tabPage_qrCode.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_qrCode.Name = "tabPage_qrCode";
            this.tabPage_qrCode.Size = new System.Drawing.Size(1102, 383);
            this.tabPage_qrCode.TabIndex = 4;
            this.tabPage_qrCode.Text = "二维码";
            this.tabPage_qrCode.UseVisualStyleBackColor = true;
            // 
            // textBox_pqr
            // 
            this.textBox_pqr.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox_pqr.Location = new System.Drawing.Point(0, 0);
            this.textBox_pqr.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_pqr.Name = "textBox_pqr";
            this.textBox_pqr.ReadOnly = true;
            this.textBox_pqr.Size = new System.Drawing.Size(1102, 31);
            this.textBox_pqr.TabIndex = 0;
            // 
            // pictureBox_qrCode
            // 
            this.pictureBox_qrCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_qrCode.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_qrCode.Margin = new System.Windows.Forms.Padding(5);
            this.pictureBox_qrCode.Name = "pictureBox_qrCode";
            this.pictureBox_qrCode.Padding = new System.Windows.Forms.Padding(37, 35, 37, 35);
            this.pictureBox_qrCode.Size = new System.Drawing.Size(1102, 383);
            this.pictureBox_qrCode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_qrCode.TabIndex = 0;
            this.pictureBox_qrCode.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_delete,
            this.toolStripButton_loadFromIdcard,
            this.toolStripButton_loadBlank,
            this.toolStripDropDownButton_loadBlank,
            this.toolStripButton_webCamera,
            this.toolStripSplitButton_registerFace,
            this.toolStripButton_pasteCardPhoto,
            this.toolStripSplitButton_registerFingerprint,
            this.toolStripSplitButton_registerPalmprint,
            this.toolStripButton_registerFingerprint,
            this.toolStripSeparator1,
            this.toolStripButton_createMoneyRecord,
            this.toolStripSeparator3,
            this.toolStripButton_saveTo,
            this.toolStripSplitButton_save,
            this.toolStripButton_next,
            this.toolStripButton_prev,
            this.toolStripSeparator4,
            this.toolStripButton_stopSummaryLoop,
            this.toolStripSeparator5,
            this.toolStripButton_addFriends,
            this.toolStripButton_clearOutofReservationCount,
            this.toolStripDropDownButton_otherFunc,
            this.toolStripSeparator2});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, 462);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1118, 38);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_delete
            // 
            this.toolStripButton_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_delete.Image")));
            this.toolStripButton_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_delete.Name = "toolStripButton_delete";
            this.toolStripButton_delete.Size = new System.Drawing.Size(82, 32);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // toolStripButton_loadFromIdcard
            // 
            this.toolStripButton_loadFromIdcard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_loadFromIdcard.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loadFromIdcard.Image")));
            this.toolStripButton_loadFromIdcard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loadFromIdcard.Name = "toolStripButton_loadFromIdcard";
            this.toolStripButton_loadFromIdcard.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_loadFromIdcard.Text = "从身份证导入信息";
            this.toolStripButton_loadFromIdcard.ToolTipText = "从身份证导入信息 (按住 Ctrl 键不清空原有内容)";
            this.toolStripButton_loadFromIdcard.Click += new System.EventHandler(this.toolStripButton_loadFromIdcard_Click);
            // 
            // toolStripButton_loadBlank
            // 
            this.toolStripButton_loadBlank.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_loadBlank.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loadBlank.Image")));
            this.toolStripButton_loadBlank.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loadBlank.Name = "toolStripButton_loadBlank";
            this.toolStripButton_loadBlank.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_loadBlank.Text = "从本地装入空白记录";
            this.toolStripButton_loadBlank.Click += new System.EventHandler(this.toolStripButton_loadBlank_Click);
            // 
            // toolStripDropDownButton_loadBlank
            // 
            this.toolStripDropDownButton_loadBlank.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.toolStripDropDownButton_loadBlank.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_loadBlankFromLocal,
            this.ToolStripMenuItem_loadBlankFromServer});
            this.toolStripDropDownButton_loadBlank.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_loadBlank.Image")));
            this.toolStripDropDownButton_loadBlank.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_loadBlank.Name = "toolStripDropDownButton_loadBlank";
            this.toolStripDropDownButton_loadBlank.Size = new System.Drawing.Size(21, 32);
            this.toolStripDropDownButton_loadBlank.Text = "装入空白记录";
            // 
            // toolStripMenuItem_loadBlankFromLocal
            // 
            this.toolStripMenuItem_loadBlankFromLocal.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem_loadBlankFromLocal.Image")));
            this.toolStripMenuItem_loadBlankFromLocal.Name = "toolStripMenuItem_loadBlankFromLocal";
            this.toolStripMenuItem_loadBlankFromLocal.Size = new System.Drawing.Size(339, 40);
            this.toolStripMenuItem_loadBlankFromLocal.Text = "从本地装入空白记录";
            this.toolStripMenuItem_loadBlankFromLocal.ToolTipText = "从本地装入空白记录";
            this.toolStripMenuItem_loadBlankFromLocal.Click += new System.EventHandler(this.toolStripMenuItem_loadBlankFromLocal_Click);
            // 
            // ToolStripMenuItem_loadBlankFromServer
            // 
            this.ToolStripMenuItem_loadBlankFromServer.Image = ((System.Drawing.Image)(resources.GetObject("ToolStripMenuItem_loadBlankFromServer.Image")));
            this.ToolStripMenuItem_loadBlankFromServer.Name = "ToolStripMenuItem_loadBlankFromServer";
            this.ToolStripMenuItem_loadBlankFromServer.Size = new System.Drawing.Size(339, 40);
            this.ToolStripMenuItem_loadBlankFromServer.Text = "从服务器装入空白记录";
            this.ToolStripMenuItem_loadBlankFromServer.ToolTipText = "从服务器装入空白记录";
            this.ToolStripMenuItem_loadBlankFromServer.Click += new System.EventHandler(this.ToolStripMenuItem_loadBlankFromServer_Click);
            // 
            // toolStripButton_webCamera
            // 
            this.toolStripButton_webCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_webCamera.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_webCamera.Image")));
            this.toolStripButton_webCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_webCamera.Name = "toolStripButton_webCamera";
            this.toolStripButton_webCamera.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_webCamera.Text = "从摄像头获取图像";
            this.toolStripButton_webCamera.Click += new System.EventHandler(this.toolStripButton_webCamera_Click);
            // 
            // toolStripSplitButton_registerFace
            // 
            this.toolStripSplitButton_registerFace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton_registerFace.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_registerFaceByFile,
            this.ToolStripMenuItem_pasteCardPhoto});
            this.toolStripSplitButton_registerFace.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_registerFace.Image")));
            this.toolStripSplitButton_registerFace.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_registerFace.Name = "toolStripSplitButton_registerFace";
            this.toolStripSplitButton_registerFace.Size = new System.Drawing.Size(48, 32);
            this.toolStripSplitButton_registerFace.Text = "登记人脸(用于人脸识别)";
            this.toolStripSplitButton_registerFace.ButtonClick += new System.EventHandler(this.toolStripSplitButton_registerFace_ButtonClick);
            // 
            // toolStripMenuItem_registerFaceByFile
            // 
            this.toolStripMenuItem_registerFaceByFile.Name = "toolStripMenuItem_registerFaceByFile";
            this.toolStripMenuItem_registerFaceByFile.Size = new System.Drawing.Size(535, 40);
            this.toolStripMenuItem_registerFaceByFile.Text = "用图像文件注册人脸(&R) ...";
            this.toolStripMenuItem_registerFaceByFile.Click += new System.EventHandler(this.toolStripMenuItem_registerFaceByFile_Click);
            // 
            // ToolStripMenuItem_pasteCardPhoto
            // 
            this.ToolStripMenuItem_pasteCardPhoto.Name = "ToolStripMenuItem_pasteCardPhoto";
            this.ToolStripMenuItem_pasteCardPhoto.Size = new System.Drawing.Size(535, 40);
            this.ToolStripMenuItem_pasteCardPhoto.Text = "(从剪贴板)粘贴证件照(注：和人脸识别无关)";
            this.ToolStripMenuItem_pasteCardPhoto.Click += new System.EventHandler(this.ToolStripMenuItem_pasteCardPhoto_Click);
            // 
            // toolStripButton_pasteCardPhoto
            // 
            this.toolStripButton_pasteCardPhoto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_pasteCardPhoto.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_pasteCardPhoto.Image")));
            this.toolStripButton_pasteCardPhoto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_pasteCardPhoto.Name = "toolStripButton_pasteCardPhoto";
            this.toolStripButton_pasteCardPhoto.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_pasteCardPhoto.Text = "(从剪贴板)粘贴证件照(1)";
            this.toolStripButton_pasteCardPhoto.Visible = false;
            this.toolStripButton_pasteCardPhoto.Click += new System.EventHandler(this.toolStripButton_pasteCardPhoto_Click);
            // 
            // toolStripSplitButton_registerFingerprint
            // 
            this.toolStripSplitButton_registerFingerprint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton_registerFingerprint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_fingerprintPracticeMode});
            this.toolStripSplitButton_registerFingerprint.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_registerFingerprint.Image")));
            this.toolStripSplitButton_registerFingerprint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_registerFingerprint.Name = "toolStripSplitButton_registerFingerprint";
            this.toolStripSplitButton_registerFingerprint.Size = new System.Drawing.Size(48, 32);
            this.toolStripSplitButton_registerFingerprint.Text = "登记指纹";
            this.toolStripSplitButton_registerFingerprint.ButtonClick += new System.EventHandler(this.toolStripSplitButton_registerFingerprint_ButtonClick);
            // 
            // ToolStripMenuItem_fingerprintPracticeMode
            // 
            this.ToolStripMenuItem_fingerprintPracticeMode.Name = "ToolStripMenuItem_fingerprintPracticeMode";
            this.ToolStripMenuItem_fingerprintPracticeMode.Size = new System.Drawing.Size(276, 40);
            this.ToolStripMenuItem_fingerprintPracticeMode.Text = "指纹练习模式 ...";
            this.ToolStripMenuItem_fingerprintPracticeMode.Click += new System.EventHandler(this.ToolStripMenuItem_fingerprintPracticeMode_Click);
            // 
            // toolStripSplitButton_registerPalmprint
            // 
            this.toolStripSplitButton_registerPalmprint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton_registerPalmprint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_clearPalmprint1});
            this.toolStripSplitButton_registerPalmprint.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_registerPalmprint.Image")));
            this.toolStripSplitButton_registerPalmprint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_registerPalmprint.Name = "toolStripSplitButton_registerPalmprint";
            this.toolStripSplitButton_registerPalmprint.Size = new System.Drawing.Size(48, 32);
            this.toolStripSplitButton_registerPalmprint.Text = "登记掌纹";
            this.toolStripSplitButton_registerPalmprint.ButtonClick += new System.EventHandler(this.toolStripSplitButton_registerPalmprint_ButtonClick);
            // 
            // toolStripMenuItem_clearPalmprint1
            // 
            this.toolStripMenuItem_clearPalmprint1.Name = "toolStripMenuItem_clearPalmprint1";
            this.toolStripMenuItem_clearPalmprint1.Size = new System.Drawing.Size(255, 40);
            this.toolStripMenuItem_clearPalmprint1.Text = "清除掌纹特征";
            this.toolStripMenuItem_clearPalmprint1.Click += new System.EventHandler(this.toolStripMenuItem_clearPalmprint_Click);
            // 
            // toolStripButton_registerFingerprint
            // 
            this.toolStripButton_registerFingerprint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_registerFingerprint.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_registerFingerprint.Image")));
            this.toolStripButton_registerFingerprint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_registerFingerprint.Name = "toolStripButton_registerFingerprint";
            this.toolStripButton_registerFingerprint.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_registerFingerprint.Text = "登记指纹";
            this.toolStripButton_registerFingerprint.Visible = false;
            this.toolStripButton_registerFingerprint.Click += new System.EventHandler(this.toolStripButton_registerFingerprint_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_createMoneyRecord
            // 
            this.toolStripButton_createMoneyRecord.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_hire,
            this.ToolStripMenuItem_foregift,
            this.ToolStripMenuItem_returnForegift});
            this.toolStripButton_createMoneyRecord.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_createMoneyRecord.Image")));
            this.toolStripButton_createMoneyRecord.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_createMoneyRecord.Name = "toolStripButton_createMoneyRecord";
            this.toolStripButton_createMoneyRecord.Size = new System.Drawing.Size(183, 32);
            this.toolStripButton_createMoneyRecord.Text = "创建交费请求";
            // 
            // ToolStripMenuItem_hire
            // 
            this.ToolStripMenuItem_hire.Name = "ToolStripMenuItem_hire";
            this.ToolStripMenuItem_hire.Size = new System.Drawing.Size(192, 40);
            this.ToolStripMenuItem_hire.Text = "交租金";
            this.ToolStripMenuItem_hire.Click += new System.EventHandler(this.ToolStripMenuItem_hire_Click);
            // 
            // ToolStripMenuItem_foregift
            // 
            this.ToolStripMenuItem_foregift.Name = "ToolStripMenuItem_foregift";
            this.ToolStripMenuItem_foregift.Size = new System.Drawing.Size(192, 40);
            this.ToolStripMenuItem_foregift.Text = "交押金";
            this.ToolStripMenuItem_foregift.Click += new System.EventHandler(this.ToolStripMenuItem_foregift_Click);
            // 
            // ToolStripMenuItem_returnForegift
            // 
            this.ToolStripMenuItem_returnForegift.Name = "ToolStripMenuItem_returnForegift";
            this.ToolStripMenuItem_returnForegift.Size = new System.Drawing.Size(192, 40);
            this.ToolStripMenuItem_returnForegift.Text = "退押金";
            this.ToolStripMenuItem_returnForegift.Click += new System.EventHandler(this.ToolStripMenuItem_returnForegift_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_saveTo
            // 
            this.toolStripButton_saveTo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_saveTo.Image")));
            this.toolStripButton_saveTo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_saveTo.Name = "toolStripButton_saveTo";
            this.toolStripButton_saveTo.Size = new System.Drawing.Size(82, 32);
            this.toolStripButton_saveTo.Text = "新增";
            this.toolStripButton_saveTo.ToolTipText = "将记录保存为一条新的记录";
            this.toolStripButton_saveTo.Click += new System.EventHandler(this.toolStripButton_saveTo_Click);
            // 
            // toolStripSplitButton_save
            // 
            this.toolStripSplitButton_save.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_saveChangeBarcode,
            this.ToolStripMenuItem_saveChangeState,
            this.ToolStripMenuItem_saveForce});
            this.toolStripSplitButton_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton_save.Image")));
            this.toolStripSplitButton_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton_save.Name = "toolStripSplitButton_save";
            this.toolStripSplitButton_save.Size = new System.Drawing.Size(102, 32);
            this.toolStripSplitButton_save.Text = "保存";
            this.toolStripSplitButton_save.ButtonClick += new System.EventHandler(this.toolStripSplitButton_save_ButtonClick);
            // 
            // ToolStripMenuItem_saveChangeBarcode
            // 
            this.ToolStripMenuItem_saveChangeBarcode.Name = "ToolStripMenuItem_saveChangeBarcode";
            this.ToolStripMenuItem_saveChangeBarcode.Size = new System.Drawing.Size(353, 40);
            this.ToolStripMenuItem_saveChangeBarcode.Text = "保存(强制修改证条码号)";
            this.ToolStripMenuItem_saveChangeBarcode.Click += new System.EventHandler(this.ToolStripMenuItem_saveChangeBarcode_Click);
            // 
            // ToolStripMenuItem_saveChangeState
            // 
            this.ToolStripMenuItem_saveChangeState.Name = "ToolStripMenuItem_saveChangeState";
            this.ToolStripMenuItem_saveChangeState.Size = new System.Drawing.Size(353, 40);
            this.ToolStripMenuItem_saveChangeState.Text = "保存(修改状态)";
            this.ToolStripMenuItem_saveChangeState.Click += new System.EventHandler(this.ToolStripMenuItem_saveChangeState_Click);
            // 
            // ToolStripMenuItem_saveForce
            // 
            this.ToolStripMenuItem_saveForce.Name = "ToolStripMenuItem_saveForce";
            this.ToolStripMenuItem_saveForce.Size = new System.Drawing.Size(353, 40);
            this.ToolStripMenuItem_saveForce.Text = "保存(强制修改)";
            this.ToolStripMenuItem_saveForce.Click += new System.EventHandler(this.ToolStripMenuItem_saveForce_Click);
            // 
            // toolStripButton_next
            // 
            this.toolStripButton_next.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_next.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_next.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_next.Image")));
            this.toolStripButton_next.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_next.Name = "toolStripButton_next";
            this.toolStripButton_next.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_next.Text = "后一记录";
            this.toolStripButton_next.Click += new System.EventHandler(this.toolStripButton_next_Click);
            // 
            // toolStripButton_prev
            // 
            this.toolStripButton_prev.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_prev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_prev.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prev.Image")));
            this.toolStripButton_prev.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_prev.Name = "toolStripButton_prev";
            this.toolStripButton_prev.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_prev.Text = "前一记录";
            this.toolStripButton_prev.Click += new System.EventHandler(this.toolStripButton_prev_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_stopSummaryLoop
            // 
            this.toolStripButton_stopSummaryLoop.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_stopSummaryLoop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_stopSummaryLoop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stopSummaryLoop.Image")));
            this.toolStripButton_stopSummaryLoop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_stopSummaryLoop.Name = "toolStripButton_stopSummaryLoop";
            this.toolStripButton_stopSummaryLoop.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_stopSummaryLoop.Text = "停止装载书目摘要";
            this.toolStripButton_stopSummaryLoop.Click += new System.EventHandler(this.toolStripButton_stopSummaryLoop_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_addFriends
            // 
            this.toolStripButton_addFriends.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_addFriends.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_addFriends.Image")));
            this.toolStripButton_addFriends.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_addFriends.Name = "toolStripButton_addFriends";
            this.toolStripButton_addFriends.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_addFriends.Text = "加好友";
            this.toolStripButton_addFriends.Click += new System.EventHandler(this.toolStripButton_addFriends_Click);
            // 
            // toolStripButton_clearOutofReservationCount
            // 
            this.toolStripButton_clearOutofReservationCount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_clearOutofReservationCount.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearOutofReservationCount.Image")));
            this.toolStripButton_clearOutofReservationCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearOutofReservationCount.Name = "toolStripButton_clearOutofReservationCount";
            this.toolStripButton_clearOutofReservationCount.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_clearOutofReservationCount.Text = "清除预约到书后未取次数";
            this.toolStripButton_clearOutofReservationCount.Click += new System.EventHandler(this.toolStripButton_clearOutofReservationCount_Click);
            // 
            // toolStripDropDownButton_otherFunc
            // 
            this.toolStripDropDownButton_otherFunc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_otherFunc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_saveTemplate,
            this.toolStripMenuItem_loadBlankRecord,
            this.toolStripMenuItem_notifyRecall,
            this.toolStripMenuItem_notifyOverdue,
            this.toolStripSeparator8,
            this.toolStripMenuItem_createRfidCard,
            this.toolStripMenuItem_bindCardNumber,
            this.toolStripMenuItem_editXML,
            this.toolStripMenuItem_exportDetailToExcelFile,
            this.toolStripMenuItem_exportExcel,
            this.ToolStripMenuItem_exportBorrowingBarcode,
            this.toolStripSeparator7,
            this.toolStripMenuItem_moveRecord,
            this.toolStripMenuItem_clearFaceFeature,
            this.toolStripMenuItem_clearFingerprint,
            this.toolStripMenuItem_clearPalmprint,
            this.toolStripSeparator6,
            this.toolStripButton_option});
            this.toolStripDropDownButton_otherFunc.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_otherFunc.Image")));
            this.toolStripDropDownButton_otherFunc.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_otherFunc.Name = "toolStripDropDownButton_otherFunc";
            this.toolStripDropDownButton_otherFunc.Size = new System.Drawing.Size(48, 32);
            this.toolStripDropDownButton_otherFunc.Text = "...";
            this.toolStripDropDownButton_otherFunc.ToolTipText = "更多命令...";
            // 
            // toolStripButton_saveTemplate
            // 
            this.toolStripButton_saveTemplate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_saveTemplate.Image")));
            this.toolStripButton_saveTemplate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_saveTemplate.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_saveTemplate.Name = "toolStripButton_saveTemplate";
            this.toolStripButton_saveTemplate.Size = new System.Drawing.Size(227, 32);
            this.toolStripButton_saveTemplate.Text = "保存读者记录到模板";
            this.toolStripButton_saveTemplate.Click += new System.EventHandler(this.toolStripButton_saveTemplate_Click);
            // 
            // toolStripMenuItem_loadBlankRecord
            // 
            this.toolStripMenuItem_loadBlankRecord.Name = "toolStripMenuItem_loadBlankRecord";
            this.toolStripMenuItem_loadBlankRecord.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_loadBlankRecord.Text = "装载空白记录";
            this.toolStripMenuItem_loadBlankRecord.Visible = false;
            // 
            // toolStripMenuItem_notifyOverdue
            // 
            this.toolStripMenuItem_notifyOverdue.Name = "toolStripMenuItem_notifyOverdue";
            this.toolStripMenuItem_notifyOverdue.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_notifyOverdue.Text = "立即发出超期通知";
            this.toolStripMenuItem_notifyOverdue.Click += new System.EventHandler(this.toolStripMenuItem_notifyOverdue_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(440, 6);
            // 
            // toolStripMenuItem_createRfidCard
            // 
            this.toolStripMenuItem_createRfidCard.Name = "toolStripMenuItem_createRfidCard";
            this.toolStripMenuItem_createRfidCard.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_createRfidCard.Text = "创建 RFID 读者卡 (ISO15693) ...";
            this.toolStripMenuItem_createRfidCard.Click += new System.EventHandler(this.toolStripMenuItem_createRfidCard_Click);
            // 
            // toolStripMenuItem_bindCardNumber
            // 
            this.toolStripMenuItem_bindCardNumber.Name = "toolStripMenuItem_bindCardNumber";
            this.toolStripMenuItem_bindCardNumber.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_bindCardNumber.Text = "绑定卡号 ...";
            this.toolStripMenuItem_bindCardNumber.Click += new System.EventHandler(this.toolStripMenuItem_bindCardNumber_Click);
            // 
            // toolStripMenuItem_editXML
            // 
            this.toolStripMenuItem_editXML.Name = "toolStripMenuItem_editXML";
            this.toolStripMenuItem_editXML.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_editXML.Text = "编辑读者记录 XML ...";
            this.toolStripMenuItem_editXML.Click += new System.EventHandler(this.toolStripMenuItem_editXML_Click);
            // 
            // toolStripMenuItem_exportDetailToExcelFile
            // 
            this.toolStripMenuItem_exportDetailToExcelFile.Name = "toolStripMenuItem_exportDetailToExcelFile";
            this.toolStripMenuItem_exportDetailToExcelFile.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_exportDetailToExcelFile.Text = "导出读者信息到 Excel 文件(&E)...";
            this.toolStripMenuItem_exportDetailToExcelFile.Click += new System.EventHandler(this.toolStripMenuItem_exportDetailToExcelFile_Click);
            // 
            // toolStripMenuItem_exportExcel
            // 
            this.toolStripMenuItem_exportExcel.Name = "toolStripMenuItem_exportExcel";
            this.toolStripMenuItem_exportExcel.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_exportExcel.Text = "导出到 Excel 文件(&X)...";
            this.toolStripMenuItem_exportExcel.Visible = false;
            this.toolStripMenuItem_exportExcel.Click += new System.EventHandler(this.toolStripMenuItem_exportExcel_Click);
            // 
            // ToolStripMenuItem_exportBorrowingBarcode
            // 
            this.ToolStripMenuItem_exportBorrowingBarcode.Name = "ToolStripMenuItem_exportBorrowingBarcode";
            this.ToolStripMenuItem_exportBorrowingBarcode.Size = new System.Drawing.Size(443, 40);
            this.ToolStripMenuItem_exportBorrowingBarcode.Text = "导出在借册条码号到文本文件(&E)...";
            this.ToolStripMenuItem_exportBorrowingBarcode.Click += new System.EventHandler(this.ToolStripMenuItem_exportBorrowingBarcode_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(440, 6);
            // 
            // toolStripMenuItem_moveRecord
            // 
            this.toolStripMenuItem_moveRecord.Name = "toolStripMenuItem_moveRecord";
            this.toolStripMenuItem_moveRecord.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_moveRecord.Text = "移动读者记录(&M)";
            this.toolStripMenuItem_moveRecord.ToolTipText = "在读者库之间移动记录";
            this.toolStripMenuItem_moveRecord.Click += new System.EventHandler(this.toolStripMenuItem_moveRecord_Click);
            // 
            // toolStripMenuItem_clearFaceFeature
            // 
            this.toolStripMenuItem_clearFaceFeature.Name = "toolStripMenuItem_clearFaceFeature";
            this.toolStripMenuItem_clearFaceFeature.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_clearFaceFeature.Text = "清除人脸特征和图片";
            this.toolStripMenuItem_clearFaceFeature.Click += new System.EventHandler(this.toolStripMenuItem_clearFaceFeature_Click);
            // 
            // toolStripMenuItem_clearFingerprint
            // 
            this.toolStripMenuItem_clearFingerprint.Name = "toolStripMenuItem_clearFingerprint";
            this.toolStripMenuItem_clearFingerprint.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_clearFingerprint.Text = "清除指纹特征";
            this.toolStripMenuItem_clearFingerprint.Click += new System.EventHandler(this.toolStripMenuItem_clearFingerprint_Click);
            // 
            // toolStripMenuItem_clearPalmprint
            // 
            this.toolStripMenuItem_clearPalmprint.Name = "toolStripMenuItem_clearPalmprint";
            this.toolStripMenuItem_clearPalmprint.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_clearPalmprint.Text = "清除掌纹特征";
            this.toolStripMenuItem_clearPalmprint.Click += new System.EventHandler(this.toolStripMenuItem_clearPalmprint_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(440, 6);
            // 
            // toolStripButton_option
            // 
            this.toolStripButton_option.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_option.Image")));
            this.toolStripButton_option.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_option.Name = "toolStripButton_option";
            this.toolStripButton_option.Size = new System.Drawing.Size(82, 32);
            this.toolStripButton_option.Text = "选项";
            this.toolStripButton_option.Click += new System.EventHandler(this.toolStripButton_option_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStrip_load
            // 
            this.toolStrip_load.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_load.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_load.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_load.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripTextBox_barcode,
            this.toolStripButton_load});
            this.toolStrip_load.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_load.Name = "toolStrip_load";
            this.toolStrip_load.Size = new System.Drawing.Size(439, 38);
            this.toolStrip_load.TabIndex = 5;
            this.toolStrip_load.Text = "toolStrip2";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(101, 32);
            this.toolStripLabel1.Text = "证条码号:";
            // 
            // toolStripTextBox_barcode
            // 
            this.toolStripTextBox_barcode.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.toolStripTextBox_barcode.Name = "toolStripTextBox_barcode";
            this.toolStripTextBox_barcode.Size = new System.Drawing.Size(272, 38);
            this.toolStripTextBox_barcode.Enter += new System.EventHandler(this.toolStripTextBox_barcode_Enter);
            this.toolStripTextBox_barcode.Leave += new System.EventHandler(this.toolStripTextBox_barcode_Leave);
            this.toolStripTextBox_barcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripTextBox_barcode_KeyDown);
            this.toolStripTextBox_barcode.TextChanged += new System.EventHandler(this.toolStripTextBox_barcode_TextChanged);
            // 
            // toolStripButton_load
            // 
            this.toolStripButton_load.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_load.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_load.Image")));
            this.toolStripButton_load.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_load.Name = "toolStripButton_load";
            this.toolStripButton_load.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_load.Text = "装载";
            this.toolStripButton_load.Click += new System.EventHandler(this.toolStripButton_load_Click);
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.toolStrip_load, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.toolStrip1, 0, 2);
            this.tableLayoutPanel_main.Controls.Add(this.tabControl_readerInfo, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 3;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(1118, 500);
            this.tableLayoutPanel_main.TabIndex = 6;
            // 
            // toolStripMenuItem_notifyRecall
            // 
            this.toolStripMenuItem_notifyRecall.Name = "toolStripMenuItem_notifyRecall";
            this.toolStripMenuItem_notifyRecall.Size = new System.Drawing.Size(443, 40);
            this.toolStripMenuItem_notifyRecall.Text = "立即发出召回通知 ...";
            this.toolStripMenuItem_notifyRecall.Click += new System.EventHandler(this.toolStripMenuItem_notifyRecall_Click);
            // 
            // ReaderInfoForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 500);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ReaderInfoForm";
            this.ShowInTaskbar = false;
            this.Text = "读者";
            this.Activated += new System.EventHandler(this.ReaderInfoForm_Activated);
            this.Deactivate += new System.EventHandler(this.ReaderInfoForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReaderInfoForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReaderInfoForm_FormClosed);
            this.Load += new System.EventHandler(this.ReaderInfoForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ReaderInfoForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ReaderInfoForm_DragEnter);
            this.Enter += new System.EventHandler(this.ReaderInfoForm_Enter);
            this.Leave += new System.EventHandler(this.ReaderInfoForm_Leave);
            this.splitContainer_normal.Panel1.ResumeLayout(false);
            this.splitContainer_normal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_normal)).EndInit();
            this.splitContainer_normal.ResumeLayout(false);
            this.tabControl_readerInfo.ResumeLayout(false);
            this.tabPage_normal.ResumeLayout(false);
            this.tabPage_borrowHistory.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.tabPage_objects.ResumeLayout(false);
            this.tabPage_qrCode.ResumeLayout(false);
            this.tabPage_qrCode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_qrCode)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip_load.ResumeLayout(false);
            this.toolStrip_load.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_normal;
        private ReaderEditControl readerEditControl1;
        private System.Windows.Forms.WebBrowser webBrowser_readerInfo;
        private System.Windows.Forms.TabControl tabControl_readerInfo;
        private System.Windows.Forms.TabPage tabPage_normal;
        private System.Windows.Forms.TabPage tabPage_xml;
        private System.Windows.Forms.WebBrowser webBrowser_xml;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_saveTo;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_prev;
        private System.Windows.Forms.ToolStripButton toolStripButton_next;
        private System.Windows.Forms.ToolStripButton toolStripButton_stopSummaryLoop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton_createMoneyRecord;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_hire;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_foregift;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_returnForegift;
        private System.Windows.Forms.ToolStrip toolStrip_load;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_barcode;
        private System.Windows.Forms.ToolStripButton toolStripButton_load;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearOutofReservationCount;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_otherFunc;
        private System.Windows.Forms.ToolStripButton toolStripButton_saveTemplate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton toolStripButton_option;
        private System.Windows.Forms.TabPage tabPage_objects;
        private DigitalPlatform.CirculationClient.BinaryResControl binaryResControl1;
        private System.Windows.Forms.ToolStripButton toolStripButton_pasteCardPhoto;
        private System.Windows.Forms.ToolStripButton toolStripButton_webCamera;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_loadBlank;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_loadBlankFromServer;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_loadBlankFromLocal;
        private System.Windows.Forms.ToolStripButton toolStripButton_loadFromIdcard;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_moveRecord;
        private System.Windows.Forms.ToolStripButton toolStripButton_registerFingerprint;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_clearFingerprint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exportBorrowingBarcode;
        private System.Windows.Forms.ToolStripButton toolStripButton_loadBlank;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_exportExcel;
        private System.Windows.Forms.ToolStripButton toolStripButton_addFriends;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_exportDetailToExcelFile;
        private System.Windows.Forms.TabPage tabPage_borrowHistory;
        private System.Windows.Forms.WebBrowser webBrowser_borrowHistory;
        private System.Windows.Forms.TabPage tabPage_qrCode;
        private System.Windows.Forms.PictureBox pictureBox_qrCode;
        private System.Windows.Forms.TextBox textBox_pqr;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_editXML;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_registerFace;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_pasteCardPhoto;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_createRfidCard;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_bindCardNumber;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_clearFaceFeature;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_save;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_saveChangeBarcode;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_saveForce;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_registerFingerprint;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_fingerprintPracticeMode;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_saveChangeState;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton_registerPalmprint;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_clearPalmprint;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_clearPalmprint1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_loadBlankRecord;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_registerFaceByFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_notifyOverdue;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_notifyRecall;
    }
}