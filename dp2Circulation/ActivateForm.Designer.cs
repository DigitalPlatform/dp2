namespace dp2Circulation
{
    partial class ActivateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivateForm));
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_old = new System.Windows.Forms.Panel();
            this.toolStrip_old = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_old_save = new System.Windows.Forms.ToolStripButton();
            this.tabControl_old = new System.Windows.Forms.TabControl();
            this.tabPage_oldBasic = new System.Windows.Forms.TabPage();
            this.readerEditControl_old = new dp2Circulation.ReaderEditControl();
            this.tabPage_oldBorrowInfo = new System.Windows.Forms.TabPage();
            this.webBrowser_oldReaderInfo = new System.Windows.Forms.WebBrowser();
            this.tabPage_oldXml = new System.Windows.Forms.TabPage();
            this.webBrowser_oldXml = new System.Windows.Forms.WebBrowser();
            this.button_loadOldUserInfo = new System.Windows.Forms.Button();
            this.textBox_oldBarcode = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel_new = new System.Windows.Forms.Panel();
            this.toolStrip_new = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_new_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_new_copyFromOld = new System.Windows.Forms.ToolStripButton();
            this.tabControl_new = new System.Windows.Forms.TabControl();
            this.tabPage_newBasic = new System.Windows.Forms.TabPage();
            this.readerEditControl_new = new dp2Circulation.ReaderEditControl();
            this.tabPage_newBorrowInfo = new System.Windows.Forms.TabPage();
            this.webBrowser_newReaderInfo = new System.Windows.Forms.WebBrowser();
            this.tabPage_newXml = new System.Windows.Forms.TabPage();
            this.webBrowser_newXml = new System.Windows.Forms.WebBrowser();
            this.button_loadNewUserInfo = new System.Windows.Forms.Button();
            this.textBox_newBarcode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_activate = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.button_devolve = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.panel_old.SuspendLayout();
            this.toolStrip_old.SuspendLayout();
            this.tabControl_old.SuspendLayout();
            this.tabPage_oldBasic.SuspendLayout();
            this.tabPage_oldBorrowInfo.SuspendLayout();
            this.tabPage_oldXml.SuspendLayout();
            this.panel_new.SuspendLayout();
            this.toolStrip_new.SuspendLayout();
            this.tabControl_new.SuspendLayout();
            this.tabPage_newBasic.SuspendLayout();
            this.tabPage_newBorrowInfo.SuspendLayout();
            this.tabPage_newXml.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 10);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_old);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.panel_new);
            this.splitContainer_main.Size = new System.Drawing.Size(523, 282);
            this.splitContainer_main.SplitterDistance = 258;
            this.splitContainer_main.SplitterWidth = 3;
            this.splitContainer_main.TabIndex = 0;
            // 
            // panel_old
            // 
            this.panel_old.AllowDrop = true;
            this.panel_old.Controls.Add(this.toolStrip_old);
            this.panel_old.Controls.Add(this.tabControl_old);
            this.panel_old.Controls.Add(this.button_loadOldUserInfo);
            this.panel_old.Controls.Add(this.textBox_oldBarcode);
            this.panel_old.Controls.Add(this.label3);
            this.panel_old.Controls.Add(this.label1);
            this.panel_old.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_old.Location = new System.Drawing.Point(0, 0);
            this.panel_old.Margin = new System.Windows.Forms.Padding(2);
            this.panel_old.Name = "panel_old";
            this.panel_old.Size = new System.Drawing.Size(258, 282);
            this.panel_old.TabIndex = 0;
            this.panel_old.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel_old_DragDrop);
            this.panel_old.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel_old_DragEnter);
            // 
            // toolStrip_old
            // 
            this.toolStrip_old.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip_old.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_old.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_old.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_old_save});
            this.toolStrip_old.Location = new System.Drawing.Point(0, 257);
            this.toolStrip_old.Name = "toolStrip_old";
            this.toolStrip_old.Size = new System.Drawing.Size(26, 25);
            this.toolStrip_old.TabIndex = 6;
            this.toolStrip_old.Text = "toolStrip1";
            // 
            // toolStripButton_old_save
            // 
            this.toolStripButton_old_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_old_save.Enabled = false;
            this.toolStripButton_old_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_old_save.Image")));
            this.toolStripButton_old_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_old_save.Name = "toolStripButton_old_save";
            this.toolStripButton_old_save.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_old_save.Text = "保存源记录";
            this.toolStripButton_old_save.Click += new System.EventHandler(this.toolStripButton_old_save_Click);
            // 
            // tabControl_old
            // 
            this.tabControl_old.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_old.Controls.Add(this.tabPage_oldBasic);
            this.tabControl_old.Controls.Add(this.tabPage_oldBorrowInfo);
            this.tabControl_old.Controls.Add(this.tabPage_oldXml);
            this.tabControl_old.Location = new System.Drawing.Point(0, 47);
            this.tabControl_old.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_old.Name = "tabControl_old";
            this.tabControl_old.SelectedIndex = 0;
            this.tabControl_old.Size = new System.Drawing.Size(258, 213);
            this.tabControl_old.TabIndex = 5;
            // 
            // tabPage_oldBasic
            // 
            this.tabPage_oldBasic.Controls.Add(this.readerEditControl_old);
            this.tabPage_oldBasic.Location = new System.Drawing.Point(4, 22);
            this.tabPage_oldBasic.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_oldBasic.Name = "tabPage_oldBasic";
            this.tabPage_oldBasic.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_oldBasic.Size = new System.Drawing.Size(250, 187);
            this.tabPage_oldBasic.TabIndex = 0;
            this.tabPage_oldBasic.Text = "基本信息";
            this.tabPage_oldBasic.UseVisualStyleBackColor = true;
            // 
            // readerEditControl_old
            // 
            this.readerEditControl_old.Address = "";
            this.readerEditControl_old.BackColor = System.Drawing.Color.Lavender;
            this.readerEditControl_old.Barcode = "";
            this.readerEditControl_old.CardNumber = "";
            this.readerEditControl_old.Changed = false;
            this.readerEditControl_old.Comment = "";
            this.readerEditControl_old.CreateDate = "Sun, 03 Dec 2006 00:00:00 +0800";
            this.readerEditControl_old.DateOfBirth = "Sun, 03 Dec 2006 00:00:00 +0800";
            this.readerEditControl_old.Department = "";
            this.readerEditControl_old.Dock = System.Windows.Forms.DockStyle.Fill;
            this.readerEditControl_old.Email = "";
            this.readerEditControl_old.ExpireDate = "Sun, 03 Dec 2006 00:00:00 +0800";
            this.readerEditControl_old.Fingerprint = "";
            this.readerEditControl_old.FingerprintVersion = "";
            this.readerEditControl_old.Foregift = "";
            this.readerEditControl_old.Gender = "";
            this.readerEditControl_old.HireExpireDate = "";
            this.readerEditControl_old.HirePeriod = "";
            this.readerEditControl_old.IdCardNumber = "";
            this.readerEditControl_old.Initializing = true;
            this.readerEditControl_old.Location = new System.Drawing.Point(2, 2);
            this.readerEditControl_old.Margin = new System.Windows.Forms.Padding(2);
            this.readerEditControl_old.Name = "readerEditControl_old";
            this.readerEditControl_old.NameString = "";
            this.readerEditControl_old.Post = "";
            this.readerEditControl_old.ReaderType = "";
            this.readerEditControl_old.RecPath = "";
            this.readerEditControl_old.Size = new System.Drawing.Size(246, 183);
            this.readerEditControl_old.State = "";
            this.readerEditControl_old.TabIndex = 4;
            this.readerEditControl_old.Tel = "";
            this.readerEditControl_old.GetLibraryCode += new dp2Circulation.GetLibraryCodeEventHandler(this.readerEditControl_old_GetLibraryCode);
            this.readerEditControl_old.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(this.readerEditControl_old_ContentChanged);
            // 
            // tabPage_oldBorrowInfo
            // 
            this.tabPage_oldBorrowInfo.Controls.Add(this.webBrowser_oldReaderInfo);
            this.tabPage_oldBorrowInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_oldBorrowInfo.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_oldBorrowInfo.Name = "tabPage_oldBorrowInfo";
            this.tabPage_oldBorrowInfo.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_oldBorrowInfo.Size = new System.Drawing.Size(250, 187);
            this.tabPage_oldBorrowInfo.TabIndex = 1;
            this.tabPage_oldBorrowInfo.Text = "借阅信息";
            this.tabPage_oldBorrowInfo.UseVisualStyleBackColor = true;
            // 
            // webBrowser_oldReaderInfo
            // 
            this.webBrowser_oldReaderInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_oldReaderInfo.Location = new System.Drawing.Point(2, 2);
            this.webBrowser_oldReaderInfo.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_oldReaderInfo.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_oldReaderInfo.Name = "webBrowser_oldReaderInfo";
            this.webBrowser_oldReaderInfo.Size = new System.Drawing.Size(188, 70);
            this.webBrowser_oldReaderInfo.TabIndex = 0;
            // 
            // tabPage_oldXml
            // 
            this.tabPage_oldXml.Controls.Add(this.webBrowser_oldXml);
            this.tabPage_oldXml.Location = new System.Drawing.Point(4, 22);
            this.tabPage_oldXml.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_oldXml.Name = "tabPage_oldXml";
            this.tabPage_oldXml.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_oldXml.Size = new System.Drawing.Size(250, 187);
            this.tabPage_oldXml.TabIndex = 2;
            this.tabPage_oldXml.Text = "XML";
            this.tabPage_oldXml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_oldXml
            // 
            this.webBrowser_oldXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_oldXml.Location = new System.Drawing.Point(3, 3);
            this.webBrowser_oldXml.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_oldXml.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_oldXml.Name = "webBrowser_oldXml";
            this.webBrowser_oldXml.Size = new System.Drawing.Size(186, 68);
            this.webBrowser_oldXml.TabIndex = 0;
            // 
            // button_loadOldUserInfo
            // 
            this.button_loadOldUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadOldUserInfo.AutoSize = true;
            this.button_loadOldUserInfo.Image = ((System.Drawing.Image)(resources.GetObject("button_loadOldUserInfo.Image")));
            this.button_loadOldUserInfo.Location = new System.Drawing.Point(236, 22);
            this.button_loadOldUserInfo.Margin = new System.Windows.Forms.Padding(2);
            this.button_loadOldUserInfo.Name = "button_loadOldUserInfo";
            this.button_loadOldUserInfo.Size = new System.Drawing.Size(22, 22);
            this.button_loadOldUserInfo.TabIndex = 3;
            this.button_loadOldUserInfo.UseVisualStyleBackColor = true;
            this.button_loadOldUserInfo.Click += new System.EventHandler(this.button_loadOldUserInfo_Click);
            // 
            // textBox_oldBarcode
            // 
            this.textBox_oldBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_oldBarcode.Location = new System.Drawing.Point(65, 22);
            this.textBox_oldBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_oldBarcode.Name = "textBox_oldBarcode";
            this.textBox_oldBarcode.Size = new System.Drawing.Size(170, 21);
            this.textBox_oldBarcode.TabIndex = 2;
            this.textBox_oldBarcode.Enter += new System.EventHandler(this.textBox_oldBarcode_Enter);
            this.textBox_oldBarcode.Leave += new System.EventHandler(this.textBox_oldBarcode_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-2, 26);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "条码号(&B):";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(-2, 2);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(259, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "源证信息";
            // 
            // panel_new
            // 
            this.panel_new.AllowDrop = true;
            this.panel_new.Controls.Add(this.toolStrip_new);
            this.panel_new.Controls.Add(this.tabControl_new);
            this.panel_new.Controls.Add(this.button_loadNewUserInfo);
            this.panel_new.Controls.Add(this.textBox_newBarcode);
            this.panel_new.Controls.Add(this.label4);
            this.panel_new.Controls.Add(this.label2);
            this.panel_new.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_new.Location = new System.Drawing.Point(0, 0);
            this.panel_new.Margin = new System.Windows.Forms.Padding(2);
            this.panel_new.Name = "panel_new";
            this.panel_new.Size = new System.Drawing.Size(262, 282);
            this.panel_new.TabIndex = 0;
            this.panel_new.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel_new_DragDrop);
            this.panel_new.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel_new_DragEnter);
            // 
            // toolStrip_new
            // 
            this.toolStrip_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip_new.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_new.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_new.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_new_save,
            this.toolStripSeparator1,
            this.toolStripButton_new_copyFromOld});
            this.toolStrip_new.Location = new System.Drawing.Point(0, 257);
            this.toolStrip_new.Name = "toolStrip_new";
            this.toolStrip_new.Size = new System.Drawing.Size(55, 25);
            this.toolStrip_new.TabIndex = 7;
            this.toolStrip_new.Text = "toolStrip1";
            // 
            // toolStripButton_new_save
            // 
            this.toolStripButton_new_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_new_save.Enabled = false;
            this.toolStripButton_new_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_new_save.Image")));
            this.toolStripButton_new_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_new_save.Name = "toolStripButton_new_save";
            this.toolStripButton_new_save.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_new_save.Text = "保存目标记录";
            this.toolStripButton_new_save.Click += new System.EventHandler(this.toolStripButton_new_save_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_new_copyFromOld
            // 
            this.toolStripButton_new_copyFromOld.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_new_copyFromOld.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_new_copyFromOld.Image")));
            this.toolStripButton_new_copyFromOld.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_new_copyFromOld.Name = "toolStripButton_new_copyFromOld";
            this.toolStripButton_new_copyFromOld.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_new_copyFromOld.Text = "从源记录中复制全部字段";
            this.toolStripButton_new_copyFromOld.Click += new System.EventHandler(this.toolStripButton_new_copyFromOld_Click);
            // 
            // tabControl_new
            // 
            this.tabControl_new.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_new.Controls.Add(this.tabPage_newBasic);
            this.tabControl_new.Controls.Add(this.tabPage_newBorrowInfo);
            this.tabControl_new.Controls.Add(this.tabPage_newXml);
            this.tabControl_new.Location = new System.Drawing.Point(0, 47);
            this.tabControl_new.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_new.Name = "tabControl_new";
            this.tabControl_new.SelectedIndex = 0;
            this.tabControl_new.Size = new System.Drawing.Size(262, 213);
            this.tabControl_new.TabIndex = 6;
            // 
            // tabPage_newBasic
            // 
            this.tabPage_newBasic.Controls.Add(this.readerEditControl_new);
            this.tabPage_newBasic.Location = new System.Drawing.Point(4, 22);
            this.tabPage_newBasic.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_newBasic.Name = "tabPage_newBasic";
            this.tabPage_newBasic.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_newBasic.Size = new System.Drawing.Size(254, 187);
            this.tabPage_newBasic.TabIndex = 0;
            this.tabPage_newBasic.Text = "基本信息";
            this.tabPage_newBasic.UseVisualStyleBackColor = true;
            // 
            // readerEditControl_new
            // 
            this.readerEditControl_new.Address = "";
            this.readerEditControl_new.Barcode = "";
            this.readerEditControl_new.CardNumber = "";
            this.readerEditControl_new.Changed = false;
            this.readerEditControl_new.Comment = "";
            this.readerEditControl_new.CreateDate = "Sun, 03 Dec 2006 00:00:00 +0800";
            this.readerEditControl_new.DateOfBirth = "Sun, 03 Dec 2006 00:00:00 +0800";
            this.readerEditControl_new.Department = "";
            this.readerEditControl_new.Dock = System.Windows.Forms.DockStyle.Fill;
            this.readerEditControl_new.Email = "";
            this.readerEditControl_new.ExpireDate = "Sun, 03 Dec 2006 00:00:00 +0800";
            this.readerEditControl_new.Fingerprint = "";
            this.readerEditControl_new.FingerprintVersion = "";
            this.readerEditControl_new.Foregift = "";
            this.readerEditControl_new.Gender = "";
            this.readerEditControl_new.HireExpireDate = "";
            this.readerEditControl_new.HirePeriod = "";
            this.readerEditControl_new.IdCardNumber = "";
            this.readerEditControl_new.Initializing = true;
            this.readerEditControl_new.Location = new System.Drawing.Point(2, 2);
            this.readerEditControl_new.Margin = new System.Windows.Forms.Padding(2);
            this.readerEditControl_new.Name = "readerEditControl_new";
            this.readerEditControl_new.NameString = "";
            this.readerEditControl_new.Post = "";
            this.readerEditControl_new.ReaderType = "";
            this.readerEditControl_new.RecPath = "";
            this.readerEditControl_new.Size = new System.Drawing.Size(250, 183);
            this.readerEditControl_new.State = "";
            this.readerEditControl_new.TabIndex = 5;
            this.readerEditControl_new.Tel = "";
            this.readerEditControl_new.GetLibraryCode += new dp2Circulation.GetLibraryCodeEventHandler(this.readerEditControl_new_GetLibraryCode);
            this.readerEditControl_new.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(this.readerEditControl_new_ContentChanged);
            // 
            // tabPage_newBorrowInfo
            // 
            this.tabPage_newBorrowInfo.Controls.Add(this.webBrowser_newReaderInfo);
            this.tabPage_newBorrowInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_newBorrowInfo.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_newBorrowInfo.Name = "tabPage_newBorrowInfo";
            this.tabPage_newBorrowInfo.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_newBorrowInfo.Size = new System.Drawing.Size(254, 187);
            this.tabPage_newBorrowInfo.TabIndex = 1;
            this.tabPage_newBorrowInfo.Text = "借阅信息";
            this.tabPage_newBorrowInfo.UseVisualStyleBackColor = true;
            // 
            // webBrowser_newReaderInfo
            // 
            this.webBrowser_newReaderInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_newReaderInfo.Location = new System.Drawing.Point(2, 2);
            this.webBrowser_newReaderInfo.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_newReaderInfo.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_newReaderInfo.Name = "webBrowser_newReaderInfo";
            this.webBrowser_newReaderInfo.Size = new System.Drawing.Size(188, 70);
            this.webBrowser_newReaderInfo.TabIndex = 0;
            // 
            // tabPage_newXml
            // 
            this.tabPage_newXml.Controls.Add(this.webBrowser_newXml);
            this.tabPage_newXml.Location = new System.Drawing.Point(4, 22);
            this.tabPage_newXml.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_newXml.Name = "tabPage_newXml";
            this.tabPage_newXml.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_newXml.Size = new System.Drawing.Size(254, 187);
            this.tabPage_newXml.TabIndex = 2;
            this.tabPage_newXml.Text = "XML";
            this.tabPage_newXml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_newXml
            // 
            this.webBrowser_newXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_newXml.Location = new System.Drawing.Point(3, 3);
            this.webBrowser_newXml.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_newXml.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_newXml.Name = "webBrowser_newXml";
            this.webBrowser_newXml.Size = new System.Drawing.Size(186, 68);
            this.webBrowser_newXml.TabIndex = 0;
            // 
            // button_loadNewUserInfo
            // 
            this.button_loadNewUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadNewUserInfo.AutoSize = true;
            this.button_loadNewUserInfo.Image = ((System.Drawing.Image)(resources.GetObject("button_loadNewUserInfo.Image")));
            this.button_loadNewUserInfo.Location = new System.Drawing.Point(240, 24);
            this.button_loadNewUserInfo.Margin = new System.Windows.Forms.Padding(2);
            this.button_loadNewUserInfo.Name = "button_loadNewUserInfo";
            this.button_loadNewUserInfo.Size = new System.Drawing.Size(22, 22);
            this.button_loadNewUserInfo.TabIndex = 4;
            this.button_loadNewUserInfo.UseVisualStyleBackColor = true;
            this.button_loadNewUserInfo.Click += new System.EventHandler(this.button_loadNewUserInfo_Click);
            // 
            // textBox_newBarcode
            // 
            this.textBox_newBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_newBarcode.Location = new System.Drawing.Point(65, 24);
            this.textBox_newBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_newBarcode.Name = "textBox_newBarcode";
            this.textBox_newBarcode.Size = new System.Drawing.Size(174, 21);
            this.textBox_newBarcode.TabIndex = 3;
            this.textBox_newBarcode.Enter += new System.EventHandler(this.textBox_newBarcode_Enter);
            this.textBox_newBarcode.Leave += new System.EventHandler(this.textBox_newBarcode_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2, 28);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "条码号(&B):";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(-2, 2);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "目标证信息";
            // 
            // button_activate
            // 
            this.button_activate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_activate.Location = new System.Drawing.Point(388, 298);
            this.button_activate.Margin = new System.Windows.Forms.Padding(2);
            this.button_activate.Name = "button_activate";
            this.button_activate.Size = new System.Drawing.Size(134, 22);
            this.button_activate.TabIndex = 1;
            this.button_activate.Text = "转移并激活目标证(&A)";
            this.button_activate.UseVisualStyleBackColor = true;
            this.button_activate.Click += new System.EventHandler(this.button_activate_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(281, 162);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "基本信息";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(281, 162);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "借阅信息";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(281, 162);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "基本信息";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 24);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(281, 162);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "借阅信息";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // button_devolve
            // 
            this.button_devolve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_devolve.Location = new System.Drawing.Point(311, 298);
            this.button_devolve.Margin = new System.Windows.Forms.Padding(2);
            this.button_devolve.Name = "button_devolve";
            this.button_devolve.Size = new System.Drawing.Size(73, 22);
            this.button_devolve.TabIndex = 2;
            this.button_devolve.Text = "转移(&V)";
            this.button_devolve.UseVisualStyleBackColor = true;
            this.button_devolve.Click += new System.EventHandler(this.button_devolve_Click);
            // 
            // ActivateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 330);
            this.Controls.Add(this.button_devolve);
            this.Controls.Add(this.button_activate);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ActivateForm";
            this.ShowInTaskbar = false;
            this.Text = "激活窗";
            this.Activated += new System.EventHandler(this.ActivateForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ActivateForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ActivateForm_FormClosed);
            this.Load += new System.EventHandler(this.ActivateForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.panel_old.ResumeLayout(false);
            this.panel_old.PerformLayout();
            this.toolStrip_old.ResumeLayout(false);
            this.toolStrip_old.PerformLayout();
            this.tabControl_old.ResumeLayout(false);
            this.tabPage_oldBasic.ResumeLayout(false);
            this.tabPage_oldBorrowInfo.ResumeLayout(false);
            this.tabPage_oldXml.ResumeLayout(false);
            this.panel_new.ResumeLayout(false);
            this.panel_new.PerformLayout();
            this.toolStrip_new.ResumeLayout(false);
            this.toolStrip_new.PerformLayout();
            this.tabControl_new.ResumeLayout(false);
            this.tabPage_newBasic.ResumeLayout(false);
            this.tabPage_newBorrowInfo.ResumeLayout(false);
            this.tabPage_newXml.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Panel panel_old;
        private System.Windows.Forms.Panel panel_new;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_oldBarcode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_newBarcode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_loadOldUserInfo;
        private System.Windows.Forms.Button button_loadNewUserInfo;
        private ReaderEditControl readerEditControl_old;
        private ReaderEditControl readerEditControl_new;
        private System.Windows.Forms.Button button_activate;
        private System.Windows.Forms.TabControl tabControl_old;
        private System.Windows.Forms.TabPage tabPage_oldBasic;
        private System.Windows.Forms.TabPage tabPage_oldBorrowInfo;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabControl tabControl_new;
        private System.Windows.Forms.TabPage tabPage_newBasic;
        private System.Windows.Forms.TabPage tabPage_newBorrowInfo;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.WebBrowser webBrowser_oldReaderInfo;
        private System.Windows.Forms.WebBrowser webBrowser_newReaderInfo;
        private System.Windows.Forms.Button button_devolve;
        private System.Windows.Forms.TabPage tabPage_oldXml;
        private System.Windows.Forms.TabPage tabPage_newXml;
        private System.Windows.Forms.WebBrowser webBrowser_oldXml;
        private System.Windows.Forms.WebBrowser webBrowser_newXml;
        private System.Windows.Forms.ToolStrip toolStrip_old;
        private System.Windows.Forms.ToolStripButton toolStripButton_old_save;
        private System.Windows.Forms.ToolStrip toolStrip_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_new_save;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_new_copyFromOld;
    }
}