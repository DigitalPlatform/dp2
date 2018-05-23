namespace dp2Circulation
{
    partial class MainForm
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

            RemoveDownloader(null, true);

            if (this.PropertyTaskList != null)
                this.PropertyTaskList.Dispose();
            if (this.OperHistory != null)
                this.OperHistory.Dispose();

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
                this._imageManager.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip_main = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_runProject = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_projectManage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_startAnotherDp2circulation = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_functionWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openQuickChargingForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openChargingForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openEntityRegisterWizard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openReaderSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openItemSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openBiblioSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openOrderSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openIssueSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openCommentSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openInvoiceSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openArrivedSearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openMarc856SearchForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openReaderInfoForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openItemInfoForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openEntityForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openAmerceForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openActivateForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openReaderManageForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openChangePasswordForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_function = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_openFunctionWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openSettlementForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openPassGateForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openZhongcihaoForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openCallNumberForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openChargingPrintManageForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openCardPrintForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openLabelPrintForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_statisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openOperLogStatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator28 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openReaderStatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator25 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openItemStatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openOrderStatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator26 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openBiblioStatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator27 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openXmlStatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_openIso2709StatisForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_openReportForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_statisProjectManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_installStatisProjects = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_updateStatisProjects = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_installStatisProjectsFromDisk = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_updateStatisProjectsFromDisk = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_separator_function2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_chatForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_messageForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openReservationListForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator30 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_systemManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openClockForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openCalendarForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openBatchTaskForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openOperLogForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openManagerForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_channelForm = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openTestForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUrgentChargingForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_operCheckBorrowInfoForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_recoverUrgentLog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_clearCache = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearCfgCache = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearSummaryCache = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearDatabaseInfoCatch = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_reLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_logout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator29 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_initFingerprintCache = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_batch = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openQuickChangeEntityForm_1 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openQuickChangeBiblioForm = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_batchOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_itemHandover = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_printOrder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_accept = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_printAccept = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_printClaim = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_printAccountBook = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_printBindingList = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_inventory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_importExport = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_importFromOrderDistributeExcelFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openTestSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_window = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_tileHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_tileVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_cascade = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_arrangeIcons = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_closeAllMdiWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_displayFixPanel = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_ui = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_font = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_restoreDefaultFont = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_configuration = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_resetSerialCode = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_utility = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem_packageErrorLog = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_updateDp2circulation = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_createGreenApplication = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_upgradeFromDisk = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_refreshLibraryUID = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator31 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_copyright = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar_main = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_stopAll = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_stopAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton_selectLibraryCode = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox_barcode = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton_loadBarcode = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_barcodeLoadStyle = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_loadReaderInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_loadItemInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_autoLoadItemOrReader = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.toolButton_borrow = new System.Windows.Forms.ToolStripButton();
            this.toolButton_return = new System.Windows.Forms.ToolStripButton();
            this.toolButton_verifyReturn = new System.Windows.Forms.ToolStripButton();
            this.toolButton_renew = new System.Windows.Forms.ToolStripButton();
            this.toolButton_lost = new System.Windows.Forms.ToolStripButton();
            this.toolButton_amerce = new System.Windows.Forms.ToolStripButton();
            this.toolButton_readerManage = new System.Windows.Forms.ToolStripButton();
            this.toolButton_print = new System.Windows.Forms.ToolStripButton();
            this.panel_fixed = new System.Windows.Forms.Panel();
            this.tabControl_panelFixed = new System.Windows.Forms.TabControl();
            this.contextMenuStrip_fixedPanel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_fixedPanel_clear = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage_history = new System.Windows.Forms.TabPage();
            this.webBrowser_history = new System.Windows.Forms.WebBrowser();
            this.tabPage_property = new System.Windows.Forms.TabPage();
            this.tabPage_verifyResult = new System.Windows.Forms.TabPage();
            this.tabPage_generateData = new System.Windows.Forms.TabPage();
            this.tabPage_camera = new System.Windows.Forms.TabPage();
            this.tabPage_accept = new System.Windows.Forms.TabPage();
            this.tabPage_share = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_messageHub = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_messageHub = new System.Windows.Forms.WebBrowser();
            this.toolStrip_messageHub = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_messageHub_userManage = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_messageHub_relogin = new System.Windows.Forms.ToolStripButton();
            this.tabPage_browse = new System.Windows.Forms.TabPage();
            this.toolStrip_panelFixed = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_close = new System.Windows.Forms.ToolStripButton();
            this.splitter_fixed = new System.Windows.Forms.Splitter();
            this.timer_operHistory = new System.Windows.Forms.Timer(this.components);
            this.menuStrip_main.SuspendLayout();
            this.statusStrip_main.SuspendLayout();
            this.toolStrip_main.SuspendLayout();
            this.panel_fixed.SuspendLayout();
            this.tabControl_panelFixed.SuspendLayout();
            this.contextMenuStrip_fixedPanel.SuspendLayout();
            this.tabPage_history.SuspendLayout();
            this.tabPage_share.SuspendLayout();
            this.tableLayoutPanel_messageHub.SuspendLayout();
            this.toolStrip_messageHub.SuspendLayout();
            this.toolStrip_panelFixed.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip_main
            // 
            this.menuStrip_main.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_functionWindows,
            this.MenuItem_function,
            this.MenuItem_batch,
            this.MenuItem_window,
            this.MenuItem_ui,
            this.MenuItem_help});
            this.menuStrip_main.Location = new System.Drawing.Point(0, 0);
            this.menuStrip_main.MdiWindowListItem = this.MenuItem_window;
            this.menuStrip_main.Name = "menuStrip_main";
            this.menuStrip_main.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip_main.Size = new System.Drawing.Size(943, 34);
            this.menuStrip_main.TabIndex = 0;
            this.menuStrip_main.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_runProject,
            this.ToolStripMenuItem_projectManage,
            this.toolStripSeparator6,
            this.MenuItem_startAnotherDp2circulation,
            this.toolStripSeparator14,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(80, 28);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // toolStripMenuItem_runProject
            // 
            this.toolStripMenuItem_runProject.Name = "toolStripMenuItem_runProject";
            this.toolStripMenuItem_runProject.Size = new System.Drawing.Size(360, 30);
            this.toolStripMenuItem_runProject.Text = "执行统计方案(&R)...";
            this.toolStripMenuItem_runProject.Click += new System.EventHandler(this.toolStripMenuItem_runProject_Click);
            // 
            // ToolStripMenuItem_projectManage
            // 
            this.ToolStripMenuItem_projectManage.Name = "ToolStripMenuItem_projectManage";
            this.ToolStripMenuItem_projectManage.Size = new System.Drawing.Size(360, 30);
            this.ToolStripMenuItem_projectManage.Text = "统计方案管理[框架窗口](&P)";
            this.ToolStripMenuItem_projectManage.Click += new System.EventHandler(this.ToolStripMenuItem_projectManage_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(357, 6);
            this.toolStripSeparator6.Visible = false;
            // 
            // MenuItem_startAnotherDp2circulation
            // 
            this.MenuItem_startAnotherDp2circulation.Name = "MenuItem_startAnotherDp2circulation";
            this.MenuItem_startAnotherDp2circulation.Size = new System.Drawing.Size(360, 30);
            this.MenuItem_startAnotherDp2circulation.Text = "启动新的 dp2Circulation 实例(&S)";
            this.MenuItem_startAnotherDp2circulation.Visible = false;
            this.MenuItem_startAnotherDp2circulation.Click += new System.EventHandler(this.MenuItem_startAnotherDp2circulation_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(357, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(360, 30);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_functionWindows
            // 
            this.MenuItem_functionWindows.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openQuickChargingForm,
            this.MenuItem_openChargingForm,
            this.MenuItem_openEntityRegisterWizard,
            this.toolStripSeparator3,
            this.MenuItem_openReaderSearchForm,
            this.MenuItem_openItemSearchForm,
            this.MenuItem_openBiblioSearchForm,
            this.toolStripSeparator19,
            this.MenuItem_openOrderSearchForm,
            this.MenuItem_openIssueSearchForm,
            this.MenuItem_openCommentSearchForm,
            this.MenuItem_openInvoiceSearchForm,
            this.MenuItem_openArrivedSearchForm,
            this.MenuItem_openMarc856SearchForm,
            this.toolStripSeparator1,
            this.MenuItem_openReaderInfoForm,
            this.MenuItem_openItemInfoForm,
            this.MenuItem_openEntityForm,
            this.toolStripSeparator7,
            this.MenuItem_openAmerceForm,
            this.MenuItem_openActivateForm,
            this.MenuItem_openReaderManageForm,
            this.toolStripSeparator5,
            this.MenuItem_openChangePasswordForm});
            this.MenuItem_functionWindows.Name = "MenuItem_functionWindows";
            this.MenuItem_functionWindows.Size = new System.Drawing.Size(118, 28);
            this.MenuItem_functionWindows.Text = "常用窗口(&R)";
            // 
            // MenuItem_openQuickChargingForm
            // 
            this.MenuItem_openQuickChargingForm.Name = "MenuItem_openQuickChargingForm";
            this.MenuItem_openQuickChargingForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openQuickChargingForm.Text = "快捷出纳窗(&Q)";
            this.MenuItem_openQuickChargingForm.Click += new System.EventHandler(this.MenuItem_openQuickChargingForm_Click);
            // 
            // MenuItem_openChargingForm
            // 
            this.MenuItem_openChargingForm.Name = "MenuItem_openChargingForm";
            this.MenuItem_openChargingForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openChargingForm.Text = "出纳窗(&C)";
            this.MenuItem_openChargingForm.Click += new System.EventHandler(this.MenuItem_openChargingForm_Click);
            // 
            // MenuItem_openEntityRegisterWizard
            // 
            this.MenuItem_openEntityRegisterWizard.Name = "MenuItem_openEntityRegisterWizard";
            this.MenuItem_openEntityRegisterWizard.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openEntityRegisterWizard.Text = "册登记窗(&T)";
            this.MenuItem_openEntityRegisterWizard.Click += new System.EventHandler(this.MenuItem_openEntityRegisterWizard_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(240, 6);
            // 
            // MenuItem_openReaderSearchForm
            // 
            this.MenuItem_openReaderSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openReaderSearchForm.Image")));
            this.MenuItem_openReaderSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openReaderSearchForm.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.MenuItem_openReaderSearchForm.Name = "MenuItem_openReaderSearchForm";
            this.MenuItem_openReaderSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openReaderSearchForm.Text = "读者查询窗(&S)";
            this.MenuItem_openReaderSearchForm.Click += new System.EventHandler(this.MenuItem_openReaderSearchForm_Click);
            // 
            // MenuItem_openItemSearchForm
            // 
            this.MenuItem_openItemSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openItemSearchForm.Image")));
            this.MenuItem_openItemSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openItemSearchForm.Name = "MenuItem_openItemSearchForm";
            this.MenuItem_openItemSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openItemSearchForm.Text = "实体查询窗(&E)";
            this.MenuItem_openItemSearchForm.Click += new System.EventHandler(this.MenuItem_openItemSearchForm_Click);
            // 
            // MenuItem_openBiblioSearchForm
            // 
            this.MenuItem_openBiblioSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openBiblioSearchForm.Image")));
            this.MenuItem_openBiblioSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openBiblioSearchForm.Name = "MenuItem_openBiblioSearchForm";
            this.MenuItem_openBiblioSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openBiblioSearchForm.Text = "书目查询窗(&B)";
            this.MenuItem_openBiblioSearchForm.Click += new System.EventHandler(this.MenuItem_openBiblioSearchForm_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(240, 6);
            // 
            // MenuItem_openOrderSearchForm
            // 
            this.MenuItem_openOrderSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openOrderSearchForm.Image")));
            this.MenuItem_openOrderSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openOrderSearchForm.Name = "MenuItem_openOrderSearchForm";
            this.MenuItem_openOrderSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openOrderSearchForm.Text = "订购查询窗(&O)";
            this.MenuItem_openOrderSearchForm.Click += new System.EventHandler(this.MenuItem_openOrderSearchForm_Click);
            // 
            // MenuItem_openIssueSearchForm
            // 
            this.MenuItem_openIssueSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openIssueSearchForm.Image")));
            this.MenuItem_openIssueSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openIssueSearchForm.Name = "MenuItem_openIssueSearchForm";
            this.MenuItem_openIssueSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openIssueSearchForm.Text = "期查询窗(&I)";
            this.MenuItem_openIssueSearchForm.Click += new System.EventHandler(this.MenuItem_openIssueSearchForm_Click);
            // 
            // MenuItem_openCommentSearchForm
            // 
            this.MenuItem_openCommentSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openCommentSearchForm.Image")));
            this.MenuItem_openCommentSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openCommentSearchForm.Name = "MenuItem_openCommentSearchForm";
            this.MenuItem_openCommentSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openCommentSearchForm.Text = "评注查询窗(&C)";
            this.MenuItem_openCommentSearchForm.Click += new System.EventHandler(this.MenuItem_openCommentSearchForm_Click);
            // 
            // MenuItem_openInvoiceSearchForm
            // 
            this.MenuItem_openInvoiceSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openInvoiceSearchForm.Name = "MenuItem_openInvoiceSearchForm";
            this.MenuItem_openInvoiceSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openInvoiceSearchForm.Text = "发票查询窗(&N)";
            this.MenuItem_openInvoiceSearchForm.Click += new System.EventHandler(this.MenuItem_openInvoiceSearchForm_Click);
            // 
            // MenuItem_openArrivedSearchForm
            // 
            this.MenuItem_openArrivedSearchForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openArrivedSearchForm.Image")));
            this.MenuItem_openArrivedSearchForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openArrivedSearchForm.Name = "MenuItem_openArrivedSearchForm";
            this.MenuItem_openArrivedSearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openArrivedSearchForm.Text = "预约到书查询窗(&A)";
            this.MenuItem_openArrivedSearchForm.Click += new System.EventHandler(this.MenuItem_openArrivedSearchForm_Click);
            // 
            // MenuItem_openMarc856SearchForm
            // 
            this.MenuItem_openMarc856SearchForm.Name = "MenuItem_openMarc856SearchForm";
            this.MenuItem_openMarc856SearchForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openMarc856SearchForm.Text = "856 字段查询窗(&8)";
            this.MenuItem_openMarc856SearchForm.Click += new System.EventHandler(this.MenuItem_openMarc856SearchForm_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(240, 6);
            // 
            // MenuItem_openReaderInfoForm
            // 
            this.MenuItem_openReaderInfoForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openReaderInfoForm.Image")));
            this.MenuItem_openReaderInfoForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openReaderInfoForm.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.MenuItem_openReaderInfoForm.Name = "MenuItem_openReaderInfoForm";
            this.MenuItem_openReaderInfoForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openReaderInfoForm.Text = "读者窗(&R)";
            this.MenuItem_openReaderInfoForm.Click += new System.EventHandler(this.MenuItem_openReaderInfoForm_Click);
            // 
            // MenuItem_openItemInfoForm
            // 
            this.MenuItem_openItemInfoForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openItemInfoForm.Image")));
            this.MenuItem_openItemInfoForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openItemInfoForm.Name = "MenuItem_openItemInfoForm";
            this.MenuItem_openItemInfoForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openItemInfoForm.Text = "册窗(&I)";
            this.MenuItem_openItemInfoForm.Click += new System.EventHandler(this.MenuItem_openItemInfoForm_Click);
            // 
            // MenuItem_openEntityForm
            // 
            this.MenuItem_openEntityForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openEntityForm.Image")));
            this.MenuItem_openEntityForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openEntityForm.Name = "MenuItem_openEntityForm";
            this.MenuItem_openEntityForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openEntityForm.Text = "种册窗(&E)";
            this.MenuItem_openEntityForm.Click += new System.EventHandler(this.MenuItem_openEntityForm_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(240, 6);
            // 
            // MenuItem_openAmerceForm
            // 
            this.MenuItem_openAmerceForm.Name = "MenuItem_openAmerceForm";
            this.MenuItem_openAmerceForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openAmerceForm.Text = "交费窗(&A)";
            this.MenuItem_openAmerceForm.Click += new System.EventHandler(this.MenuItem_openAmerceForm_Click);
            // 
            // MenuItem_openActivateForm
            // 
            this.MenuItem_openActivateForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openActivateForm.Name = "MenuItem_openActivateForm";
            this.MenuItem_openActivateForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openActivateForm.Text = "激活窗(&A)";
            this.MenuItem_openActivateForm.Click += new System.EventHandler(this.MenuItem_openActivateForm_Click);
            // 
            // MenuItem_openReaderManageForm
            // 
            this.MenuItem_openReaderManageForm.Name = "MenuItem_openReaderManageForm";
            this.MenuItem_openReaderManageForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openReaderManageForm.Text = "停借窗(&M)";
            this.MenuItem_openReaderManageForm.Click += new System.EventHandler(this.MenuItem_openReaderManageForm_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(240, 6);
            // 
            // MenuItem_openChangePasswordForm
            // 
            this.MenuItem_openChangePasswordForm.Name = "MenuItem_openChangePasswordForm";
            this.MenuItem_openChangePasswordForm.Size = new System.Drawing.Size(243, 30);
            this.MenuItem_openChangePasswordForm.Text = "修改密码窗(&P)";
            this.MenuItem_openChangePasswordForm.Click += new System.EventHandler(this.MenuItem_openChangePasswordForm_Click);
            // 
            // MenuItem_function
            // 
            this.MenuItem_function.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_openFunctionWindow,
            this.toolStripSeparator13,
            this.toolStripMenuItem_statisForm,
            this.toolStripMenuItem_statisProjectManagement,
            this.MenuItem_separator_function2,
            this.MenuItem_chatForm,
            this.MenuItem_messageForm,
            this.MenuItem_openReservationListForm,
            this.toolStripSeparator30,
            this.MenuItem_systemManagement,
            this.MenuItem_recoverUrgentLog,
            this.toolStripSeparator12,
            this.ToolStripMenuItem_clearCache,
            this.MenuItem_reLogin,
            this.MenuItem_logout,
            this.toolStripSeparator29,
            this.MenuItem_initFingerprintCache});
            this.MenuItem_function.Name = "MenuItem_function";
            this.MenuItem_function.Size = new System.Drawing.Size(83, 28);
            this.MenuItem_function.Text = "功能(&U)";
            // 
            // ToolStripMenuItem_openFunctionWindow
            // 
            this.ToolStripMenuItem_openFunctionWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openSettlementForm,
            this.MenuItem_openPassGateForm,
            this.MenuItem_openZhongcihaoForm,
            this.MenuItem_openCallNumberForm,
            this.toolStripSeparator23,
            this.MenuItem_openChargingPrintManageForm,
            this.MenuItem_openCardPrintForm,
            this.MenuItem_openLabelPrintForm});
            this.ToolStripMenuItem_openFunctionWindow.Name = "ToolStripMenuItem_openFunctionWindow";
            this.ToolStripMenuItem_openFunctionWindow.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_openFunctionWindow.Text = "打开功能窗口";
            // 
            // MenuItem_openSettlementForm
            // 
            this.MenuItem_openSettlementForm.Name = "MenuItem_openSettlementForm";
            this.MenuItem_openSettlementForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openSettlementForm.Text = "结算窗(&S)";
            this.MenuItem_openSettlementForm.Click += new System.EventHandler(this.MenuItem_openSettlementForm_Click);
            // 
            // MenuItem_openPassGateForm
            // 
            this.MenuItem_openPassGateForm.Name = "MenuItem_openPassGateForm";
            this.MenuItem_openPassGateForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openPassGateForm.Text = "入馆登记窗(&P)";
            this.MenuItem_openPassGateForm.Click += new System.EventHandler(this.MenuItem_openPassGateForm_Click);
            // 
            // MenuItem_openZhongcihaoForm
            // 
            this.MenuItem_openZhongcihaoForm.Name = "MenuItem_openZhongcihaoForm";
            this.MenuItem_openZhongcihaoForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openZhongcihaoForm.Text = "种次号窗(&Z)";
            this.MenuItem_openZhongcihaoForm.Click += new System.EventHandler(this.MenuItem_openZhongcihaoForm_Click);
            // 
            // MenuItem_openCallNumberForm
            // 
            this.MenuItem_openCallNumberForm.Name = "MenuItem_openCallNumberForm";
            this.MenuItem_openCallNumberForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openCallNumberForm.Text = "同类书区分号窗(&C)";
            this.MenuItem_openCallNumberForm.Click += new System.EventHandler(this.MenuItem_openCallNumberForm_Click);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(239, 6);
            // 
            // MenuItem_openChargingPrintManageForm
            // 
            this.MenuItem_openChargingPrintManageForm.Name = "MenuItem_openChargingPrintManageForm";
            this.MenuItem_openChargingPrintManageForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openChargingPrintManageForm.Text = "出纳打印管理窗(&C)";
            this.MenuItem_openChargingPrintManageForm.Click += new System.EventHandler(this.MenuItem_openChargingPrintManageForm_Click);
            // 
            // MenuItem_openCardPrintForm
            // 
            this.MenuItem_openCardPrintForm.Name = "MenuItem_openCardPrintForm";
            this.MenuItem_openCardPrintForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openCardPrintForm.Text = "卡片打印窗(&C)";
            this.MenuItem_openCardPrintForm.Click += new System.EventHandler(this.MenuItem_openCardPrintForm_Click);
            // 
            // MenuItem_openLabelPrintForm
            // 
            this.MenuItem_openLabelPrintForm.Name = "MenuItem_openLabelPrintForm";
            this.MenuItem_openLabelPrintForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openLabelPrintForm.Text = "标签打印窗(&L)";
            this.MenuItem_openLabelPrintForm.Click += new System.EventHandler(this.MenuItem_openLabelPrintForm_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(305, 6);
            // 
            // toolStripMenuItem_statisForm
            // 
            this.toolStripMenuItem_statisForm.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openOperLogStatisForm,
            this.toolStripSeparator28,
            this.MenuItem_openReaderStatisForm,
            this.toolStripSeparator25,
            this.MenuItem_openItemStatisForm,
            this.MenuItem_openOrderStatisForm,
            this.toolStripSeparator26,
            this.MenuItem_openBiblioStatisForm,
            this.toolStripSeparator27,
            this.MenuItem_openXmlStatisForm,
            this.ToolStripMenuItem_openIso2709StatisForm,
            this.toolStripSeparator4,
            this.ToolStripMenuItem_openReportForm});
            this.toolStripMenuItem_statisForm.Name = "toolStripMenuItem_statisForm";
            this.toolStripMenuItem_statisForm.Size = new System.Drawing.Size(308, 30);
            this.toolStripMenuItem_statisForm.Text = "统计窗";
            // 
            // MenuItem_openOperLogStatisForm
            // 
            this.MenuItem_openOperLogStatisForm.Name = "MenuItem_openOperLogStatisForm";
            this.MenuItem_openOperLogStatisForm.Size = new System.Drawing.Size(237, 30);
            this.MenuItem_openOperLogStatisForm.Text = "日志统计窗(&S)";
            this.MenuItem_openOperLogStatisForm.Click += new System.EventHandler(this.MenuItem_openOperLogStatisForm_Click);
            // 
            // toolStripSeparator28
            // 
            this.toolStripSeparator28.Name = "toolStripSeparator28";
            this.toolStripSeparator28.Size = new System.Drawing.Size(234, 6);
            // 
            // MenuItem_openReaderStatisForm
            // 
            this.MenuItem_openReaderStatisForm.Name = "MenuItem_openReaderStatisForm";
            this.MenuItem_openReaderStatisForm.Size = new System.Drawing.Size(237, 30);
            this.MenuItem_openReaderStatisForm.Text = "读者统计窗(&R)";
            this.MenuItem_openReaderStatisForm.Click += new System.EventHandler(this.MenuItem_openReaderStatisForm_Click);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(234, 6);
            // 
            // MenuItem_openItemStatisForm
            // 
            this.MenuItem_openItemStatisForm.Name = "MenuItem_openItemStatisForm";
            this.MenuItem_openItemStatisForm.Size = new System.Drawing.Size(237, 30);
            this.MenuItem_openItemStatisForm.Text = "册统计窗(&I)";
            this.MenuItem_openItemStatisForm.Click += new System.EventHandler(this.MenuItem_openItemStatisForm_Click);
            // 
            // MenuItem_openOrderStatisForm
            // 
            this.MenuItem_openOrderStatisForm.Name = "MenuItem_openOrderStatisForm";
            this.MenuItem_openOrderStatisForm.Size = new System.Drawing.Size(237, 30);
            this.MenuItem_openOrderStatisForm.Text = "订购统计窗(&O)";
            this.MenuItem_openOrderStatisForm.Click += new System.EventHandler(this.MenuItem_openOrderStatisForm_Click);
            // 
            // toolStripSeparator26
            // 
            this.toolStripSeparator26.Name = "toolStripSeparator26";
            this.toolStripSeparator26.Size = new System.Drawing.Size(234, 6);
            // 
            // MenuItem_openBiblioStatisForm
            // 
            this.MenuItem_openBiblioStatisForm.Name = "MenuItem_openBiblioStatisForm";
            this.MenuItem_openBiblioStatisForm.Size = new System.Drawing.Size(237, 30);
            this.MenuItem_openBiblioStatisForm.Text = "书目统计窗(&B)";
            this.MenuItem_openBiblioStatisForm.Click += new System.EventHandler(this.MenuItem_openBiblioStatisForm_Click);
            // 
            // toolStripSeparator27
            // 
            this.toolStripSeparator27.Name = "toolStripSeparator27";
            this.toolStripSeparator27.Size = new System.Drawing.Size(234, 6);
            // 
            // MenuItem_openXmlStatisForm
            // 
            this.MenuItem_openXmlStatisForm.Name = "MenuItem_openXmlStatisForm";
            this.MenuItem_openXmlStatisForm.Size = new System.Drawing.Size(237, 30);
            this.MenuItem_openXmlStatisForm.Text = "XML统计窗(&X)";
            this.MenuItem_openXmlStatisForm.Click += new System.EventHandler(this.MenuItem_openXmlStatisForm_Click);
            // 
            // ToolStripMenuItem_openIso2709StatisForm
            // 
            this.ToolStripMenuItem_openIso2709StatisForm.Name = "ToolStripMenuItem_openIso2709StatisForm";
            this.ToolStripMenuItem_openIso2709StatisForm.Size = new System.Drawing.Size(237, 30);
            this.ToolStripMenuItem_openIso2709StatisForm.Text = "ISO2709统计窗(&I)";
            this.ToolStripMenuItem_openIso2709StatisForm.Click += new System.EventHandler(this.MenuItem_openIso2709StatisForm_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(234, 6);
            // 
            // ToolStripMenuItem_openReportForm
            // 
            this.ToolStripMenuItem_openReportForm.Name = "ToolStripMenuItem_openReportForm";
            this.ToolStripMenuItem_openReportForm.Size = new System.Drawing.Size(237, 30);
            this.ToolStripMenuItem_openReportForm.Text = "报表窗(&R)";
            this.ToolStripMenuItem_openReportForm.Click += new System.EventHandler(this.ToolStripMenuItem_openReportForm_Click);
            // 
            // toolStripMenuItem_statisProjectManagement
            // 
            this.toolStripMenuItem_statisProjectManagement.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_installStatisProjects,
            this.MenuItem_updateStatisProjects,
            this.toolStripSeparator22,
            this.MenuItem_installStatisProjectsFromDisk,
            this.MenuItem_updateStatisProjectsFromDisk});
            this.toolStripMenuItem_statisProjectManagement.Name = "toolStripMenuItem_statisProjectManagement";
            this.toolStripMenuItem_statisProjectManagement.Size = new System.Drawing.Size(308, 30);
            this.toolStripMenuItem_statisProjectManagement.Text = "统计方案管理";
            // 
            // MenuItem_installStatisProjects
            // 
            this.MenuItem_installStatisProjects.Name = "MenuItem_installStatisProjects";
            this.MenuItem_installStatisProjects.Size = new System.Drawing.Size(398, 30);
            this.MenuItem_installStatisProjects.Text = "从 dp2003.com 安装全部方案(&I)";
            this.MenuItem_installStatisProjects.Click += new System.EventHandler(this.MenuItem_installStatisProjects_Click);
            // 
            // MenuItem_updateStatisProjects
            // 
            this.MenuItem_updateStatisProjects.Name = "MenuItem_updateStatisProjects";
            this.MenuItem_updateStatisProjects.Size = new System.Drawing.Size(398, 30);
            this.MenuItem_updateStatisProjects.Text = "从 dp2003.com 检查更新全部方案(&U)";
            this.MenuItem_updateStatisProjects.Click += new System.EventHandler(this.MenuItem_updateStatisProjects_Click);
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(395, 6);
            // 
            // MenuItem_installStatisProjectsFromDisk
            // 
            this.MenuItem_installStatisProjectsFromDisk.Name = "MenuItem_installStatisProjectsFromDisk";
            this.MenuItem_installStatisProjectsFromDisk.Size = new System.Drawing.Size(398, 30);
            this.MenuItem_installStatisProjectsFromDisk.Text = "从磁盘目录安装全部方案(&D)";
            this.MenuItem_installStatisProjectsFromDisk.Click += new System.EventHandler(this.MenuItem_installStatisProjectsFromDisk_Click);
            // 
            // MenuItem_updateStatisProjectsFromDisk
            // 
            this.MenuItem_updateStatisProjectsFromDisk.Name = "MenuItem_updateStatisProjectsFromDisk";
            this.MenuItem_updateStatisProjectsFromDisk.Size = new System.Drawing.Size(398, 30);
            this.MenuItem_updateStatisProjectsFromDisk.Text = "从磁盘目录检查更新全部方案(&P)";
            this.MenuItem_updateStatisProjectsFromDisk.Click += new System.EventHandler(this.MenuItem_updateStatisProjectsFromDisk_Click);
            // 
            // MenuItem_separator_function2
            // 
            this.MenuItem_separator_function2.Name = "MenuItem_separator_function2";
            this.MenuItem_separator_function2.Size = new System.Drawing.Size(305, 6);
            // 
            // MenuItem_chatForm
            // 
            this.MenuItem_chatForm.Name = "MenuItem_chatForm";
            this.MenuItem_chatForm.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_chatForm.Text = "聊天(&C)";
            this.MenuItem_chatForm.Click += new System.EventHandler(this.MenuItem_chatForm_Click);
            // 
            // MenuItem_messageForm
            // 
            this.MenuItem_messageForm.Name = "MenuItem_messageForm";
            this.MenuItem_messageForm.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_messageForm.Text = "消息(&M)";
            this.MenuItem_messageForm.Click += new System.EventHandler(this.MenuItem_messageForm_Click);
            // 
            // MenuItem_openReservationListForm
            // 
            this.MenuItem_openReservationListForm.Name = "MenuItem_openReservationListForm";
            this.MenuItem_openReservationListForm.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_openReservationListForm.Text = "预约响应(&R)";
            this.MenuItem_openReservationListForm.Click += new System.EventHandler(this.MenuItem_openReservationListForm_Click);
            // 
            // toolStripSeparator30
            // 
            this.toolStripSeparator30.Name = "toolStripSeparator30";
            this.toolStripSeparator30.Size = new System.Drawing.Size(305, 6);
            // 
            // MenuItem_systemManagement
            // 
            this.MenuItem_systemManagement.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openClockForm,
            this.MenuItem_openCalendarForm,
            this.toolStripSeparator8,
            this.MenuItem_openBatchTaskForm,
            this.MenuItem_openOperLogForm,
            this.MenuItem_openManagerForm,
            this.MenuItem_openUserForm,
            this.MenuItem_channelForm,
            this.toolStripSeparator9,
            this.MenuItem_openTestForm,
            this.MenuItem_openUrgentChargingForm,
            this.MenuItem_operCheckBorrowInfoForm});
            this.MenuItem_systemManagement.Name = "MenuItem_systemManagement";
            this.MenuItem_systemManagement.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_systemManagement.Text = "系统维护";
            // 
            // MenuItem_openClockForm
            // 
            this.MenuItem_openClockForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openClockForm.Image")));
            this.MenuItem_openClockForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openClockForm.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.MenuItem_openClockForm.Name = "MenuItem_openClockForm";
            this.MenuItem_openClockForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openClockForm.Text = "时钟窗(&C)";
            this.MenuItem_openClockForm.Click += new System.EventHandler(this.MenuItem_openClockForm_Click);
            // 
            // MenuItem_openCalendarForm
            // 
            this.MenuItem_openCalendarForm.Enabled = false;
            this.MenuItem_openCalendarForm.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_openCalendarForm.Image")));
            this.MenuItem_openCalendarForm.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_openCalendarForm.Name = "MenuItem_openCalendarForm";
            this.MenuItem_openCalendarForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openCalendarForm.Text = "日历窗(&C)";
            this.MenuItem_openCalendarForm.Click += new System.EventHandler(this.MenuItem_openCalendarForm_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(239, 6);
            // 
            // MenuItem_openBatchTaskForm
            // 
            this.MenuItem_openBatchTaskForm.Name = "MenuItem_openBatchTaskForm";
            this.MenuItem_openBatchTaskForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openBatchTaskForm.Text = "批处理任务窗(&T)";
            this.MenuItem_openBatchTaskForm.Click += new System.EventHandler(this.MenuItem_openBatchTaskForm_Click);
            // 
            // MenuItem_openOperLogForm
            // 
            this.MenuItem_openOperLogForm.Name = "MenuItem_openOperLogForm";
            this.MenuItem_openOperLogForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openOperLogForm.Text = "日志窗(&L)";
            this.MenuItem_openOperLogForm.Click += new System.EventHandler(this.MenuItem_openOperLogForm_Click);
            // 
            // MenuItem_openManagerForm
            // 
            this.MenuItem_openManagerForm.Name = "MenuItem_openManagerForm";
            this.MenuItem_openManagerForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openManagerForm.Text = "系统管理窗(&M)";
            this.MenuItem_openManagerForm.Click += new System.EventHandler(this.MenuItem_openManagerForm_Click);
            // 
            // MenuItem_openUserForm
            // 
            this.MenuItem_openUserForm.Name = "MenuItem_openUserForm";
            this.MenuItem_openUserForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openUserForm.Text = "用户窗(&U)";
            this.MenuItem_openUserForm.Click += new System.EventHandler(this.MenuItem_openUserForm_Click);
            // 
            // MenuItem_channelForm
            // 
            this.MenuItem_channelForm.Name = "MenuItem_channelForm";
            this.MenuItem_channelForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_channelForm.Text = "通道管理窗(&C)";
            this.MenuItem_channelForm.Click += new System.EventHandler(this.MenuItem_channelForm_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(239, 6);
            // 
            // MenuItem_openTestForm
            // 
            this.MenuItem_openTestForm.Name = "MenuItem_openTestForm";
            this.MenuItem_openTestForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openTestForm.Text = "测试窗(&T)";
            this.MenuItem_openTestForm.Click += new System.EventHandler(this.MenuItem_openTestForm_Click);
            // 
            // MenuItem_openUrgentChargingForm
            // 
            this.MenuItem_openUrgentChargingForm.Name = "MenuItem_openUrgentChargingForm";
            this.MenuItem_openUrgentChargingForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_openUrgentChargingForm.Text = "应急出纳窗(&U)";
            this.MenuItem_openUrgentChargingForm.Click += new System.EventHandler(this.MenuItem_openUrgentChargingForm_Click);
            // 
            // MenuItem_operCheckBorrowInfoForm
            // 
            this.MenuItem_operCheckBorrowInfoForm.Name = "MenuItem_operCheckBorrowInfoForm";
            this.MenuItem_operCheckBorrowInfoForm.Size = new System.Drawing.Size(242, 30);
            this.MenuItem_operCheckBorrowInfoForm.Text = "检查借还信息窗(&C)";
            this.MenuItem_operCheckBorrowInfoForm.Click += new System.EventHandler(this.MenuItem_operCheckBorrowInfoForm_Click);
            // 
            // MenuItem_recoverUrgentLog
            // 
            this.MenuItem_recoverUrgentLog.Enabled = false;
            this.MenuItem_recoverUrgentLog.Name = "MenuItem_recoverUrgentLog";
            this.MenuItem_recoverUrgentLog.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_recoverUrgentLog.Text = "恢复应急日志到服务器(&R)...";
            this.MenuItem_recoverUrgentLog.Click += new System.EventHandler(this.MenuItem_recoverUrgentLog_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(305, 6);
            // 
            // ToolStripMenuItem_clearCache
            // 
            this.ToolStripMenuItem_clearCache.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_clearCfgCache,
            this.MenuItem_clearSummaryCache,
            this.MenuItem_clearDatabaseInfoCatch});
            this.ToolStripMenuItem_clearCache.Name = "ToolStripMenuItem_clearCache";
            this.ToolStripMenuItem_clearCache.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_clearCache.Text = "清除缓存";
            // 
            // MenuItem_clearCfgCache
            // 
            this.MenuItem_clearCfgCache.Name = "MenuItem_clearCfgCache";
            this.MenuItem_clearCfgCache.Size = new System.Drawing.Size(302, 30);
            this.MenuItem_clearCfgCache.Text = "清除配置文件本地缓存(&C)";
            this.MenuItem_clearCfgCache.Click += new System.EventHandler(this.MenuItem_clearCfgCache_Click);
            // 
            // MenuItem_clearSummaryCache
            // 
            this.MenuItem_clearSummaryCache.Name = "MenuItem_clearSummaryCache";
            this.MenuItem_clearSummaryCache.Size = new System.Drawing.Size(302, 30);
            this.MenuItem_clearSummaryCache.Text = "清除书目摘要本地缓存(&M)";
            this.MenuItem_clearSummaryCache.Click += new System.EventHandler(this.MenuItem_clearSummaryCache_Click);
            // 
            // MenuItem_clearDatabaseInfoCatch
            // 
            this.MenuItem_clearDatabaseInfoCatch.Name = "MenuItem_clearDatabaseInfoCatch";
            this.MenuItem_clearDatabaseInfoCatch.Size = new System.Drawing.Size(302, 30);
            this.MenuItem_clearDatabaseInfoCatch.Text = "刷新数据库信息缓存(&D)";
            this.MenuItem_clearDatabaseInfoCatch.Click += new System.EventHandler(this.MenuItem_clearDatabaseInfoCatch_Click);
            // 
            // MenuItem_reLogin
            // 
            this.MenuItem_reLogin.Name = "MenuItem_reLogin";
            this.MenuItem_reLogin.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_reLogin.Text = "重新登录(&L) ...";
            this.MenuItem_reLogin.Click += new System.EventHandler(this.MenuItem_reLogin_Click);
            // 
            // MenuItem_logout
            // 
            this.MenuItem_logout.Name = "MenuItem_logout";
            this.MenuItem_logout.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_logout.Text = "登出(&O)";
            // 
            // toolStripSeparator29
            // 
            this.toolStripSeparator29.Name = "toolStripSeparator29";
            this.toolStripSeparator29.Size = new System.Drawing.Size(305, 6);
            // 
            // MenuItem_initFingerprintCache
            // 
            this.MenuItem_initFingerprintCache.Name = "MenuItem_initFingerprintCache";
            this.MenuItem_initFingerprintCache.Size = new System.Drawing.Size(308, 30);
            this.MenuItem_initFingerprintCache.Text = "初始化指纹缓存(&I)";
            this.MenuItem_initFingerprintCache.Click += new System.EventHandler(this.MenuItem_initFingerprintCache_Click);
            // 
            // MenuItem_batch
            // 
            this.MenuItem_batch.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openQuickChangeEntityForm_1,
            this.MenuItem_openQuickChangeBiblioForm,
            this.MenuItem_batchOrder,
            this.toolStripSeparator16,
            this.MenuItem_itemHandover,
            this.MenuItem_printOrder,
            this.MenuItem_accept,
            this.MenuItem_printAccept,
            this.MenuItem_printClaim,
            this.MenuItem_printAccountBook,
            this.MenuItem_printBindingList,
            this.MenuItem_inventory,
            this.MenuItem_importExport,
            this.MenuItem_importFromOrderDistributeExcelFile,
            this.toolStripSeparator17,
            this.MenuItem_openTestSearch});
            this.MenuItem_batch.Name = "MenuItem_batch";
            this.MenuItem_batch.Size = new System.Drawing.Size(99, 28);
            this.MenuItem_batch.Text = "批处理(&B)";
            // 
            // MenuItem_openQuickChangeEntityForm_1
            // 
            this.MenuItem_openQuickChangeEntityForm_1.Name = "MenuItem_openQuickChangeEntityForm_1";
            this.MenuItem_openQuickChangeEntityForm_1.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_openQuickChangeEntityForm_1.Text = "批修改册(&I)";
            this.MenuItem_openQuickChangeEntityForm_1.Click += new System.EventHandler(this.MenuItem_openQuickChangeEntityForm_Click);
            // 
            // MenuItem_openQuickChangeBiblioForm
            // 
            this.MenuItem_openQuickChangeBiblioForm.Name = "MenuItem_openQuickChangeBiblioForm";
            this.MenuItem_openQuickChangeBiblioForm.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_openQuickChangeBiblioForm.Text = "批修改书目(&B)";
            this.MenuItem_openQuickChangeBiblioForm.Click += new System.EventHandler(this.MenuItem_openQuickChangeBiblioForm_Click);
            // 
            // MenuItem_batchOrder
            // 
            this.MenuItem_batchOrder.Name = "MenuItem_batchOrder";
            this.MenuItem_batchOrder.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_batchOrder.Text = "批订购";
            this.MenuItem_batchOrder.Click += new System.EventHandler(this.MenuItem_batchOrder_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(330, 6);
            // 
            // MenuItem_itemHandover
            // 
            this.MenuItem_itemHandover.Name = "MenuItem_itemHandover";
            this.MenuItem_itemHandover.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_itemHandover.Text = "典藏移交(&D)";
            this.MenuItem_itemHandover.Click += new System.EventHandler(this.MenuItem_handover_Click);
            // 
            // MenuItem_printOrder
            // 
            this.MenuItem_printOrder.Name = "MenuItem_printOrder";
            this.MenuItem_printOrder.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_printOrder.Text = "打印订单(&O)";
            this.MenuItem_printOrder.Click += new System.EventHandler(this.MenuItem_printOrder_Click);
            // 
            // MenuItem_accept
            // 
            this.MenuItem_accept.Name = "MenuItem_accept";
            this.MenuItem_accept.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_accept.Text = "验收(&A)";
            this.MenuItem_accept.Click += new System.EventHandler(this.MenuItem_accept_Click);
            // 
            // MenuItem_printAccept
            // 
            this.MenuItem_printAccept.Name = "MenuItem_printAccept";
            this.MenuItem_printAccept.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_printAccept.Text = "打印验收单(&P)";
            this.MenuItem_printAccept.Click += new System.EventHandler(this.MenuItem_printAccept_Click);
            // 
            // MenuItem_printClaim
            // 
            this.MenuItem_printClaim.Name = "MenuItem_printClaim";
            this.MenuItem_printClaim.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_printClaim.Text = "打印催询单(&C)";
            this.MenuItem_printClaim.Click += new System.EventHandler(this.MenuItem_printClaim_Click);
            // 
            // MenuItem_printAccountBook
            // 
            this.MenuItem_printAccountBook.Name = "MenuItem_printAccountBook";
            this.MenuItem_printAccountBook.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_printAccountBook.Text = "打印财产账(&A)";
            this.MenuItem_printAccountBook.Click += new System.EventHandler(this.MenuItem_printAccountBook_Click);
            // 
            // MenuItem_printBindingList
            // 
            this.MenuItem_printBindingList.Name = "MenuItem_printBindingList";
            this.MenuItem_printBindingList.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_printBindingList.Text = "打印装订单(&B)";
            this.MenuItem_printBindingList.Click += new System.EventHandler(this.MenuItem_printBindingList_Click);
            // 
            // MenuItem_inventory
            // 
            this.MenuItem_inventory.Name = "MenuItem_inventory";
            this.MenuItem_inventory.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_inventory.Text = "盘点(&V)";
            this.MenuItem_inventory.Click += new System.EventHandler(this.MenuItem_inventory_Click);
            // 
            // MenuItem_importExport
            // 
            this.MenuItem_importExport.Name = "MenuItem_importExport";
            this.MenuItem_importExport.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_importExport.Text = "从书目转储文件导入(&I)";
            this.MenuItem_importExport.Click += new System.EventHandler(this.MenuItem_importExport_Click);
            // 
            // MenuItem_importFromOrderDistributeExcelFile
            // 
            this.MenuItem_importFromOrderDistributeExcelFile.Name = "MenuItem_importFromOrderDistributeExcelFile";
            this.MenuItem_importFromOrderDistributeExcelFile.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_importFromOrderDistributeExcelFile.Text = "从订购去向 Excel 文件导入(&D)";
            this.MenuItem_importFromOrderDistributeExcelFile.Click += new System.EventHandler(this.MenuItem_importFromOrderDistributeExcelFile_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(330, 6);
            // 
            // MenuItem_openTestSearch
            // 
            this.MenuItem_openTestSearch.Name = "MenuItem_openTestSearch";
            this.MenuItem_openTestSearch.Size = new System.Drawing.Size(333, 30);
            this.MenuItem_openTestSearch.Text = "测试检索窗(&T)";
            this.MenuItem_openTestSearch.Click += new System.EventHandler(this.MenuItem_openTestSearch_Click);
            // 
            // MenuItem_window
            // 
            this.MenuItem_window.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_tileHorizontal,
            this.MenuItem_tileVertical,
            this.MenuItem_cascade,
            this.MenuItem_arrangeIcons,
            this.MenuItem_closeAllMdiWindows,
            this.toolStripSeparator24,
            this.MenuItem_displayFixPanel});
            this.MenuItem_window.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_window.Name = "MenuItem_window";
            this.MenuItem_window.Size = new System.Drawing.Size(88, 28);
            this.MenuItem_window.Text = "窗口(&W)";
            // 
            // MenuItem_tileHorizontal
            // 
            this.MenuItem_tileHorizontal.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_tileHorizontal.Image")));
            this.MenuItem_tileHorizontal.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_tileHorizontal.Name = "MenuItem_tileHorizontal";
            this.MenuItem_tileHorizontal.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_tileHorizontal.Text = "平铺[水平](&T)";
            this.MenuItem_tileHorizontal.Click += new System.EventHandler(this.MenuItem_mdi_arrange_Click);
            // 
            // MenuItem_tileVertical
            // 
            this.MenuItem_tileVertical.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_tileVertical.Name = "MenuItem_tileVertical";
            this.MenuItem_tileVertical.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_tileVertical.Text = "平铺[垂直](&I)";
            this.MenuItem_tileVertical.Click += new System.EventHandler(this.MenuItem_mdi_arrange_Click);
            // 
            // MenuItem_cascade
            // 
            this.MenuItem_cascade.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_cascade.Image")));
            this.MenuItem_cascade.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_cascade.Name = "MenuItem_cascade";
            this.MenuItem_cascade.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_cascade.Text = "层叠(&C)";
            this.MenuItem_cascade.Click += new System.EventHandler(this.MenuItem_mdi_arrange_Click);
            // 
            // MenuItem_arrangeIcons
            // 
            this.MenuItem_arrangeIcons.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_arrangeIcons.Name = "MenuItem_arrangeIcons";
            this.MenuItem_arrangeIcons.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_arrangeIcons.Text = "排列图标(&A)";
            this.MenuItem_arrangeIcons.Click += new System.EventHandler(this.MenuItem_mdi_arrange_Click);
            // 
            // MenuItem_closeAllMdiWindows
            // 
            this.MenuItem_closeAllMdiWindows.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_closeAllMdiWindows.Name = "MenuItem_closeAllMdiWindows";
            this.MenuItem_closeAllMdiWindows.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_closeAllMdiWindows.Text = "关闭全部 MDI 窗口(&A)";
            this.MenuItem_closeAllMdiWindows.Click += new System.EventHandler(this.MenuItem_closeAllMdiWindows_Click);
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            this.toolStripSeparator24.Size = new System.Drawing.Size(269, 6);
            // 
            // MenuItem_displayFixPanel
            // 
            this.MenuItem_displayFixPanel.Checked = true;
            this.MenuItem_displayFixPanel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MenuItem_displayFixPanel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_displayFixPanel.Name = "MenuItem_displayFixPanel";
            this.MenuItem_displayFixPanel.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_displayFixPanel.Text = "固定面板(&F)";
            this.MenuItem_displayFixPanel.Click += new System.EventHandler(this.MenuItem_displayFixPanel_Click);
            // 
            // MenuItem_ui
            // 
            this.MenuItem_ui.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_font,
            this.MenuItem_restoreDefaultFont});
            this.MenuItem_ui.Name = "MenuItem_ui";
            this.MenuItem_ui.Size = new System.Drawing.Size(75, 28);
            this.MenuItem_ui.Text = "外观(&I)";
            // 
            // MenuItem_font
            // 
            this.MenuItem_font.Image = ((System.Drawing.Image)(resources.GetObject("MenuItem_font.Image")));
            this.MenuItem_font.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuItem_font.Name = "MenuItem_font";
            this.MenuItem_font.Size = new System.Drawing.Size(244, 30);
            this.MenuItem_font.Text = "字体(&F)...";
            this.MenuItem_font.Click += new System.EventHandler(this.MenuItem_font_Click);
            // 
            // MenuItem_restoreDefaultFont
            // 
            this.MenuItem_restoreDefaultFont.Name = "MenuItem_restoreDefaultFont";
            this.MenuItem_restoreDefaultFont.Size = new System.Drawing.Size(244, 30);
            this.MenuItem_restoreDefaultFont.Text = "恢复为缺省字体(&D)";
            this.MenuItem_restoreDefaultFont.Click += new System.EventHandler(this.MenuItem_restoreDefaultFont_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_configuration,
            this.toolStripSeparator15,
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder,
            this.toolStripSeparator11,
            this.MenuItem_resetSerialCode,
            this.MenuItem_utility,
            this.toolStripSeparator20,
            this.menuItem_packageErrorLog,
            this.menuItem_updateDp2circulation,
            this.MenuItem_createGreenApplication,
            this.MenuItem_upgradeFromDisk,
            this.MenuItem_refreshLibraryUID,
            this.toolStripSeparator31,
            this.MenuItem_copyright});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(84, 28);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_configuration
            // 
            this.MenuItem_configuration.Name = "MenuItem_configuration";
            this.MenuItem_configuration.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_configuration.Text = "参数配置(&C)...";
            this.MenuItem_configuration.Click += new System.EventHandler(this.MenuItem_configuration_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(269, 6);
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_openUserFolder.Text = "打开用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_openDataFolder.Text = "打开数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_openProgramFolder.Text = "打开程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(269, 6);
            // 
            // MenuItem_resetSerialCode
            // 
            this.MenuItem_resetSerialCode.Name = "MenuItem_resetSerialCode";
            this.MenuItem_resetSerialCode.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_resetSerialCode.Text = "设置序列号(&R) ...";
            this.MenuItem_resetSerialCode.Click += new System.EventHandler(this.MenuItem_resetSerialCode_Click);
            // 
            // MenuItem_utility
            // 
            this.MenuItem_utility.Name = "MenuItem_utility";
            this.MenuItem_utility.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_utility.Text = "实用工具(&U)...";
            this.MenuItem_utility.Click += new System.EventHandler(this.MenuItem_utility_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(269, 6);
            // 
            // menuItem_packageErrorLog
            // 
            this.menuItem_packageErrorLog.Name = "menuItem_packageErrorLog";
            this.menuItem_packageErrorLog.Size = new System.Drawing.Size(272, 30);
            this.menuItem_packageErrorLog.Text = "打包错误日志(&L)...";
            this.menuItem_packageErrorLog.Click += new System.EventHandler(this.menuItem_packageErrorLog_Click);
            // 
            // menuItem_updateDp2circulation
            // 
            this.menuItem_updateDp2circulation.Name = "menuItem_updateDp2circulation";
            this.menuItem_updateDp2circulation.Size = new System.Drawing.Size(272, 30);
            this.menuItem_updateDp2circulation.Text = "更新 dp2Circulation";
            this.menuItem_updateDp2circulation.Visible = false;
            this.menuItem_updateDp2circulation.Click += new System.EventHandler(this.menuItem_updateDp2circulation_Click);
            // 
            // MenuItem_createGreenApplication
            // 
            this.MenuItem_createGreenApplication.Name = "MenuItem_createGreenApplication";
            this.MenuItem_createGreenApplication.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_createGreenApplication.Text = "创建备用绿色安装目录";
            this.MenuItem_createGreenApplication.Click += new System.EventHandler(this.MenuItem_createGreenApplication_Click);
            // 
            // MenuItem_upgradeFromDisk
            // 
            this.MenuItem_upgradeFromDisk.Name = "MenuItem_upgradeFromDisk";
            this.MenuItem_upgradeFromDisk.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_upgradeFromDisk.Text = "从磁盘升级(&U) ...";
            this.MenuItem_upgradeFromDisk.Click += new System.EventHandler(this.MenuItem_upgradeFromDisk_Click);
            // 
            // MenuItem_refreshLibraryUID
            // 
            this.MenuItem_refreshLibraryUID.Name = "MenuItem_refreshLibraryUID";
            this.MenuItem_refreshLibraryUID.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_refreshLibraryUID.Text = "刷新 LibraryUID";
            this.MenuItem_refreshLibraryUID.Click += new System.EventHandler(this.MenuItem_refreshLibraryUID_Click);
            // 
            // toolStripSeparator31
            // 
            this.toolStripSeparator31.Name = "toolStripSeparator31";
            this.toolStripSeparator31.Size = new System.Drawing.Size(269, 6);
            // 
            // MenuItem_copyright
            // 
            this.MenuItem_copyright.Name = "MenuItem_copyright";
            this.MenuItem_copyright.Size = new System.Drawing.Size(272, 30);
            this.MenuItem_copyright.Text = "关于(&A)...";
            this.MenuItem_copyright.Click += new System.EventHandler(this.MenuItem_about_Click);
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar_main,
            this.toolStripStatusLabel_main});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 525);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip_main.Size = new System.Drawing.Size(943, 30);
            this.statusStrip_main.TabIndex = 1;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripProgressBar_main
            // 
            this.toolStripProgressBar_main.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripProgressBar_main.Name = "toolStripProgressBar_main";
            this.toolStripProgressBar_main.Size = new System.Drawing.Size(150, 24);
            this.toolStripProgressBar_main.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(768, 25);
            this.toolStripStatusLabel_main.Spring = true;
            this.toolStripStatusLabel_main.Text = "欢迎使用 dp2Circulation ...";
            this.toolStripStatusLabel_main.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.AllowDrop = true;
            this.toolStrip_main.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip_main.ImageScalingSize = new System.Drawing.Size(64, 64);
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripDropDownButton_stopAll,
            this.toolStripSeparator2,
            this.toolStripDropDownButton_selectLibraryCode,
            this.toolStripLabel1,
            this.toolStripTextBox_barcode,
            this.toolStripButton_loadBarcode,
            this.toolStripDropDownButton_barcodeLoadStyle,
            this.toolStripSeparator10,
            this.toolButton_refresh,
            this.toolStripSeparator18,
            this.toolButton_borrow,
            this.toolButton_return,
            this.toolButton_verifyReturn,
            this.toolButton_renew,
            this.toolButton_lost,
            this.toolButton_amerce,
            this.toolButton_readerManage,
            this.toolButton_print});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 34);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_main.Size = new System.Drawing.Size(943, 32);
            this.toolStrip_main.TabIndex = 2;
            this.toolStrip_main.Text = "toolStrip1";
            this.toolStrip_main.DragDrop += new System.Windows.Forms.DragEventHandler(this.toolStrip_main_DragDrop);
            this.toolStrip_main.DragEnter += new System.Windows.Forms.DragEventHandler(this.toolStrip_main_DragEnter);
            // 
            // toolButton_stop
            // 
            this.toolButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_stop.Enabled = false;
            this.toolButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_stop.Image")));
            this.toolButton_stop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolButton_stop.Name = "toolButton_stop";
            this.toolButton_stop.Size = new System.Drawing.Size(26, 29);
            this.toolButton_stop.Text = "停止";
            this.toolButton_stop.Click += new System.EventHandler(this.toolButton_stop_Click);
            // 
            // toolStripDropDownButton_stopAll
            // 
            this.toolStripDropDownButton_stopAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_stopAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_stopAll});
            this.toolStripDropDownButton_stopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_stopAll.Name = "toolStripDropDownButton_stopAll";
            this.toolStripDropDownButton_stopAll.Size = new System.Drawing.Size(18, 29);
            this.toolStripDropDownButton_stopAll.Text = "停止全部";
            // 
            // ToolStripMenuItem_stopAll
            // 
            this.ToolStripMenuItem_stopAll.Name = "ToolStripMenuItem_stopAll";
            this.ToolStripMenuItem_stopAll.Size = new System.Drawing.Size(189, 30);
            this.ToolStripMenuItem_stopAll.Text = "停止全部(&A)";
            this.ToolStripMenuItem_stopAll.ToolTipText = "停止全部正在处理的操作";
            this.ToolStripMenuItem_stopAll.Click += new System.EventHandler(this.ToolStripMenuItem_stopAll_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 32);
            // 
            // toolStripDropDownButton_selectLibraryCode
            // 
            this.toolStripDropDownButton_selectLibraryCode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_selectLibraryCode.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_selectLibraryCode.Image")));
            this.toolStripDropDownButton_selectLibraryCode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_selectLibraryCode.Name = "toolStripDropDownButton_selectLibraryCode";
            this.toolStripDropDownButton_selectLibraryCode.Size = new System.Drawing.Size(100, 29);
            this.toolStripDropDownButton_selectLibraryCode.Text = "选择分馆";
            this.toolStripDropDownButton_selectLibraryCode.ToolTipText = "选择当前操作所针对的分馆";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(50, 29);
            this.toolStripLabel1.Text = "条码:";
            // 
            // toolStripTextBox_barcode
            // 
            this.toolStripTextBox_barcode.Name = "toolStripTextBox_barcode";
            this.toolStripTextBox_barcode.Size = new System.Drawing.Size(148, 32);
            this.toolStripTextBox_barcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripTextBox_barcode_KeyDown);
            // 
            // toolStripButton_loadBarcode
            // 
            this.toolStripButton_loadBarcode.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loadBarcode.Image")));
            this.toolStripButton_loadBarcode.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_loadBarcode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loadBarcode.Name = "toolStripButton_loadBarcode";
            this.toolStripButton_loadBarcode.Size = new System.Drawing.Size(66, 29);
            this.toolStripButton_loadBarcode.Text = "自动";
            this.toolStripButton_loadBarcode.ToolTipText = "装载条码";
            this.toolStripButton_loadBarcode.Click += new System.EventHandler(this.toolStripButton_loadBarcode_Click);
            // 
            // toolStripDropDownButton_barcodeLoadStyle
            // 
            this.toolStripDropDownButton_barcodeLoadStyle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.toolStripDropDownButton_barcodeLoadStyle.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_loadReaderInfo,
            this.ToolStripMenuItem_loadItemInfo,
            this.toolStripSeparator21,
            this.ToolStripMenuItem_autoLoadItemOrReader});
            this.toolStripDropDownButton_barcodeLoadStyle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_barcodeLoadStyle.Name = "toolStripDropDownButton_barcodeLoadStyle";
            this.toolStripDropDownButton_barcodeLoadStyle.Size = new System.Drawing.Size(18, 29);
            this.toolStripDropDownButton_barcodeLoadStyle.Text = "装载方式";
            this.toolStripDropDownButton_barcodeLoadStyle.ToolTipText = "装载方式";
            // 
            // ToolStripMenuItem_loadReaderInfo
            // 
            this.ToolStripMenuItem_loadReaderInfo.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.ToolStripMenuItem_loadReaderInfo.Name = "ToolStripMenuItem_loadReaderInfo";
            this.ToolStripMenuItem_loadReaderInfo.Size = new System.Drawing.Size(254, 30);
            this.ToolStripMenuItem_loadReaderInfo.Text = "装载读者记录";
            this.ToolStripMenuItem_loadReaderInfo.Click += new System.EventHandler(this.ToolStripMenuItem_loadReaderInfo_Click);
            // 
            // ToolStripMenuItem_loadItemInfo
            // 
            this.ToolStripMenuItem_loadItemInfo.Name = "ToolStripMenuItem_loadItemInfo";
            this.ToolStripMenuItem_loadItemInfo.Size = new System.Drawing.Size(254, 30);
            this.ToolStripMenuItem_loadItemInfo.Text = "装载册记录";
            this.ToolStripMenuItem_loadItemInfo.Click += new System.EventHandler(this.ToolStripMenuItem_loadItemInfo_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(251, 6);
            // 
            // ToolStripMenuItem_autoLoadItemOrReader
            // 
            this.ToolStripMenuItem_autoLoadItemOrReader.Checked = true;
            this.ToolStripMenuItem_autoLoadItemOrReader.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolStripMenuItem_autoLoadItemOrReader.Name = "ToolStripMenuItem_autoLoadItemOrReader";
            this.ToolStripMenuItem_autoLoadItemOrReader.Size = new System.Drawing.Size(254, 30);
            this.ToolStripMenuItem_autoLoadItemOrReader.Text = "自动判断类型并装载";
            this.ToolStripMenuItem_autoLoadItemOrReader.Click += new System.EventHandler(this.ToolStripMenuItem_autoLoadItemOrReader_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 32);
            // 
            // toolButton_refresh
            // 
            this.toolButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_refresh.Enabled = false;
            this.toolButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_refresh.Image")));
            this.toolButton_refresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_refresh.Name = "toolButton_refresh";
            this.toolButton_refresh.Size = new System.Drawing.Size(23, 29);
            this.toolButton_refresh.Text = "刷新 (F5)";
            this.toolButton_refresh.Click += new System.EventHandler(this.toolButton_refresh_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(6, 32);
            // 
            // toolButton_borrow
            // 
            this.toolButton_borrow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_borrow.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_borrow.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_borrow.Image")));
            this.toolButton_borrow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_borrow.Name = "toolButton_borrow";
            this.toolButton_borrow.Size = new System.Drawing.Size(32, 29);
            this.toolButton_borrow.Text = "借";
            this.toolButton_borrow.Click += new System.EventHandler(this.toolButton_borrow_Click);
            // 
            // toolButton_return
            // 
            this.toolButton_return.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_return.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_return.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_return.Image")));
            this.toolButton_return.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_return.Name = "toolButton_return";
            this.toolButton_return.Size = new System.Drawing.Size(32, 29);
            this.toolButton_return.Text = "还";
            this.toolButton_return.Click += new System.EventHandler(this.toolButton_return_Click);
            // 
            // toolButton_verifyReturn
            // 
            this.toolButton_verifyReturn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_verifyReturn.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_verifyReturn.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_verifyReturn.Image")));
            this.toolButton_verifyReturn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_verifyReturn.Name = "toolButton_verifyReturn";
            this.toolButton_verifyReturn.Size = new System.Drawing.Size(68, 29);
            this.toolButton_verifyReturn.Text = "验证还";
            this.toolButton_verifyReturn.Click += new System.EventHandler(this.toolButton_verifyReturn_Click);
            // 
            // toolButton_renew
            // 
            this.toolButton_renew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_renew.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_renew.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_renew.Image")));
            this.toolButton_renew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_renew.Name = "toolButton_renew";
            this.toolButton_renew.Size = new System.Drawing.Size(50, 29);
            this.toolButton_renew.Text = "续借";
            this.toolButton_renew.Click += new System.EventHandler(this.toolButton_renew_Click);
            // 
            // toolButton_lost
            // 
            this.toolButton_lost.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_lost.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_lost.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_lost.Image")));
            this.toolButton_lost.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_lost.Name = "toolButton_lost";
            this.toolButton_lost.Size = new System.Drawing.Size(50, 29);
            this.toolButton_lost.Text = "丢失";
            this.toolButton_lost.Click += new System.EventHandler(this.toolButton_lost_Click);
            // 
            // toolButton_amerce
            // 
            this.toolButton_amerce.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_amerce.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_amerce.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_amerce.Image")));
            this.toolButton_amerce.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_amerce.Name = "toolButton_amerce";
            this.toolButton_amerce.Size = new System.Drawing.Size(50, 29);
            this.toolButton_amerce.Text = "交费";
            this.toolButton_amerce.Click += new System.EventHandler(this.toolButton_amerce_Click);
            // 
            // toolButton_readerManage
            // 
            this.toolButton_readerManage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolButton_readerManage.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold);
            this.toolButton_readerManage.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_readerManage.Image")));
            this.toolButton_readerManage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_readerManage.Name = "toolButton_readerManage";
            this.toolButton_readerManage.Size = new System.Drawing.Size(50, 29);
            this.toolButton_readerManage.Text = "停借";
            this.toolButton_readerManage.Click += new System.EventHandler(this.toolButton_readerManage_Click);
            // 
            // toolButton_print
            // 
            this.toolButton_print.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolButton_print.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_print.Image")));
            this.toolButton_print.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButton_print.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_print.Name = "toolButton_print";
            this.toolButton_print.Size = new System.Drawing.Size(68, 29);
            this.toolButton_print.Text = "打印";
            this.toolButton_print.Click += new System.EventHandler(this.toolButton_print_Click);
            // 
            // panel_fixed
            // 
            this.panel_fixed.Controls.Add(this.tabControl_panelFixed);
            this.panel_fixed.Controls.Add(this.toolStrip_panelFixed);
            this.panel_fixed.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel_fixed.Location = new System.Drawing.Point(633, 66);
            this.panel_fixed.Name = "panel_fixed";
            this.panel_fixed.Size = new System.Drawing.Size(310, 459);
            this.panel_fixed.TabIndex = 5;
            // 
            // tabControl_panelFixed
            // 
            this.tabControl_panelFixed.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_panelFixed.ContextMenuStrip = this.contextMenuStrip_fixedPanel;
            this.tabControl_panelFixed.Controls.Add(this.tabPage_history);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_property);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_verifyResult);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_generateData);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_camera);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_accept);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_share);
            this.tabControl_panelFixed.Controls.Add(this.tabPage_browse);
            this.tabControl_panelFixed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_panelFixed.Location = new System.Drawing.Point(0, 25);
            this.tabControl_panelFixed.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_panelFixed.Name = "tabControl_panelFixed";
            this.tabControl_panelFixed.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_panelFixed.SelectedIndex = 0;
            this.tabControl_panelFixed.Size = new System.Drawing.Size(310, 434);
            this.tabControl_panelFixed.TabIndex = 1;
            this.tabControl_panelFixed.SelectedIndexChanged += new System.EventHandler(this.tabControl_panelFixed_SelectedIndexChanged);
            this.tabControl_panelFixed.SizeChanged += new System.EventHandler(this.tabControl_panelFixed_SizeChanged);
            // 
            // contextMenuStrip_fixedPanel
            // 
            this.contextMenuStrip_fixedPanel.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip_fixedPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_fixedPanel_clear});
            this.contextMenuStrip_fixedPanel.Name = "contextMenuStrip_fixedPanel";
            this.contextMenuStrip_fixedPanel.Size = new System.Drawing.Size(117, 32);
            this.contextMenuStrip_fixedPanel.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_fixedPanel_Opening);
            // 
            // toolStripMenuItem_fixedPanel_clear
            // 
            this.toolStripMenuItem_fixedPanel_clear.Name = "toolStripMenuItem_fixedPanel_clear";
            this.toolStripMenuItem_fixedPanel_clear.Size = new System.Drawing.Size(116, 28);
            this.toolStripMenuItem_fixedPanel_clear.Text = "清除";
            this.toolStripMenuItem_fixedPanel_clear.Click += new System.EventHandler(this.toolStripMenuItem_fixedPanel_clear_Click);
            // 
            // tabPage_history
            // 
            this.tabPage_history.Controls.Add(this.webBrowser_history);
            this.tabPage_history.Location = new System.Drawing.Point(4, 36);
            this.tabPage_history.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_history.Name = "tabPage_history";
            this.tabPage_history.Size = new System.Drawing.Size(302, 394);
            this.tabPage_history.TabIndex = 0;
            this.tabPage_history.Text = "操作历史";
            this.tabPage_history.UseVisualStyleBackColor = true;
            // 
            // webBrowser_history
            // 
            this.webBrowser_history.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_history.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_history.MinimumSize = new System.Drawing.Size(22, 24);
            this.webBrowser_history.Name = "webBrowser_history";
            this.webBrowser_history.Size = new System.Drawing.Size(302, 394);
            this.webBrowser_history.TabIndex = 0;
            // 
            // tabPage_property
            // 
            this.tabPage_property.Location = new System.Drawing.Point(4, 36);
            this.tabPage_property.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_property.Name = "tabPage_property";
            this.tabPage_property.Size = new System.Drawing.Size(302, 394);
            this.tabPage_property.TabIndex = 1;
            this.tabPage_property.Text = "属性";
            this.tabPage_property.UseVisualStyleBackColor = true;
            // 
            // tabPage_verifyResult
            // 
            this.tabPage_verifyResult.Location = new System.Drawing.Point(4, 36);
            this.tabPage_verifyResult.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_verifyResult.Name = "tabPage_verifyResult";
            this.tabPage_verifyResult.Size = new System.Drawing.Size(302, 394);
            this.tabPage_verifyResult.TabIndex = 2;
            this.tabPage_verifyResult.Text = "校验结果";
            this.tabPage_verifyResult.UseVisualStyleBackColor = true;
            // 
            // tabPage_generateData
            // 
            this.tabPage_generateData.Location = new System.Drawing.Point(4, 36);
            this.tabPage_generateData.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_generateData.Name = "tabPage_generateData";
            this.tabPage_generateData.Size = new System.Drawing.Size(302, 394);
            this.tabPage_generateData.TabIndex = 3;
            this.tabPage_generateData.Text = "创建数据";
            this.tabPage_generateData.UseVisualStyleBackColor = true;
            // 
            // tabPage_camera
            // 
            this.tabPage_camera.Location = new System.Drawing.Point(4, 36);
            this.tabPage_camera.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_camera.Name = "tabPage_camera";
            this.tabPage_camera.Size = new System.Drawing.Size(302, 394);
            this.tabPage_camera.TabIndex = 4;
            this.tabPage_camera.Text = "QR 识别";
            this.tabPage_camera.UseVisualStyleBackColor = true;
            // 
            // tabPage_accept
            // 
            this.tabPage_accept.Location = new System.Drawing.Point(4, 36);
            this.tabPage_accept.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_accept.Name = "tabPage_accept";
            this.tabPage_accept.Size = new System.Drawing.Size(302, 394);
            this.tabPage_accept.TabIndex = 5;
            this.tabPage_accept.Text = "验收";
            this.tabPage_accept.UseVisualStyleBackColor = true;
            this.tabPage_accept.Enter += new System.EventHandler(this.tabPage_accept_Enter);
            this.tabPage_accept.Leave += new System.EventHandler(this.tabPage_accept_Leave);
            // 
            // tabPage_share
            // 
            this.tabPage_share.Controls.Add(this.tableLayoutPanel_messageHub);
            this.tabPage_share.Location = new System.Drawing.Point(4, 36);
            this.tabPage_share.Name = "tabPage_share";
            this.tabPage_share.Size = new System.Drawing.Size(302, 394);
            this.tabPage_share.TabIndex = 6;
            this.tabPage_share.Text = "分享";
            this.tabPage_share.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_messageHub
            // 
            this.tableLayoutPanel_messageHub.ColumnCount = 1;
            this.tableLayoutPanel_messageHub.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_messageHub.Controls.Add(this.webBrowser_messageHub, 0, 0);
            this.tableLayoutPanel_messageHub.Controls.Add(this.toolStrip_messageHub, 0, 1);
            this.tableLayoutPanel_messageHub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_messageHub.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_messageHub.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_messageHub.Name = "tableLayoutPanel_messageHub";
            this.tableLayoutPanel_messageHub.RowCount = 3;
            this.tableLayoutPanel_messageHub.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_messageHub.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_messageHub.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel_messageHub.Size = new System.Drawing.Size(302, 394);
            this.tableLayoutPanel_messageHub.TabIndex = 2;
            // 
            // webBrowser_messageHub
            // 
            this.webBrowser_messageHub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_messageHub.Location = new System.Drawing.Point(4, 4);
            this.webBrowser_messageHub.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_messageHub.MinimumSize = new System.Drawing.Size(30, 30);
            this.webBrowser_messageHub.Name = "webBrowser_messageHub";
            this.webBrowser_messageHub.Size = new System.Drawing.Size(294, 355);
            this.webBrowser_messageHub.TabIndex = 0;
            // 
            // toolStrip_messageHub
            // 
            this.toolStrip_messageHub.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_messageHub.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip_messageHub.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_messageHub_userManage,
            this.toolStripButton_messageHub_relogin});
            this.toolStrip_messageHub.Location = new System.Drawing.Point(0, 363);
            this.toolStrip_messageHub.Name = "toolStrip_messageHub";
            this.toolStrip_messageHub.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_messageHub.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_messageHub.Size = new System.Drawing.Size(302, 31);
            this.toolStrip_messageHub.TabIndex = 1;
            this.toolStrip_messageHub.Text = "toolStrip1";
            // 
            // toolStripButton_messageHub_userManage
            // 
            this.toolStripButton_messageHub_userManage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_messageHub_userManage.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_messageHub_userManage.Image")));
            this.toolStripButton_messageHub_userManage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_messageHub_userManage.Name = "toolStripButton_messageHub_userManage";
            this.toolStripButton_messageHub_userManage.Size = new System.Drawing.Size(86, 28);
            this.toolStripButton_messageHub_userManage.Text = "用户管理";
            this.toolStripButton_messageHub_userManage.Click += new System.EventHandler(this.toolStripButton_messageHub_userManage_Click);
            // 
            // toolStripButton_messageHub_relogin
            // 
            this.toolStripButton_messageHub_relogin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_messageHub_relogin.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_messageHub_relogin.Image")));
            this.toolStripButton_messageHub_relogin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_messageHub_relogin.Name = "toolStripButton_messageHub_relogin";
            this.toolStripButton_messageHub_relogin.Size = new System.Drawing.Size(86, 28);
            this.toolStripButton_messageHub_relogin.Text = "重新登录";
            this.toolStripButton_messageHub_relogin.Click += new System.EventHandler(this.toolStripButton_messageHub_relogin_Click);
            // 
            // tabPage_browse
            // 
            this.tabPage_browse.Location = new System.Drawing.Point(4, 36);
            this.tabPage_browse.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_browse.Name = "tabPage_browse";
            this.tabPage_browse.Size = new System.Drawing.Size(302, 394);
            this.tabPage_browse.TabIndex = 7;
            this.tabPage_browse.Text = "浏览";
            this.tabPage_browse.UseVisualStyleBackColor = true;
            // 
            // toolStrip_panelFixed
            // 
            this.toolStrip_panelFixed.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStrip_panelFixed.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_panelFixed.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip_panelFixed.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_close});
            this.toolStrip_panelFixed.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_panelFixed.Name = "toolStrip_panelFixed";
            this.toolStrip_panelFixed.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_panelFixed.Size = new System.Drawing.Size(310, 25);
            this.toolStrip_panelFixed.TabIndex = 3;
            this.toolStrip_panelFixed.Text = "toolStrip1";
            // 
            // toolStripButton_close
            // 
            this.toolStripButton_close.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_close.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripButton_close.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_close.Image")));
            this.toolStripButton_close.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_close.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_close.Name = "toolStripButton_close";
            this.toolStripButton_close.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_close.Text = "隐藏面板";
            this.toolStripButton_close.Click += new System.EventHandler(this.toolStripButton_close_Click);
            // 
            // splitter_fixed
            // 
            this.splitter_fixed.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter_fixed.Location = new System.Drawing.Point(630, 66);
            this.splitter_fixed.Name = "splitter_fixed";
            this.splitter_fixed.Size = new System.Drawing.Size(3, 459);
            this.splitter_fixed.TabIndex = 6;
            this.splitter_fixed.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(943, 555);
            this.Controls.Add(this.splitter_fixed);
            this.Controls.Add(this.panel_fixed);
            this.Controls.Add(this.toolStrip_main);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.menuStrip_main);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip_main;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "dp2circulation V2 -- 内务";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MdiChildActivate += new System.EventHandler(this.MainForm_MdiChildActivate);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.menuStrip_main.ResumeLayout(false);
            this.menuStrip_main.PerformLayout();
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.panel_fixed.ResumeLayout(false);
            this.panel_fixed.PerformLayout();
            this.tabControl_panelFixed.ResumeLayout(false);
            this.contextMenuStrip_fixedPanel.ResumeLayout(false);
            this.tabPage_history.ResumeLayout(false);
            this.tabPage_share.ResumeLayout(false);
            this.tableLayoutPanel_messageHub.ResumeLayout(false);
            this.tableLayoutPanel_messageHub.PerformLayout();
            this.toolStrip_messageHub.ResumeLayout(false);
            this.toolStrip_messageHub.PerformLayout();
            this.toolStrip_panelFixed.ResumeLayout(false);
            this.toolStrip_panelFixed.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip_main;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_functionWindows;
        private System.Windows.Forms.StatusStrip statusStrip_main;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_main;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar_main;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openReaderInfoForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_window;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_tileHorizontal;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_tileVertical;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_cascade;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_arrangeIcons;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openReaderSearchForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openItemSearchForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_configuration;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        internal System.Windows.Forms.ToolStripTextBox toolStripTextBox_barcode;
        internal System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_barcodeLoadStyle;
        internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_loadReaderInfo;
        internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_loadItemInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openItemInfoForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openChargingForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openChangePasswordForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openAmerceForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openBiblioSearchForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openEntityForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openActivateForm;
        internal System.Windows.Forms.Panel panel_fixed;
        internal System.Windows.Forms.Splitter splitter_fixed;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripButton toolButton_borrow;
        private System.Windows.Forms.ToolStripButton toolButton_return;
        internal System.Windows.Forms.ToolStripButton toolButton_renew;
        internal System.Windows.Forms.ToolStripButton toolButton_lost;
        internal System.Windows.Forms.ToolStripButton toolButton_amerce;
        internal System.Windows.Forms.ToolStripButton toolButton_verifyReturn;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openReaderManageForm;
        internal System.Windows.Forms.ToolStripButton toolButton_readerManage;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_function;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_recoverUrgentLog;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_batch;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_itemHandover;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openQuickChangeEntityForm_1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_copyright;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        internal System.Windows.Forms.ToolStripButton toolButton_print;
        private System.Windows.Forms.ToolStripSeparator MenuItem_separator_function2;
        private System.Windows.Forms.TabPage tabPage_history;
        private System.Windows.Forms.WebBrowser webBrowser_history;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_ui;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_font;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_logout;
        private System.Windows.Forms.Timer timer_operHistory;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_printOrder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_accept;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_printAccept;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_autoLoadItemOrReader;
        private System.Windows.Forms.ToolStripButton toolStripButton_loadBarcode;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_printClaim;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_printAccountBook;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_printBindingList;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openQuickChangeBiblioForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openTestSearch;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_restoreDefaultFont;
        internal System.Windows.Forms.TabPage tabPage_property;
        internal System.Windows.Forms.TabControl tabControl_panelFixed;
        private System.Windows.Forms.TabPage tabPage_verifyResult;
        private System.Windows.Forms.ToolStrip toolStrip_panelFixed;
        private System.Windows.Forms.ToolStripButton toolStripButton_close;
        internal System.Windows.Forms.ToolStripButton toolButton_refresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openOrderSearchForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openIssueSearchForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openCommentSearchForm;
        private System.Windows.Forms.TabPage tabPage_generateData;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_utility;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_statisProjectManagement;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_installStatisProjects;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_updateStatisProjects;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_statisForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openOperLogStatisForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openReaderStatisForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openItemStatisForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openBiblioStatisForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openXmlStatisForm;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_openIso2709StatisForm;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_clearCache;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_clearCfgCache;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_clearSummaryCache;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearDatabaseInfoCatch;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_openFunctionWindow;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openSettlementForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openPassGateForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openZhongcihaoForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openCallNumberForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator23;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openChargingPrintManageForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openCardPrintForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openLabelPrintForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_systemManagement;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openClockForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openCalendarForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openBatchTaskForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openOperLogForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openManagerForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openTestForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUrgentChargingForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_operCheckBorrowInfoForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_runProject;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_projectManage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_stopAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_stopAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator21;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator22;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_installStatisProjectsFromDisk;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_updateStatisProjectsFromDisk;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator24;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_displayFixPanel;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openOrderStatisForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator28;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator25;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator26;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator27;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openInvoiceSearchForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator29;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_initFingerprintCache;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_fixedPanel;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_fixedPanel_clear;
        private System.Windows.Forms.TabPage tabPage_camera;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_closeAllMdiWindows;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_channelForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openQuickChargingForm;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_openReportForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_inventory;
        private System.Windows.Forms.TabPage tabPage_accept;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_messageForm;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator30;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetSerialCode;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_reLogin;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openEntityRegisterWizard;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openArrivedSearchForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openReservationListForm;
        private System.Windows.Forms.ToolStripMenuItem menuItem_packageErrorLog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator31;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_chatForm;
        private System.Windows.Forms.TabPage tabPage_share;
        private System.Windows.Forms.WebBrowser webBrowser_messageHub;
        private System.Windows.Forms.ToolStripMenuItem menuItem_updateDp2circulation;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_upgradeFromDisk;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openMarc856SearchForm;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_startAnotherDp2circulation;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_createGreenApplication;
        private System.Windows.Forms.ToolStrip toolStrip_messageHub;
        private System.Windows.Forms.ToolStripButton toolStripButton_messageHub_userManage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_messageHub;
        private System.Windows.Forms.ToolStripButton toolStripButton_messageHub_relogin;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_importExport;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_selectLibraryCode;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_refreshLibraryUID;
        private System.Windows.Forms.TabPage tabPage_browse;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_batchOrder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_importFromOrderDistributeExcelFile;
    }
}

