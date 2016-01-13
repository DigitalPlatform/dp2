namespace dp2Circulation
{
    partial class ChargingForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChargingForm));
            this.contextMenuStrip_selectFunc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_borrow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_return = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_verifyReturn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_renew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_verifyRenew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_lost = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_readerInfo = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_reader = new System.Windows.Forms.WebBrowser();
            this.textBox_readerInfo = new System.Windows.Forms.TextBox();
            this.toolStrip_navigate = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripDropDownButton_itemBarcodeNavigate = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_openEntityForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_openItemInfoForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton_readerBarcodeNavigate = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_naviToAmerceForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_naviToReaderInfoForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_naviToActivateForm_old = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_openReaderManageForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_naviToActivateForm_new = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel_biblioAndItem = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_operation = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_readerBarcode = new System.Windows.Forms.TextBox();
            this.textBox_readerPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_verifyReaderPassword = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_verifyReaderPassword = new System.Windows.Forms.Button();
            this.textBox_itemBarcode = new System.Windows.Forms.TextBox();
            this.button_loadReader = new System.Windows.Forms.Button();
            this.button_itemAction = new System.Windows.Forms.Button();
            this.splitContainer_biblioAndItem = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_biblioInfo = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_biblioInfo = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel_itemInfo = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_item = new System.Windows.Forms.WebBrowser();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_itemInfo = new System.Windows.Forms.TextBox();
            this.contextMenuStrip_verifyReaderPassword = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_verifyReaderPassword = new System.Windows.Forms.ToolStripMenuItem();
            this.panel_main = new System.Windows.Forms.Panel();
            this.contextMenuStrip_selectFunc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_readerInfo.SuspendLayout();
            this.toolStrip_navigate.SuspendLayout();
            this.tableLayoutPanel_biblioAndItem.SuspendLayout();
            this.tableLayoutPanel_operation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_biblioAndItem)).BeginInit();
            this.splitContainer_biblioAndItem.Panel1.SuspendLayout();
            this.splitContainer_biblioAndItem.Panel2.SuspendLayout();
            this.splitContainer_biblioAndItem.SuspendLayout();
            this.tableLayoutPanel_biblioInfo.SuspendLayout();
            this.tableLayoutPanel_itemInfo.SuspendLayout();
            this.contextMenuStrip_verifyReaderPassword.SuspendLayout();
            this.panel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip_selectFunc
            // 
            this.contextMenuStrip_selectFunc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_borrow,
            this.toolStripMenuItem_return,
            this.toolStripMenuItem_verifyReturn,
            this.toolStripMenuItem_renew,
            this.toolStripMenuItem_verifyRenew,
            this.toolStripMenuItem_lost});
            this.contextMenuStrip_selectFunc.Name = "contextMenuStrip_selectFunc";
            this.contextMenuStrip_selectFunc.Size = new System.Drawing.Size(125, 136);
            // 
            // toolStripMenuItem_borrow
            // 
            this.toolStripMenuItem_borrow.Name = "toolStripMenuItem_borrow";
            this.toolStripMenuItem_borrow.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_borrow.Text = "借";
            this.toolStripMenuItem_borrow.Click += new System.EventHandler(this.toolStripMenuItem_borrow_Click);
            // 
            // toolStripMenuItem_return
            // 
            this.toolStripMenuItem_return.Name = "toolStripMenuItem_return";
            this.toolStripMenuItem_return.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_return.Text = "还";
            this.toolStripMenuItem_return.Click += new System.EventHandler(this.toolStripMenuItem_return_Click);
            // 
            // toolStripMenuItem_verifyReturn
            // 
            this.toolStripMenuItem_verifyReturn.Name = "toolStripMenuItem_verifyReturn";
            this.toolStripMenuItem_verifyReturn.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_verifyReturn.Text = "验证还";
            this.toolStripMenuItem_verifyReturn.Click += new System.EventHandler(this.toolStripMenuItem_verifyReturn_Click);
            // 
            // toolStripMenuItem_renew
            // 
            this.toolStripMenuItem_renew.Name = "toolStripMenuItem_renew";
            this.toolStripMenuItem_renew.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_renew.Text = "续借";
            this.toolStripMenuItem_renew.Click += new System.EventHandler(this.toolStripMenuItem_renew_Click);
            // 
            // toolStripMenuItem_verifyRenew
            // 
            this.toolStripMenuItem_verifyRenew.Name = "toolStripMenuItem_verifyRenew";
            this.toolStripMenuItem_verifyRenew.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_verifyRenew.Text = "验证续借";
            this.toolStripMenuItem_verifyRenew.Click += new System.EventHandler(this.toolStripMenuItem_verifyRenew_Click);
            // 
            // toolStripMenuItem_lost
            // 
            this.toolStripMenuItem_lost.Name = "toolStripMenuItem_lost";
            this.toolStripMenuItem_lost.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_lost.Text = "丢失";
            this.toolStripMenuItem_lost.Click += new System.EventHandler(this.toolStripMenuItem_lost_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 8);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_readerInfo);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_biblioAndItem);
            this.splitContainer_main.Size = new System.Drawing.Size(442, 380);
            this.splitContainer_main.SplitterDistance = 211;
            this.splitContainer_main.TabIndex = 6;
            // 
            // tableLayoutPanel_readerInfo
            // 
            this.tableLayoutPanel_readerInfo.ColumnCount = 1;
            this.tableLayoutPanel_readerInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_readerInfo.Controls.Add(this.webBrowser_reader, 0, 1);
            this.tableLayoutPanel_readerInfo.Controls.Add(this.textBox_readerInfo, 0, 2);
            this.tableLayoutPanel_readerInfo.Controls.Add(this.toolStrip_navigate, 0, 0);
            this.tableLayoutPanel_readerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_readerInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_readerInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_readerInfo.Name = "tableLayoutPanel_readerInfo";
            this.tableLayoutPanel_readerInfo.RowCount = 6;
            this.tableLayoutPanel_readerInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_readerInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_readerInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_readerInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_readerInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_readerInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
            this.tableLayoutPanel_readerInfo.Size = new System.Drawing.Size(211, 380);
            this.tableLayoutPanel_readerInfo.TabIndex = 1;
            // 
            // webBrowser_reader
            // 
            this.webBrowser_reader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_reader.Location = new System.Drawing.Point(0, 25);
            this.webBrowser_reader.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_reader.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_reader.Name = "webBrowser_reader";
            this.webBrowser_reader.Size = new System.Drawing.Size(211, 150);
            this.webBrowser_reader.TabIndex = 0;
            this.webBrowser_reader.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_reader_DocumentCompleted);
            // 
            // textBox_readerInfo
            // 
            this.textBox_readerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_readerInfo.Location = new System.Drawing.Point(2, 177);
            this.textBox_readerInfo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_readerInfo.MaxLength = 0;
            this.textBox_readerInfo.Multiline = true;
            this.textBox_readerInfo.Name = "textBox_readerInfo";
            this.textBox_readerInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_readerInfo.Size = new System.Drawing.Size(207, 64);
            this.textBox_readerInfo.TabIndex = 1;
            // 
            // toolStrip_navigate
            // 
            this.toolStrip_navigate.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_navigate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripDropDownButton_itemBarcodeNavigate,
            this.toolStripDropDownButton_readerBarcodeNavigate});
            this.toolStrip_navigate.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_navigate.Name = "toolStrip_navigate";
            this.toolStrip_navigate.Size = new System.Drawing.Size(211, 25);
            this.toolStrip_navigate.TabIndex = 2;
            this.toolStrip_navigate.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(57, 22);
            this.toolStripLabel1.Text = "读者信息";
            // 
            // toolStripDropDownButton_itemBarcodeNavigate
            // 
            this.toolStripDropDownButton_itemBarcodeNavigate.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton_itemBarcodeNavigate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_openEntityForm,
            this.toolStripMenuItem_openItemInfoForm});
            this.toolStripDropDownButton_itemBarcodeNavigate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_itemBarcodeNavigate.Image")));
            this.toolStripDropDownButton_itemBarcodeNavigate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_itemBarcodeNavigate.Name = "toolStripDropDownButton_itemBarcodeNavigate";
            this.toolStripDropDownButton_itemBarcodeNavigate.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton_itemBarcodeNavigate.ToolTipText = "册条码号快速导航";
            // 
            // toolStripMenuItem_openEntityForm
            // 
            this.toolStripMenuItem_openEntityForm.Enabled = false;
            this.toolStripMenuItem_openEntityForm.Name = "toolStripMenuItem_openEntityForm";
            this.toolStripMenuItem_openEntityForm.Size = new System.Drawing.Size(112, 22);
            this.toolStripMenuItem_openEntityForm.Text = "种册窗";
            this.toolStripMenuItem_openEntityForm.Click += new System.EventHandler(this.toolStripMenuItem_openEntityForm_Click);
            // 
            // toolStripMenuItem_openItemInfoForm
            // 
            this.toolStripMenuItem_openItemInfoForm.Enabled = false;
            this.toolStripMenuItem_openItemInfoForm.Name = "toolStripMenuItem_openItemInfoForm";
            this.toolStripMenuItem_openItemInfoForm.Size = new System.Drawing.Size(112, 22);
            this.toolStripMenuItem_openItemInfoForm.Text = "实体窗";
            this.toolStripMenuItem_openItemInfoForm.Click += new System.EventHandler(this.toolStripMenuItem_openItemInfoForm_Click);
            // 
            // toolStripDropDownButton_readerBarcodeNavigate
            // 
            this.toolStripDropDownButton_readerBarcodeNavigate.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton_readerBarcodeNavigate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_naviToAmerceForm,
            this.toolStripMenuItem_naviToReaderInfoForm,
            this.toolStripMenuItem_naviToActivateForm_old,
            this.toolStripMenuItem_openReaderManageForm,
            this.toolStripMenuItem_naviToActivateForm_new});
            this.toolStripDropDownButton_readerBarcodeNavigate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_readerBarcodeNavigate.Image")));
            this.toolStripDropDownButton_readerBarcodeNavigate.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripDropDownButton_readerBarcodeNavigate.Name = "toolStripDropDownButton_readerBarcodeNavigate";
            this.toolStripDropDownButton_readerBarcodeNavigate.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton_readerBarcodeNavigate.ToolTipText = "读者证条码号快速导航";
            // 
            // toolStripMenuItem_naviToAmerceForm
            // 
            this.toolStripMenuItem_naviToAmerceForm.Enabled = false;
            this.toolStripMenuItem_naviToAmerceForm.Name = "toolStripMenuItem_naviToAmerceForm";
            this.toolStripMenuItem_naviToAmerceForm.Size = new System.Drawing.Size(144, 22);
            this.toolStripMenuItem_naviToAmerceForm.Text = "交费窗";
            this.toolStripMenuItem_naviToAmerceForm.Click += new System.EventHandler(this.toolStripMenuItem_naviToAmerceForm_Click);
            // 
            // toolStripMenuItem_naviToReaderInfoForm
            // 
            this.toolStripMenuItem_naviToReaderInfoForm.Enabled = false;
            this.toolStripMenuItem_naviToReaderInfoForm.Name = "toolStripMenuItem_naviToReaderInfoForm";
            this.toolStripMenuItem_naviToReaderInfoForm.Size = new System.Drawing.Size(144, 22);
            this.toolStripMenuItem_naviToReaderInfoForm.Text = "读者窗";
            this.toolStripMenuItem_naviToReaderInfoForm.Click += new System.EventHandler(this.toolStripMenuItem_naviToReaderInfoForm_Click);
            // 
            // toolStripMenuItem_naviToActivateForm_old
            // 
            this.toolStripMenuItem_naviToActivateForm_old.Enabled = false;
            this.toolStripMenuItem_naviToActivateForm_old.Name = "toolStripMenuItem_naviToActivateForm_old";
            this.toolStripMenuItem_naviToActivateForm_old.Size = new System.Drawing.Size(144, 22);
            this.toolStripMenuItem_naviToActivateForm_old.Text = "激活窗(源)";
            this.toolStripMenuItem_naviToActivateForm_old.Click += new System.EventHandler(this.toolStripMenuItem_naviToActivateForm_old_Click);
            // 
            // toolStripMenuItem_openReaderManageForm
            // 
            this.toolStripMenuItem_openReaderManageForm.Enabled = false;
            this.toolStripMenuItem_openReaderManageForm.Name = "toolStripMenuItem_openReaderManageForm";
            this.toolStripMenuItem_openReaderManageForm.Size = new System.Drawing.Size(144, 22);
            this.toolStripMenuItem_openReaderManageForm.Text = "停借窗";
            this.toolStripMenuItem_openReaderManageForm.Click += new System.EventHandler(this.toolStripMenuItem_openReaderManageForm_Click);
            // 
            // toolStripMenuItem_naviToActivateForm_new
            // 
            this.toolStripMenuItem_naviToActivateForm_new.Enabled = false;
            this.toolStripMenuItem_naviToActivateForm_new.Name = "toolStripMenuItem_naviToActivateForm_new";
            this.toolStripMenuItem_naviToActivateForm_new.Size = new System.Drawing.Size(144, 22);
            this.toolStripMenuItem_naviToActivateForm_new.Text = "激活窗(目标)";
            this.toolStripMenuItem_naviToActivateForm_new.Click += new System.EventHandler(this.toolStripMenuItem_naviToActivateForm_new_Click);
            // 
            // tableLayoutPanel_biblioAndItem
            // 
            this.tableLayoutPanel_biblioAndItem.AutoSize = true;
            this.tableLayoutPanel_biblioAndItem.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_biblioAndItem.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel_biblioAndItem.ColumnCount = 1;
            this.tableLayoutPanel_biblioAndItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_biblioAndItem.Controls.Add(this.tableLayoutPanel_operation, 0, 1);
            this.tableLayoutPanel_biblioAndItem.Controls.Add(this.splitContainer_biblioAndItem, 0, 0);
            this.tableLayoutPanel_biblioAndItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_biblioAndItem.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_biblioAndItem.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_biblioAndItem.MinimumSize = new System.Drawing.Size(150, 0);
            this.tableLayoutPanel_biblioAndItem.Name = "tableLayoutPanel_biblioAndItem";
            this.tableLayoutPanel_biblioAndItem.RowCount = 3;
            this.tableLayoutPanel_biblioAndItem.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblioAndItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioAndItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioAndItem.Size = new System.Drawing.Size(227, 380);
            this.tableLayoutPanel_biblioAndItem.TabIndex = 11;
            // 
            // tableLayoutPanel_operation
            // 
            this.tableLayoutPanel_operation.AutoSize = true;
            this.tableLayoutPanel_operation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_operation.ColumnCount = 4;
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operation.Controls.Add(this.textBox_readerBarcode, 1, 0);
            this.tableLayoutPanel_operation.Controls.Add(this.textBox_readerPassword, 1, 1);
            this.tableLayoutPanel_operation.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_operation.Controls.Add(this.label_verifyReaderPassword, 0, 1);
            this.tableLayoutPanel_operation.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel_operation.Controls.Add(this.button_verifyReaderPassword, 2, 1);
            this.tableLayoutPanel_operation.Controls.Add(this.textBox_itemBarcode, 1, 2);
            this.tableLayoutPanel_operation.Controls.Add(this.button_loadReader, 2, 0);
            this.tableLayoutPanel_operation.Controls.Add(this.button_itemAction, 2, 2);
            this.tableLayoutPanel_operation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_operation.Location = new System.Drawing.Point(0, 290);
            this.tableLayoutPanel_operation.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_operation.MaximumSize = new System.Drawing.Size(375, 300);
            this.tableLayoutPanel_operation.MinimumSize = new System.Drawing.Size(150, 0);
            this.tableLayoutPanel_operation.Name = "tableLayoutPanel_operation";
            this.tableLayoutPanel_operation.RowCount = 5;
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.Size = new System.Drawing.Size(227, 90);
            this.tableLayoutPanel_operation.TabIndex = 10;
            // 
            // textBox_readerBarcode
            // 
            this.textBox_readerBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_readerBarcode.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_readerBarcode.Location = new System.Drawing.Point(104, 3);
            this.textBox_readerBarcode.MaximumSize = new System.Drawing.Size(188, 25);
            this.textBox_readerBarcode.MinimumSize = new System.Drawing.Size(68, 25);
            this.textBox_readerBarcode.Name = "textBox_readerBarcode";
            this.textBox_readerBarcode.Size = new System.Drawing.Size(68, 25);
            this.textBox_readerBarcode.TabIndex = 1;
            this.textBox_readerBarcode.TextChanged += new System.EventHandler(this.textBox_readerBarcode_TextChanged);
            this.textBox_readerBarcode.Enter += new System.EventHandler(this.textBox_readerBarcode_Enter);
            this.textBox_readerBarcode.Leave += new System.EventHandler(this.textBox_readerBarcode_Leave);
            // 
            // textBox_readerPassword
            // 
            this.textBox_readerPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_readerPassword.Location = new System.Drawing.Point(104, 33);
            this.textBox_readerPassword.MaximumSize = new System.Drawing.Size(188, 25);
            this.textBox_readerPassword.MinimumSize = new System.Drawing.Size(68, 25);
            this.textBox_readerPassword.Name = "textBox_readerPassword";
            this.textBox_readerPassword.PasswordChar = '*';
            this.textBox_readerPassword.Size = new System.Drawing.Size(68, 25);
            this.textBox_readerPassword.TabIndex = 4;
            this.textBox_readerPassword.Enter += new System.EventHandler(this.textBox_readerPassword_Enter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号(&R)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_verifyReaderPassword
            // 
            this.label_verifyReaderPassword.AutoSize = true;
            this.label_verifyReaderPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_verifyReaderPassword.Location = new System.Drawing.Point(3, 30);
            this.label_verifyReaderPassword.Name = "label_verifyReaderPassword";
            this.label_verifyReaderPassword.Size = new System.Drawing.Size(95, 30);
            this.label_verifyReaderPassword.TabIndex = 3;
            this.label_verifyReaderPassword.Text = "读者密码(&P)";
            this.label_verifyReaderPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.ForeColor = System.Drawing.Color.Green;
            this.label2.Location = new System.Drawing.Point(3, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 30);
            this.label2.TabIndex = 6;
            this.label2.Text = "册条码号(&I)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button_verifyReaderPassword
            // 
            this.button_verifyReaderPassword.AutoSize = true;
            this.button_verifyReaderPassword.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_verifyReaderPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_verifyReaderPassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_verifyReaderPassword.Image = ((System.Drawing.Image)(resources.GetObject("button_verifyReaderPassword.Image")));
            this.button_verifyReaderPassword.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_verifyReaderPassword.Location = new System.Drawing.Point(149, 33);
            this.button_verifyReaderPassword.Name = "button_verifyReaderPassword";
            this.button_verifyReaderPassword.Size = new System.Drawing.Size(75, 24);
            this.button_verifyReaderPassword.TabIndex = 5;
            this.button_verifyReaderPassword.Text = "验证(&V)";
            this.button_verifyReaderPassword.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_verifyReaderPassword.UseVisualStyleBackColor = true;
            this.button_verifyReaderPassword.Click += new System.EventHandler(this.button_verifyReaderPassword_Click);
            // 
            // textBox_itemBarcode
            // 
            this.textBox_itemBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_itemBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_itemBarcode.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_itemBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_itemBarcode.Location = new System.Drawing.Point(104, 63);
            this.textBox_itemBarcode.MaximumSize = new System.Drawing.Size(188, 25);
            this.textBox_itemBarcode.MinimumSize = new System.Drawing.Size(68, 25);
            this.textBox_itemBarcode.Name = "textBox_itemBarcode";
            this.textBox_itemBarcode.Size = new System.Drawing.Size(68, 25);
            this.textBox_itemBarcode.TabIndex = 7;
            this.textBox_itemBarcode.TextChanged += new System.EventHandler(this.textBox_itemBarcode_TextChanged);
            this.textBox_itemBarcode.Enter += new System.EventHandler(this.textBox_itemBarcode_Enter);
            this.textBox_itemBarcode.Leave += new System.EventHandler(this.textBox_itemBarcode_Leave);
            // 
            // button_loadReader
            // 
            this.button_loadReader.AutoSize = true;
            this.button_loadReader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_loadReader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_loadReader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_loadReader.Image = ((System.Drawing.Image)(resources.GetObject("button_loadReader.Image")));
            this.button_loadReader.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_loadReader.Location = new System.Drawing.Point(149, 3);
            this.button_loadReader.Name = "button_loadReader";
            this.button_loadReader.Size = new System.Drawing.Size(75, 24);
            this.button_loadReader.TabIndex = 2;
            this.button_loadReader.Text = "装载(&L)";
            this.button_loadReader.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_loadReader.UseVisualStyleBackColor = true;
            this.button_loadReader.Click += new System.EventHandler(this.button_loadReader_Click);
            // 
            // button_itemAction
            // 
            this.button_itemAction.AutoSize = true;
            this.button_itemAction.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_itemAction.ContextMenuStrip = this.contextMenuStrip_selectFunc;
            this.button_itemAction.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_itemAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_itemAction.Image = ((System.Drawing.Image)(resources.GetObject("button_itemAction.Image")));
            this.button_itemAction.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_itemAction.Location = new System.Drawing.Point(149, 63);
            this.button_itemAction.Name = "button_itemAction";
            this.button_itemAction.Size = new System.Drawing.Size(75, 24);
            this.button_itemAction.TabIndex = 8;
            this.button_itemAction.Text = "执行(&E)";
            this.button_itemAction.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_itemAction.UseVisualStyleBackColor = true;
            this.button_itemAction.Click += new System.EventHandler(this.button_itemAction_Click);
            // 
            // splitContainer_biblioAndItem
            // 
            this.splitContainer_biblioAndItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_biblioAndItem.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_biblioAndItem.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_biblioAndItem.MinimumSize = new System.Drawing.Size(150, 0);
            this.splitContainer_biblioAndItem.Name = "splitContainer_biblioAndItem";
            this.splitContainer_biblioAndItem.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_biblioAndItem.Panel1
            // 
            this.splitContainer_biblioAndItem.Panel1.Controls.Add(this.tableLayoutPanel_biblioInfo);
            // 
            // splitContainer_biblioAndItem.Panel2
            // 
            this.splitContainer_biblioAndItem.Panel2.Controls.Add(this.tableLayoutPanel_itemInfo);
            this.splitContainer_biblioAndItem.Size = new System.Drawing.Size(227, 290);
            this.splitContainer_biblioAndItem.SplitterDistance = 138;
            this.splitContainer_biblioAndItem.TabIndex = 0;
            // 
            // tableLayoutPanel_biblioInfo
            // 
            this.tableLayoutPanel_biblioInfo.AutoSize = true;
            this.tableLayoutPanel_biblioInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_biblioInfo.ColumnCount = 1;
            this.tableLayoutPanel_biblioInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblioInfo.Controls.Add(this.webBrowser_biblio, 0, 1);
            this.tableLayoutPanel_biblioInfo.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel_biblioInfo.Controls.Add(this.textBox_biblioInfo, 0, 2);
            this.tableLayoutPanel_biblioInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_biblioInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_biblioInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_biblioInfo.MinimumSize = new System.Drawing.Size(150, 0);
            this.tableLayoutPanel_biblioInfo.Name = "tableLayoutPanel_biblioInfo";
            this.tableLayoutPanel_biblioInfo.RowCount = 3;
            this.tableLayoutPanel_biblioInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioInfo.Size = new System.Drawing.Size(227, 138);
            this.tableLayoutPanel_biblioInfo.TabIndex = 1;
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(0, 12);
            this.webBrowser_biblio.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(227, 47);
            this.webBrowser_biblio.TabIndex = 0;
            this.webBrowser_biblio.TabStop = false;
            this.webBrowser_biblio.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_biblio_DocumentCompleted);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label5.Location = new System.Drawing.Point(2, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "书目信息";
            // 
            // textBox_biblioInfo
            // 
            this.textBox_biblioInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_biblioInfo.Location = new System.Drawing.Point(2, 61);
            this.textBox_biblioInfo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_biblioInfo.MaxLength = 0;
            this.textBox_biblioInfo.MinimumSize = new System.Drawing.Size(151, 4);
            this.textBox_biblioInfo.Multiline = true;
            this.textBox_biblioInfo.Name = "textBox_biblioInfo";
            this.textBox_biblioInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_biblioInfo.Size = new System.Drawing.Size(223, 83);
            this.textBox_biblioInfo.TabIndex = 1;
            // 
            // tableLayoutPanel_itemInfo
            // 
            this.tableLayoutPanel_itemInfo.AutoSize = true;
            this.tableLayoutPanel_itemInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_itemInfo.ColumnCount = 1;
            this.tableLayoutPanel_itemInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_itemInfo.Controls.Add(this.webBrowser_item, 0, 1);
            this.tableLayoutPanel_itemInfo.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel_itemInfo.Controls.Add(this.textBox_itemInfo, 0, 2);
            this.tableLayoutPanel_itemInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_itemInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_itemInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_itemInfo.MinimumSize = new System.Drawing.Size(150, 0);
            this.tableLayoutPanel_itemInfo.Name = "tableLayoutPanel_itemInfo";
            this.tableLayoutPanel_itemInfo.RowCount = 3;
            this.tableLayoutPanel_itemInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_itemInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_itemInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_itemInfo.Size = new System.Drawing.Size(227, 148);
            this.tableLayoutPanel_itemInfo.TabIndex = 1;
            // 
            // webBrowser_item
            // 
            this.webBrowser_item.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_item.Location = new System.Drawing.Point(0, 12);
            this.webBrowser_item.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_item.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_item.Name = "webBrowser_item";
            this.webBrowser_item.Size = new System.Drawing.Size(227, 52);
            this.webBrowser_item.TabIndex = 0;
            this.webBrowser_item.TabStop = false;
            this.webBrowser_item.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_item_DocumentCompleted);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label6.Location = new System.Drawing.Point(2, 0);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "册信息";
            // 
            // textBox_itemInfo
            // 
            this.textBox_itemInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_itemInfo.Location = new System.Drawing.Point(2, 66);
            this.textBox_itemInfo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_itemInfo.MaxLength = 0;
            this.textBox_itemInfo.MinimumSize = new System.Drawing.Size(151, 4);
            this.textBox_itemInfo.Multiline = true;
            this.textBox_itemInfo.Name = "textBox_itemInfo";
            this.textBox_itemInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_itemInfo.Size = new System.Drawing.Size(223, 83);
            this.textBox_itemInfo.TabIndex = 1;
            // 
            // contextMenuStrip_verifyReaderPassword
            // 
            this.contextMenuStrip_verifyReaderPassword.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_verifyReaderPassword});
            this.contextMenuStrip_verifyReaderPassword.Name = "contextMenuStrip_verifyReaderPassword";
            this.contextMenuStrip_verifyReaderPassword.Size = new System.Drawing.Size(165, 26);
            // 
            // MenuItem_verifyReaderPassword
            // 
            this.MenuItem_verifyReaderPassword.Name = "MenuItem_verifyReaderPassword";
            this.MenuItem_verifyReaderPassword.Size = new System.Drawing.Size(164, 22);
            this.MenuItem_verifyReaderPassword.Text = "校验读者密码(&V)";
            // 
            // panel_main
            // 
            this.panel_main.Controls.Add(this.splitContainer_main);
            this.panel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_main.Location = new System.Drawing.Point(0, 0);
            this.panel_main.Margin = new System.Windows.Forms.Padding(2);
            this.panel_main.Name = "panel_main";
            this.panel_main.Padding = new System.Windows.Forms.Padding(0, 8, 0, 4);
            this.panel_main.Size = new System.Drawing.Size(442, 392);
            this.panel_main.TabIndex = 7;
            // 
            // ChargingForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 392);
            this.Controls.Add(this.panel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChargingForm";
            this.ShowInTaskbar = false;
            this.Text = "出纳";
            this.Activated += new System.EventHandler(this.ChargingForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChargingForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChargingForm_FormClosed);
            this.Load += new System.EventHandler(this.ChargingForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ChargingForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ChargingForm_DragEnter);
            this.contextMenuStrip_selectFunc.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_readerInfo.ResumeLayout(false);
            this.tableLayoutPanel_readerInfo.PerformLayout();
            this.toolStrip_navigate.ResumeLayout(false);
            this.toolStrip_navigate.PerformLayout();
            this.tableLayoutPanel_biblioAndItem.ResumeLayout(false);
            this.tableLayoutPanel_biblioAndItem.PerformLayout();
            this.tableLayoutPanel_operation.ResumeLayout(false);
            this.tableLayoutPanel_operation.PerformLayout();
            this.splitContainer_biblioAndItem.Panel1.ResumeLayout(false);
            this.splitContainer_biblioAndItem.Panel1.PerformLayout();
            this.splitContainer_biblioAndItem.Panel2.ResumeLayout(false);
            this.splitContainer_biblioAndItem.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_biblioAndItem)).EndInit();
            this.splitContainer_biblioAndItem.ResumeLayout(false);
            this.tableLayoutPanel_biblioInfo.ResumeLayout(false);
            this.tableLayoutPanel_biblioInfo.PerformLayout();
            this.tableLayoutPanel_itemInfo.ResumeLayout(false);
            this.tableLayoutPanel_itemInfo.PerformLayout();
            this.contextMenuStrip_verifyReaderPassword.ResumeLayout(false);
            this.panel_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser_reader;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_selectFunc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_borrow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_return;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_verifyRenew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_lost;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_verifyReaderPassword;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_verifyReaderPassword;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_readerInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_biblioAndItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_operation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_verifyReaderPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_verifyReaderPassword;
        private System.Windows.Forms.Button button_itemAction;
        private System.Windows.Forms.TextBox textBox_itemBarcode;
        private System.Windows.Forms.TextBox textBox_readerPassword;
        private System.Windows.Forms.Button button_loadReader;
        private System.Windows.Forms.TextBox textBox_readerBarcode;
        private System.Windows.Forms.SplitContainer splitContainer_biblioAndItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_biblioInfo;
        private System.Windows.Forms.WebBrowser webBrowser_biblio;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_itemInfo;
        private System.Windows.Forms.WebBrowser webBrowser_item;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_verifyReturn;
        private System.Windows.Forms.TextBox textBox_readerInfo;
        private System.Windows.Forms.TextBox textBox_biblioInfo;
        private System.Windows.Forms.TextBox textBox_itemInfo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_naviToAmerceForm;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_naviToReaderInfoForm;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_naviToActivateForm_old;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_naviToActivateForm_new;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_openReaderManageForm;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_openEntityForm;
        private System.Windows.Forms.ToolStrip toolStrip_navigate;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_readerBarcodeNavigate;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_itemBarcodeNavigate;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_openItemInfoForm;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_renew;
    }
}