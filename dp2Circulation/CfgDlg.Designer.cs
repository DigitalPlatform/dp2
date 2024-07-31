﻿namespace dp2Circulation
{
    partial class CfgDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CfgDlg));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_server = new System.Windows.Forms.TabPage();
            this.button_server_fillPinyinUrl = new System.Windows.Forms.Button();
            this.button_server_fillAuthorNumberUrl = new System.Windows.Forms.Button();
            this.textBox_server_greenPackage = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.textBox_server_pinyin_gcatUrl = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox_server_authorNumber_gcatUrl = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.textBox_server_dp2LibraryServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip_server = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_server_setXeServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_server_setHongnibaServer = new System.Windows.Forms.ToolStripButton();
            this.tabPage_defaultAccount = new System.Windows.Forms.TabPage();
            this.checkBox_defaulAccount_savePasswordLong = new System.Windows.Forms.CheckBox();
            this.checkBox_defaultAccount_occurPerStart = new System.Windows.Forms.CheckBox();
            this.textBox_defaultAccount_location = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox_defaultAccount_isReader = new System.Windows.Forms.CheckBox();
            this.checkBox_defaulAccount_savePasswordShort = new System.Windows.Forms.CheckBox();
            this.textBox_defaultAccount_password = new System.Windows.Forms.TextBox();
            this.textBox_defaultAccount_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_cacheManage = new System.Windows.Forms.TabPage();
            this.buttondownloadIsbnXmlFile = new System.Windows.Forms.Button();
            this.button_downloadPinyinXmlFile = new System.Windows.Forms.Button();
            this.button_reloadUtilDbProperties = new System.Windows.Forms.Button();
            this.button_reloadReaderDbNames = new System.Windows.Forms.Button();
            this.button_reloadBiblioDbProperties = new System.Windows.Forms.Button();
            this.button_reloadBiblioDbFromInfos = new System.Windows.Forms.Button();
            this.button_clearValueTableCache = new System.Windows.Forms.Button();
            this.tabPage_charging = new System.Windows.Forms.TabPage();
            this.checkBox_charging_isbnBorrow = new System.Windows.Forms.CheckBox();
            this.groupBox_charging_selectItemDialog = new System.Windows.Forms.GroupBox();
            this.checkBox_charging_autoOperItemDialogSingleItem = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_noBorrowHistory = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_patronBarcodeAllowHanzi = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_speakNameWhenLoadReaderRecord = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_stopFillingWhenCloseInfoDlg = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_veifyReaderPassword = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_autoClearTextbox = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_autoSwitchReaderBarcode = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_noBiblioAndItem = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_greenInfoDlgNotOccur = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_autoUppercaseBarcode = new System.Windows.Forms.CheckBox();
            this.comboBox_charging_displayFormat = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBox_charging_doubleItemInputAsEnd = new System.Windows.Forms.CheckBox();
            this.checkBox_charging_verifyBarcode = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown_charging_infoDlgOpacity = new System.Windows.Forms.NumericUpDown();
            this.checkBox_charging_force = new System.Windows.Forms.CheckBox();
            this.tabPage_quickCharging = new System.Windows.Forms.TabPage();
            this.checkBox_quickCharging_faceInputMultipleHits = new System.Windows.Forms.CheckBox();
            this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds = new System.Windows.Forms.NumericUpDown();
            this.label43 = new System.Windows.Forms.Label();
            this.checkBox_quickCharging_allowFreeSequence = new System.Windows.Forms.CheckBox();
            this.comboBox_quickCharging_displayFormat = new System.Windows.Forms.ComboBox();
            this.comboBox_quickCharging_stateSpeak = new System.Windows.Forms.ComboBox();
            this.label34 = new System.Windows.Forms.Label();
            this.comboBox_quickCharging_displayStyle = new System.Windows.Forms.ComboBox();
            this.label30 = new System.Windows.Forms.Label();
            this.checkBox_quickCharging_logOperTime = new System.Windows.Forms.CheckBox();
            this.checkBox_quickCharging_isbnBorrow = new System.Windows.Forms.CheckBox();
            this.groupBox_quickCharging_selectItemDialog = new System.Windows.Forms.GroupBox();
            this.checkBox_quickCharging_autoOperItemDialogSingleItem = new System.Windows.Forms.CheckBox();
            this.label27 = new System.Windows.Forms.Label();
            this.checkBox_quickCharging_speakBookTitle = new System.Windows.Forms.CheckBox();
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord = new System.Windows.Forms.CheckBox();
            this.checkBox_quickCharging_noBorrowHistory = new System.Windows.Forms.CheckBox();
            this.checkBox_quickCharging_verifyBarcode = new System.Windows.Forms.CheckBox();
            this.tabPage_itemManagement = new System.Windows.Forms.TabPage();
            this.label_forceVerifyDataComment = new System.Windows.Forms.Label();
            this.textBox_itemManagement_maxPicWidth = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.checkBox_itemManagement_displayOtherLibraryItem = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_linkedRecordReadonly = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_showItemQuickInputPanel = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_showQueryPanel = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_verifyDataWhenSaving = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_searchDupWhenSaving = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_cataloging = new System.Windows.Forms.CheckBox();
            this.checkBox_itemManagement_verifyItemBarcode = new System.Windows.Forms.CheckBox();
            this.tabPage_ui = new System.Windows.Forms.TabPage();
            this.textBox_ui_loginWelcomeText = new System.Windows.Forms.TextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.checkBox_ui_printLabelMode = new System.Windows.Forms.CheckBox();
            this.checkBox_ui_fixedPanelAnimationEnabled = new System.Windows.Forms.CheckBox();
            this.button_ui_getDefaultFont = new System.Windows.Forms.Button();
            this.textBox_ui_defaultFont = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.checkBox_ui_hideFixedPanel = new System.Windows.Forms.CheckBox();
            this.comboBox_ui_fixedPanelDock = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage_passgate = new System.Windows.Forms.TabPage();
            this.numericUpDown_passgate_maxListItemsCount = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage_search = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.checkBox_search_hideCommentMatchStyleAndDbName = new System.Windows.Forms.CheckBox();
            this.checkBox_search_commentPushFilling = new System.Windows.Forms.CheckBox();
            this.label19 = new System.Windows.Forms.Label();
            this.numericUpDown_search_maxCommentResultCount = new System.Windows.Forms.NumericUpDown();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBox_search_hideIssueMatchStyleAndDbName = new System.Windows.Forms.CheckBox();
            this.checkBox_search_issuePushFilling = new System.Windows.Forms.CheckBox();
            this.label18 = new System.Windows.Forms.Label();
            this.numericUpDown_search_maxIssueResultCount = new System.Windows.Forms.NumericUpDown();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBox_search_hideOrderMatchStyleAndDbName = new System.Windows.Forms.CheckBox();
            this.checkBox_search_orderPushFilling = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.numericUpDown_search_maxOrderResultCount = new System.Windows.Forms.NumericUpDown();
            this.checkBox_search_useExistDetailWindow = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox_search_itemFilterLibraryCode = new System.Windows.Forms.CheckBox();
            this.checkBox_search_hideItemMatchStyleAndDbName = new System.Windows.Forms.CheckBox();
            this.checkBox_search_itemPushFilling = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.numericUpDown_search_maxItemResultCount = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_search_hideReaderMatchStyle = new System.Windows.Forms.CheckBox();
            this.checkBox_search_readerPushFilling = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.numericUpDown_search_maxReaderResultCount = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label36 = new System.Windows.Forms.Label();
            this.numericUpDown_search_multiline_maxBiblioResultCount = new System.Windows.Forms.NumericUpDown();
            this.checkBox_search_biblioPushFilling = new System.Windows.Forms.CheckBox();
            this.checkBox_search_hideBiblioMatchStyle = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDown_search_maxBiblioResultCount = new System.Windows.Forms.NumericUpDown();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print_findProject = new System.Windows.Forms.Button();
            this.label_print_projectNameMessage = new System.Windows.Forms.Label();
            this.textBox_print_projectName = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.button_print_projectManage = new System.Windows.Forms.Button();
            this.checkBox_print_pausePrint = new System.Windows.Forms.CheckBox();
            this.comboBox_print_prnPort = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.toolStrip_print = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_managePrintScript = new System.Windows.Forms.ToolStripDropDownButton();
            this.MenuItem_print_editCharingPrintCs = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_print_editCharingPrintCsRef = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage_amerce = new System.Windows.Forms.TabPage();
            this.comboBox_amerce_layout = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.comboBox_amerce_interface = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tabPage_accept = new System.Windows.Forms.TabPage();
            this.checkBox_accept_singleClickLoadDetail = new System.Windows.Forms.CheckBox();
            this.tabPage_cardReader = new System.Windows.Forms.TabPage();
            this.groupBox_rfidTest = new System.Windows.Forms.GroupBox();
            this.checkBox_rfidTest_returnPostUndoEAS = new System.Windows.Forms.CheckBox();
            this.checkBox_rfidTest_returnAPI = new System.Windows.Forms.CheckBox();
            this.checkBox_rfidTest_returnPreEAS = new System.Windows.Forms.CheckBox();
            this.checkBox_rfidTest_borrowEAS = new System.Windows.Forms.CheckBox();
            this.groupBox_rfidReader = new System.Windows.Forms.GroupBox();
            this.comboBox_rfid_tagCachePolicy = new System.Windows.Forms.ComboBox();
            this.label_rfid_tagCachePolicy = new System.Windows.Forms.Label();
            this.groupBox_uhf = new System.Windows.Forms.GroupBox();
            this.label42 = new System.Windows.Forms.Label();
            this.numericUpDown_rfid_inventoryIdleSeconds = new System.Windows.Forms.NumericUpDown();
            this.checkBox_uhf_onlyEpcCharging = new System.Windows.Forms.CheckBox();
            this.label41 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.numericUpDown_uhf_rssi = new System.Windows.Forms.NumericUpDown();
            this.checkedComboBox_uhf_elements = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label_uhf_elements = new System.Windows.Forms.Label();
            this.checkBox_uhf_bookTagWriteUserBank = new System.Windows.Forms.CheckBox();
            this.checkBox_uhf_warningWhenDataFormatMismatch = new System.Windows.Forms.CheckBox();
            this.comboBox_uhf_dataFormat = new System.Windows.Forms.ComboBox();
            this.label39 = new System.Windows.Forms.Label();
            this.button_cardReader_setRfidUrlDefaultValue = new System.Windows.Forms.Button();
            this.textBox_cardReader_rfidCenterUrl = new System.Windows.Forms.TextBox();
            this.groupBox_idcardReader = new System.Windows.Forms.GroupBox();
            this.button_cardReader_setIdcardUrlDefaultValue = new System.Windows.Forms.Button();
            this.textBox_cardReader_idcardReaderUrl = new System.Windows.Forms.TextBox();
            this.tabPage_patron = new System.Windows.Forms.TabPage();
            this.checkBox_patron_disableBioKeyboardSimulation = new System.Windows.Forms.CheckBox();
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation = new System.Windows.Forms.CheckBox();
            this.checkBox_patron_autoRetryReaderCard = new System.Windows.Forms.CheckBox();
            this.checkBox_patron_verifyBarcode = new System.Windows.Forms.CheckBox();
            this.checkBox_patron_displaySetReaderBarcodeDialog = new System.Windows.Forms.CheckBox();
            this.tabPage_operLog = new System.Windows.Forms.TabPage();
            this.comboBox_operLog_level = new System.Windows.Forms.ComboBox();
            this.label25 = new System.Windows.Forms.Label();
            this.checkBox_operLog_autoCache = new System.Windows.Forms.CheckBox();
            this.button_operLog_clearCacheDirectory = new System.Windows.Forms.Button();
            this.checkBox_operLog_displayItemBorrowHistory = new System.Windows.Forms.CheckBox();
            this.checkBox_operLog_displayReaderBorrowHistory = new System.Windows.Forms.CheckBox();
            this.tabPage_global = new System.Windows.Forms.TabPage();
            this.checkBox_global_disableSpeak = new System.Windows.Forms.CheckBox();
            this.checkedComboBox_global_securityProtocol = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label37 = new System.Windows.Forms.Label();
            this.textBox_global_additionalLocations = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.checkBox_global_upperInputBarcode = new System.Windows.Forms.CheckBox();
            this.checkBox_global_saveOriginCoverImage = new System.Windows.Forms.CheckBox();
            this.label26 = new System.Windows.Forms.Label();
            this.checkBox_global_autoSelPinyin = new System.Windows.Forms.CheckBox();
            this.checkBox_global_displayScriptErrorDialog = new System.Windows.Forms.CheckBox();
            this.tabPage_fingerprint = new System.Windows.Forms.TabPage();
            this.groupBox_palmprintUrl = new System.Windows.Forms.GroupBox();
            this.button_fingerprint_setDefaultValue_new = new System.Windows.Forms.Button();
            this.button_palmprint_setDefaulValue = new System.Windows.Forms.Button();
            this.textBox_palmprint_readerUrl = new System.Windows.Forms.TextBox();
            this.groupBox_face = new System.Windows.Forms.GroupBox();
            this.checkBox_face_savePhotoWhileRegister = new System.Windows.Forms.CheckBox();
            this.linkLabel_installFaceCenter = new System.Windows.Forms.LinkLabel();
            this.button_face_setDefaultValue = new System.Windows.Forms.Button();
            this.textBox_face_readerUrl = new System.Windows.Forms.TextBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.textBox_fingerprint_password = new System.Windows.Forms.TextBox();
            this.textBox_fingerprint_userName = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.groupBox_fingerprint = new System.Windows.Forms.GroupBox();
            this.linkLabel_installFingerprintCenter = new System.Windows.Forms.LinkLabel();
            this.button_fingerprint_setDefaultValue = new System.Windows.Forms.Button();
            this.textBox_fingerprint_readerUrl = new System.Windows.Forms.TextBox();
            this.button_fingerprint_clearLocalCacheFiles = new System.Windows.Forms.Button();
            this.tabPage_labelPrint = new System.Windows.Forms.TabPage();
            this.comboBox_labelPrint_accessNoSource = new System.Windows.Forms.ComboBox();
            this.label28 = new System.Windows.Forms.Label();
            this.tabPage_message = new System.Windows.Forms.TabPage();
            this.groupBox_dp2mserver = new System.Windows.Forms.GroupBox();
            this.textBox_message_userName = new System.Windows.Forms.TextBox();
            this.label33 = new System.Windows.Forms.Label();
            this.button_message_setDefaultUrl = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.textBox_message_dp2MServerUrl = new System.Windows.Forms.TextBox();
            this.textBox_message_password = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.groupBox_message_compactShelf = new System.Windows.Forms.GroupBox();
            this.textBox_message_shelfAccount = new System.Windows.Forms.TextBox();
            this.label38 = new System.Windows.Forms.Label();
            this.label_message_shareBiblio_comment = new System.Windows.Forms.Label();
            this.checkBox_message_shareBiblio = new System.Windows.Forms.CheckBox();
            this.tabPage_z3950 = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.button_ucs_testUpload = new System.Windows.Forms.Button();
            this.textBox_ucs_databaseName = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.textBox_ucs_password = new System.Windows.Forms.TextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.textBox_ucs_userName = new System.Windows.Forms.TextBox();
            this.label45 = new System.Windows.Forms.Label();
            this.textBox_ucs_apiUrl = new System.Windows.Forms.TextBox();
            this.label44 = new System.Windows.Forms.Label();
            this.button_z3950_servers = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label_message_denysharebiblio = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_server.SuspendLayout();
            this.toolStrip_server.SuspendLayout();
            this.tabPage_defaultAccount.SuspendLayout();
            this.tabPage_cacheManage.SuspendLayout();
            this.tabPage_charging.SuspendLayout();
            this.groupBox_charging_selectItemDialog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_charging_infoDlgOpacity)).BeginInit();
            this.tabPage_quickCharging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds)).BeginInit();
            this.groupBox_quickCharging_selectItemDialog.SuspendLayout();
            this.tabPage_itemManagement.SuspendLayout();
            this.tabPage_ui.SuspendLayout();
            this.tabPage_passgate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_passgate_maxListItemsCount)).BeginInit();
            this.tabPage_search.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxCommentResultCount)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxIssueResultCount)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxOrderResultCount)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxItemResultCount)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxReaderResultCount)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_multiline_maxBiblioResultCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxBiblioResultCount)).BeginInit();
            this.tabPage_print.SuspendLayout();
            this.toolStrip_print.SuspendLayout();
            this.tabPage_amerce.SuspendLayout();
            this.tabPage_accept.SuspendLayout();
            this.tabPage_cardReader.SuspendLayout();
            this.groupBox_rfidTest.SuspendLayout();
            this.groupBox_rfidReader.SuspendLayout();
            this.groupBox_uhf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_rfid_inventoryIdleSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_uhf_rssi)).BeginInit();
            this.groupBox_idcardReader.SuspendLayout();
            this.tabPage_patron.SuspendLayout();
            this.tabPage_operLog.SuspendLayout();
            this.tabPage_global.SuspendLayout();
            this.tabPage_fingerprint.SuspendLayout();
            this.groupBox_palmprintUrl.SuspendLayout();
            this.groupBox_face.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox_fingerprint.SuspendLayout();
            this.tabPage_labelPrint.SuspendLayout();
            this.tabPage_message.SuspendLayout();
            this.groupBox_dp2mserver.SuspendLayout();
            this.groupBox_message_compactShelf.SuspendLayout();
            this.tabPage_z3950.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_server);
            this.tabControl_main.Controls.Add(this.tabPage_defaultAccount);
            this.tabControl_main.Controls.Add(this.tabPage_cacheManage);
            this.tabControl_main.Controls.Add(this.tabPage_charging);
            this.tabControl_main.Controls.Add(this.tabPage_quickCharging);
            this.tabControl_main.Controls.Add(this.tabPage_itemManagement);
            this.tabControl_main.Controls.Add(this.tabPage_ui);
            this.tabControl_main.Controls.Add(this.tabPage_passgate);
            this.tabControl_main.Controls.Add(this.tabPage_search);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Controls.Add(this.tabPage_amerce);
            this.tabControl_main.Controls.Add(this.tabPage_accept);
            this.tabControl_main.Controls.Add(this.tabPage_cardReader);
            this.tabControl_main.Controls.Add(this.tabPage_patron);
            this.tabControl_main.Controls.Add(this.tabPage_operLog);
            this.tabControl_main.Controls.Add(this.tabPage_global);
            this.tabControl_main.Controls.Add(this.tabPage_fingerprint);
            this.tabControl_main.Controls.Add(this.tabPage_labelPrint);
            this.tabControl_main.Controls.Add(this.tabPage_message);
            this.tabControl_main.Controls.Add(this.tabPage_z3950);
            this.tabControl_main.Location = new System.Drawing.Point(18, 17);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl_main.Multiline = true;
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(913, 548);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_server
            // 
            this.tabPage_server.AutoScroll = true;
            this.tabPage_server.Controls.Add(this.button_server_fillPinyinUrl);
            this.tabPage_server.Controls.Add(this.button_server_fillAuthorNumberUrl);
            this.tabPage_server.Controls.Add(this.textBox_server_greenPackage);
            this.tabPage_server.Controls.Add(this.label31);
            this.tabPage_server.Controls.Add(this.textBox_server_pinyin_gcatUrl);
            this.tabPage_server.Controls.Add(this.label20);
            this.tabPage_server.Controls.Add(this.textBox_server_authorNumber_gcatUrl);
            this.tabPage_server.Controls.Add(this.label14);
            this.tabPage_server.Controls.Add(this.textBox_server_dp2LibraryServerUrl);
            this.tabPage_server.Controls.Add(this.label1);
            this.tabPage_server.Controls.Add(this.toolStrip_server);
            this.tabPage_server.Location = new System.Drawing.Point(4, 85);
            this.tabPage_server.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_server.Name = "tabPage_server";
            this.tabPage_server.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_server.Size = new System.Drawing.Size(905, 459);
            this.tabPage_server.TabIndex = 0;
            this.tabPage_server.Text = " 服务器 ";
            this.tabPage_server.UseVisualStyleBackColor = true;
            // 
            // button_server_fillPinyinUrl
            // 
            this.button_server_fillPinyinUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_server_fillPinyinUrl.Location = new System.Drawing.Point(770, 310);
            this.button_server_fillPinyinUrl.Name = "button_server_fillPinyinUrl";
            this.button_server_fillPinyinUrl.Size = new System.Drawing.Size(105, 30);
            this.button_server_fillPinyinUrl.TabIndex = 10;
            this.button_server_fillPinyinUrl.Text = "常用值";
            this.button_server_fillPinyinUrl.UseVisualStyleBackColor = true;
            this.button_server_fillPinyinUrl.Click += new System.EventHandler(this.button_server_fillPinyinUrl_Click);
            // 
            // button_server_fillAuthorNumberUrl
            // 
            this.button_server_fillAuthorNumberUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_server_fillAuthorNumberUrl.Location = new System.Drawing.Point(770, 217);
            this.button_server_fillAuthorNumberUrl.Name = "button_server_fillAuthorNumberUrl";
            this.button_server_fillAuthorNumberUrl.Size = new System.Drawing.Size(105, 30);
            this.button_server_fillAuthorNumberUrl.TabIndex = 9;
            this.button_server_fillAuthorNumberUrl.Text = "常用值";
            this.button_server_fillAuthorNumberUrl.UseVisualStyleBackColor = true;
            this.button_server_fillAuthorNumberUrl.Click += new System.EventHandler(this.button_server_fillAuthorNumberUrl_Click);
            // 
            // textBox_server_greenPackage
            // 
            this.textBox_server_greenPackage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_greenPackage.Location = new System.Drawing.Point(17, 364);
            this.textBox_server_greenPackage.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_server_greenPackage.Name = "textBox_server_greenPackage";
            this.textBox_server_greenPackage.Size = new System.Drawing.Size(858, 31);
            this.textBox_server_greenPackage.TabIndex = 8;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(12, 336);
            this.label31.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(409, 21);
            this.label31.TabIndex = 7;
            this.label31.Text = "dp2Circulation 绿色安装包 服务器 URL:";
            // 
            // textBox_server_pinyin_gcatUrl
            // 
            this.textBox_server_pinyin_gcatUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_pinyin_gcatUrl.Location = new System.Drawing.Point(17, 271);
            this.textBox_server_pinyin_gcatUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_server_pinyin_gcatUrl.Name = "textBox_server_pinyin_gcatUrl";
            this.textBox_server_pinyin_gcatUrl.Size = new System.Drawing.Size(858, 31);
            this.textBox_server_pinyin_gcatUrl.TabIndex = 5;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(12, 243);
            this.label20.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(181, 21);
            this.label20.TabIndex = 4;
            this.label20.Text = "拼音 服务器 URL:";
            // 
            // textBox_server_authorNumber_gcatUrl
            // 
            this.textBox_server_authorNumber_gcatUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_authorNumber_gcatUrl.Location = new System.Drawing.Point(17, 178);
            this.textBox_server_authorNumber_gcatUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_server_authorNumber_gcatUrl.Name = "textBox_server_authorNumber_gcatUrl";
            this.textBox_server_authorNumber_gcatUrl.Size = new System.Drawing.Size(858, 31);
            this.textBox_server_authorNumber_gcatUrl.TabIndex = 3;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 150);
            this.label14.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(278, 21);
            this.label14.TabIndex = 2;
            this.label14.Text = "著者号码 GCAT 服务器 URL:";
            // 
            // textBox_server_dp2LibraryServerUrl
            // 
            this.textBox_server_dp2LibraryServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_dp2LibraryServerUrl.Location = new System.Drawing.Point(17, 40);
            this.textBox_server_dp2LibraryServerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_server_dp2LibraryServerUrl.Name = "textBox_server_dp2LibraryServerUrl";
            this.textBox_server_dp2LibraryServerUrl.Size = new System.Drawing.Size(858, 31);
            this.textBox_server_dp2LibraryServerUrl.TabIndex = 1;
            this.textBox_server_dp2LibraryServerUrl.TextChanged += new System.EventHandler(this.textBox_server_circulationServerUrl_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(249, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2Library 服务器 URL:";
            // 
            // toolStrip_server
            // 
            this.toolStrip_server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_server.AutoSize = false;
            this.toolStrip_server.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_server.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_server.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_server.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_server_setXeServer,
            this.toolStripSeparator1,
            this.toolStripButton_server_setHongnibaServer});
            this.toolStrip_server.Location = new System.Drawing.Point(17, 75);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Size = new System.Drawing.Size(862, 44);
            this.toolStrip_server.TabIndex = 6;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_server_setXeServer
            // 
            this.toolStripButton_server_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setXeServer.Name = "toolStripButton_server_setXeServer";
            this.toolStripButton_server_setXeServer.Size = new System.Drawing.Size(142, 38);
            this.toolStripButton_server_setXeServer.Text = "单机版服务器";
            this.toolStripButton_server_setXeServer.ToolTipText = "设为单机版服务器";
            this.toolStripButton_server_setXeServer.Click += new System.EventHandler(this.toolStripButton_server_setXeServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripButton_server_setHongnibaServer
            // 
            this.toolStripButton_server_setHongnibaServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setHongnibaServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setHongnibaServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setHongnibaServer.Name = "toolStripButton_server_setHongnibaServer";
            this.toolStripButton_server_setHongnibaServer.Size = new System.Drawing.Size(231, 38);
            this.toolStripButton_server_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_server_setHongnibaServer_Click);
            // 
            // tabPage_defaultAccount
            // 
            this.tabPage_defaultAccount.AutoScroll = true;
            this.tabPage_defaultAccount.Controls.Add(this.checkBox_defaulAccount_savePasswordLong);
            this.tabPage_defaultAccount.Controls.Add(this.checkBox_defaultAccount_occurPerStart);
            this.tabPage_defaultAccount.Controls.Add(this.textBox_defaultAccount_location);
            this.tabPage_defaultAccount.Controls.Add(this.label4);
            this.tabPage_defaultAccount.Controls.Add(this.checkBox_defaultAccount_isReader);
            this.tabPage_defaultAccount.Controls.Add(this.checkBox_defaulAccount_savePasswordShort);
            this.tabPage_defaultAccount.Controls.Add(this.textBox_defaultAccount_password);
            this.tabPage_defaultAccount.Controls.Add(this.textBox_defaultAccount_userName);
            this.tabPage_defaultAccount.Controls.Add(this.label3);
            this.tabPage_defaultAccount.Controls.Add(this.label2);
            this.tabPage_defaultAccount.Location = new System.Drawing.Point(4, 85);
            this.tabPage_defaultAccount.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_defaultAccount.Name = "tabPage_defaultAccount";
            this.tabPage_defaultAccount.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_defaultAccount.Size = new System.Drawing.Size(905, 459);
            this.tabPage_defaultAccount.TabIndex = 1;
            this.tabPage_defaultAccount.Text = "默认帐户 ";
            this.tabPage_defaultAccount.UseVisualStyleBackColor = true;
            // 
            // checkBox_defaulAccount_savePasswordLong
            // 
            this.checkBox_defaulAccount_savePasswordLong.Location = new System.Drawing.Point(18, 317);
            this.checkBox_defaulAccount_savePasswordLong.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_defaulAccount_savePasswordLong.Name = "checkBox_defaulAccount_savePasswordLong";
            this.checkBox_defaulAccount_savePasswordLong.Size = new System.Drawing.Size(286, 33);
            this.checkBox_defaulAccount_savePasswordLong.TabIndex = 9;
            this.checkBox_defaulAccount_savePasswordLong.Text = "长期保存密码(&L)";
            this.checkBox_defaulAccount_savePasswordLong.CheckedChanged += new System.EventHandler(this.checkBox_defaulAccount_savePasswordLong_CheckedChanged);
            // 
            // checkBox_defaultAccount_occurPerStart
            // 
            this.checkBox_defaultAccount_occurPerStart.AutoSize = true;
            this.checkBox_defaultAccount_occurPerStart.Location = new System.Drawing.Point(18, 360);
            this.checkBox_defaultAccount_occurPerStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_defaultAccount_occurPerStart.Name = "checkBox_defaultAccount_occurPerStart";
            this.checkBox_defaultAccount_occurPerStart.Size = new System.Drawing.Size(384, 25);
            this.checkBox_defaultAccount_occurPerStart.TabIndex = 8;
            this.checkBox_defaultAccount_occurPerStart.Text = "每次启动程序时均出现登录对话框(&S)";
            this.checkBox_defaultAccount_occurPerStart.UseVisualStyleBackColor = true;
            // 
            // textBox_defaultAccount_location
            // 
            this.textBox_defaultAccount_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_defaultAccount_location.Location = new System.Drawing.Point(193, 248);
            this.textBox_defaultAccount_location.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_defaultAccount_location.Name = "textBox_defaultAccount_location";
            this.textBox_defaultAccount_location.Size = new System.Drawing.Size(283, 31);
            this.textBox_defaultAccount_location.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(15, 254);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(166, 31);
            this.label4.TabIndex = 6;
            this.label4.Text = "工作台号(&W)：";
            // 
            // checkBox_defaultAccount_isReader
            // 
            this.checkBox_defaultAccount_isReader.AutoSize = true;
            this.checkBox_defaultAccount_isReader.Location = new System.Drawing.Point(193, 87);
            this.checkBox_defaultAccount_isReader.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_defaultAccount_isReader.Name = "checkBox_defaultAccount_isReader";
            this.checkBox_defaultAccount_isReader.Size = new System.Drawing.Size(111, 25);
            this.checkBox_defaultAccount_isReader.TabIndex = 2;
            this.checkBox_defaultAccount_isReader.Text = "读者(&R)";
            this.checkBox_defaultAccount_isReader.UseVisualStyleBackColor = true;
            // 
            // checkBox_defaulAccount_savePasswordShort
            // 
            this.checkBox_defaulAccount_savePasswordShort.Location = new System.Drawing.Point(193, 185);
            this.checkBox_defaulAccount_savePasswordShort.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_defaulAccount_savePasswordShort.Name = "checkBox_defaulAccount_savePasswordShort";
            this.checkBox_defaulAccount_savePasswordShort.Size = new System.Drawing.Size(286, 33);
            this.checkBox_defaulAccount_savePasswordShort.TabIndex = 5;
            this.checkBox_defaulAccount_savePasswordShort.Text = "短期保存密码(&S)";
            // 
            // textBox_defaultAccount_password
            // 
            this.textBox_defaultAccount_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_defaultAccount_password.Location = new System.Drawing.Point(193, 138);
            this.textBox_defaultAccount_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_defaultAccount_password.Name = "textBox_defaultAccount_password";
            this.textBox_defaultAccount_password.PasswordChar = '*';
            this.textBox_defaultAccount_password.Size = new System.Drawing.Size(283, 31);
            this.textBox_defaultAccount_password.TabIndex = 4;
            // 
            // textBox_defaultAccount_userName
            // 
            this.textBox_defaultAccount_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_defaultAccount_userName.Location = new System.Drawing.Point(193, 38);
            this.textBox_defaultAccount_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_defaultAccount_userName.Name = "textBox_defaultAccount_userName";
            this.textBox_defaultAccount_userName.Size = new System.Drawing.Size(283, 31);
            this.textBox_defaultAccount_userName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 143);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 31);
            this.label3.TabIndex = 3;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(15, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 31);
            this.label2.TabIndex = 0;
            this.label2.Text = "用户名(&U)：";
            // 
            // tabPage_cacheManage
            // 
            this.tabPage_cacheManage.AutoScroll = true;
            this.tabPage_cacheManage.Controls.Add(this.buttondownloadIsbnXmlFile);
            this.tabPage_cacheManage.Controls.Add(this.button_downloadPinyinXmlFile);
            this.tabPage_cacheManage.Controls.Add(this.button_reloadUtilDbProperties);
            this.tabPage_cacheManage.Controls.Add(this.button_reloadReaderDbNames);
            this.tabPage_cacheManage.Controls.Add(this.button_reloadBiblioDbProperties);
            this.tabPage_cacheManage.Controls.Add(this.button_reloadBiblioDbFromInfos);
            this.tabPage_cacheManage.Controls.Add(this.button_clearValueTableCache);
            this.tabPage_cacheManage.Location = new System.Drawing.Point(4, 85);
            this.tabPage_cacheManage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_cacheManage.Name = "tabPage_cacheManage";
            this.tabPage_cacheManage.Size = new System.Drawing.Size(905, 459);
            this.tabPage_cacheManage.TabIndex = 2;
            this.tabPage_cacheManage.Text = " 缓存管理 ";
            this.tabPage_cacheManage.UseVisualStyleBackColor = true;
            // 
            // buttondownloadIsbnXmlFile
            // 
            this.buttondownloadIsbnXmlFile.Location = new System.Drawing.Point(26, 357);
            this.buttondownloadIsbnXmlFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttondownloadIsbnXmlFile.Name = "buttondownloadIsbnXmlFile";
            this.buttondownloadIsbnXmlFile.Size = new System.Drawing.Size(389, 38);
            this.buttondownloadIsbnXmlFile.TabIndex = 6;
            this.buttondownloadIsbnXmlFile.Text = "下载 RangeMessage.xml 文件到本地(&I)";
            this.buttondownloadIsbnXmlFile.UseVisualStyleBackColor = true;
            this.buttondownloadIsbnXmlFile.Click += new System.EventHandler(this.buttondownloadIsbnXmlFile_Click);
            // 
            // button_downloadPinyinXmlFile
            // 
            this.button_downloadPinyinXmlFile.Location = new System.Drawing.Point(26, 310);
            this.button_downloadPinyinXmlFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_downloadPinyinXmlFile.Name = "button_downloadPinyinXmlFile";
            this.button_downloadPinyinXmlFile.Size = new System.Drawing.Size(389, 38);
            this.button_downloadPinyinXmlFile.TabIndex = 5;
            this.button_downloadPinyinXmlFile.Text = "下载 pinyin.xml 文件到本地(&P)";
            this.button_downloadPinyinXmlFile.UseVisualStyleBackColor = true;
            this.button_downloadPinyinXmlFile.Click += new System.EventHandler(this.button_downloadPinyinXmlFile_Click);
            // 
            // button_reloadUtilDbProperties
            // 
            this.button_reloadUtilDbProperties.Location = new System.Drawing.Point(26, 220);
            this.button_reloadUtilDbProperties.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_reloadUtilDbProperties.Name = "button_reloadUtilDbProperties";
            this.button_reloadUtilDbProperties.Size = new System.Drawing.Size(389, 38);
            this.button_reloadUtilDbProperties.TabIndex = 4;
            this.button_reloadUtilDbProperties.Text = "重新获得实体库属性列表(&B)";
            this.button_reloadUtilDbProperties.UseVisualStyleBackColor = true;
            this.button_reloadUtilDbProperties.Click += new System.EventHandler(this.button_reloadUtilDbProperties_Click);
            // 
            // button_reloadReaderDbNames
            // 
            this.button_reloadReaderDbNames.Location = new System.Drawing.Point(26, 171);
            this.button_reloadReaderDbNames.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_reloadReaderDbNames.Name = "button_reloadReaderDbNames";
            this.button_reloadReaderDbNames.Size = new System.Drawing.Size(389, 38);
            this.button_reloadReaderDbNames.TabIndex = 3;
            this.button_reloadReaderDbNames.Text = "重新获得读者库名列表(&R)";
            this.button_reloadReaderDbNames.UseVisualStyleBackColor = true;
            this.button_reloadReaderDbNames.Click += new System.EventHandler(this.button_reloadReaderDbProperties_Click);
            // 
            // button_reloadBiblioDbProperties
            // 
            this.button_reloadBiblioDbProperties.Location = new System.Drawing.Point(26, 124);
            this.button_reloadBiblioDbProperties.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_reloadBiblioDbProperties.Name = "button_reloadBiblioDbProperties";
            this.button_reloadBiblioDbProperties.Size = new System.Drawing.Size(389, 38);
            this.button_reloadBiblioDbProperties.TabIndex = 2;
            this.button_reloadBiblioDbProperties.Text = "重新获得书目库属性列表(&B)";
            this.button_reloadBiblioDbProperties.UseVisualStyleBackColor = true;
            this.button_reloadBiblioDbProperties.Click += new System.EventHandler(this.button_reloadBiblioDbNames_Click);
            // 
            // button_reloadBiblioDbFromInfos
            // 
            this.button_reloadBiblioDbFromInfos.Location = new System.Drawing.Point(26, 77);
            this.button_reloadBiblioDbFromInfos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_reloadBiblioDbFromInfos.Name = "button_reloadBiblioDbFromInfos";
            this.button_reloadBiblioDbFromInfos.Size = new System.Drawing.Size(389, 38);
            this.button_reloadBiblioDbFromInfos.TabIndex = 1;
            this.button_reloadBiblioDbFromInfos.Text = "重新获得书目库检索途径列表(&F)";
            this.button_reloadBiblioDbFromInfos.UseVisualStyleBackColor = true;
            this.button_reloadBiblioDbFromInfos.Click += new System.EventHandler(this.button_reloadBiblioDbFromInfos_Click);
            // 
            // button_clearValueTableCache
            // 
            this.button_clearValueTableCache.Location = new System.Drawing.Point(26, 28);
            this.button_clearValueTableCache.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_clearValueTableCache.Name = "button_clearValueTableCache";
            this.button_clearValueTableCache.Size = new System.Drawing.Size(389, 38);
            this.button_clearValueTableCache.TabIndex = 0;
            this.button_clearValueTableCache.Text = "清除值列表缓存(&V)";
            this.button_clearValueTableCache.UseVisualStyleBackColor = true;
            this.button_clearValueTableCache.Click += new System.EventHandler(this.button_clearValueTableCache_Click);
            // 
            // tabPage_charging
            // 
            this.tabPage_charging.AutoScroll = true;
            this.tabPage_charging.Controls.Add(this.checkBox_charging_isbnBorrow);
            this.tabPage_charging.Controls.Add(this.groupBox_charging_selectItemDialog);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_noBorrowHistory);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_patronBarcodeAllowHanzi);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_speakNameWhenLoadReaderRecord);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_stopFillingWhenCloseInfoDlg);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_veifyReaderPassword);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_autoClearTextbox);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_autoSwitchReaderBarcode);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_noBiblioAndItem);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_greenInfoDlgNotOccur);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_autoUppercaseBarcode);
            this.tabPage_charging.Controls.Add(this.comboBox_charging_displayFormat);
            this.tabPage_charging.Controls.Add(this.label7);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_doubleItemInputAsEnd);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_verifyBarcode);
            this.tabPage_charging.Controls.Add(this.label5);
            this.tabPage_charging.Controls.Add(this.numericUpDown_charging_infoDlgOpacity);
            this.tabPage_charging.Controls.Add(this.checkBox_charging_force);
            this.tabPage_charging.Location = new System.Drawing.Point(4, 85);
            this.tabPage_charging.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_charging.Name = "tabPage_charging";
            this.tabPage_charging.Size = new System.Drawing.Size(905, 459);
            this.tabPage_charging.TabIndex = 3;
            this.tabPage_charging.Text = "出纳";
            this.tabPage_charging.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_isbnBorrow
            // 
            this.checkBox_charging_isbnBorrow.AutoSize = true;
            this.checkBox_charging_isbnBorrow.Location = new System.Drawing.Point(18, 695);
            this.checkBox_charging_isbnBorrow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_isbnBorrow.Name = "checkBox_charging_isbnBorrow";
            this.checkBox_charging_isbnBorrow.Size = new System.Drawing.Size(303, 25);
            this.checkBox_charging_isbnBorrow.TabIndex = 21;
            this.checkBox_charging_isbnBorrow.Text = "启用 ISBN 借书还书功能(&I)";
            this.checkBox_charging_isbnBorrow.UseVisualStyleBackColor = true;
            this.checkBox_charging_isbnBorrow.CheckedChanged += new System.EventHandler(this.checkBox_charging_isbnBorrow_CheckedChanged);
            // 
            // groupBox_charging_selectItemDialog
            // 
            this.groupBox_charging_selectItemDialog.Controls.Add(this.checkBox_charging_autoOperItemDialogSingleItem);
            this.groupBox_charging_selectItemDialog.Location = new System.Drawing.Point(18, 731);
            this.groupBox_charging_selectItemDialog.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_charging_selectItemDialog.Name = "groupBox_charging_selectItemDialog";
            this.groupBox_charging_selectItemDialog.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_charging_selectItemDialog.Size = new System.Drawing.Size(462, 175);
            this.groupBox_charging_selectItemDialog.TabIndex = 21;
            this.groupBox_charging_selectItemDialog.TabStop = false;
            this.groupBox_charging_selectItemDialog.Text = " 选择册记录对话框 ";
            // 
            // checkBox_charging_autoOperItemDialogSingleItem
            // 
            this.checkBox_charging_autoOperItemDialogSingleItem.AutoSize = true;
            this.checkBox_charging_autoOperItemDialogSingleItem.Location = new System.Drawing.Point(26, 49);
            this.checkBox_charging_autoOperItemDialogSingleItem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_autoOperItemDialogSingleItem.Name = "checkBox_charging_autoOperItemDialogSingleItem";
            this.checkBox_charging_autoOperItemDialogSingleItem.Size = new System.Drawing.Size(321, 25);
            this.checkBox_charging_autoOperItemDialogSingleItem.TabIndex = 19;
            this.checkBox_charging_autoOperItemDialogSingleItem.Text = "自动操作唯一可用的册记录(&A)";
            this.checkBox_charging_autoOperItemDialogSingleItem.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_noBorrowHistory
            // 
            this.checkBox_charging_noBorrowHistory.AutoSize = true;
            this.checkBox_charging_noBorrowHistory.Location = new System.Drawing.Point(18, 514);
            this.checkBox_charging_noBorrowHistory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_noBorrowHistory.Name = "checkBox_charging_noBorrowHistory";
            this.checkBox_charging_noBorrowHistory.Size = new System.Drawing.Size(321, 25);
            this.checkBox_charging_noBorrowHistory.TabIndex = 12;
            this.checkBox_charging_noBorrowHistory.Text = "读者信息中不显示借阅历史(&H)";
            this.checkBox_charging_noBorrowHistory.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_patronBarcodeAllowHanzi
            // 
            this.checkBox_charging_patronBarcodeAllowHanzi.AutoSize = true;
            this.checkBox_charging_patronBarcodeAllowHanzi.Location = new System.Drawing.Point(18, 350);
            this.checkBox_charging_patronBarcodeAllowHanzi.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_charging_patronBarcodeAllowHanzi.Name = "checkBox_charging_patronBarcodeAllowHanzi";
            this.checkBox_charging_patronBarcodeAllowHanzi.Size = new System.Drawing.Size(353, 25);
            this.checkBox_charging_patronBarcodeAllowHanzi.TabIndex = 8;
            this.checkBox_charging_patronBarcodeAllowHanzi.Text = "证条码号输入框允许输入汉字 (&H)";
            this.checkBox_charging_patronBarcodeAllowHanzi.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_speakNameWhenLoadReaderRecord
            // 
            this.checkBox_charging_speakNameWhenLoadReaderRecord.AutoSize = true;
            this.checkBox_charging_speakNameWhenLoadReaderRecord.Location = new System.Drawing.Point(18, 633);
            this.checkBox_charging_speakNameWhenLoadReaderRecord.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_charging_speakNameWhenLoadReaderRecord.Name = "checkBox_charging_speakNameWhenLoadReaderRecord";
            this.checkBox_charging_speakNameWhenLoadReaderRecord.Size = new System.Drawing.Size(437, 25);
            this.checkBox_charging_speakNameWhenLoadReaderRecord.TabIndex = 14;
            this.checkBox_charging_speakNameWhenLoadReaderRecord.Text = "朗读读者姓名，当装载读者记录的时刻 (&S)";
            this.checkBox_charging_speakNameWhenLoadReaderRecord.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_stopFillingWhenCloseInfoDlg
            // 
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.AutoSize = true;
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.Location = new System.Drawing.Point(18, 443);
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.Name = "checkBox_charging_stopFillingWhenCloseInfoDlg";
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.Size = new System.Drawing.Size(321, 25);
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.TabIndex = 10;
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.Text = "关闭信息窗时停止异步填充(&S)";
            this.checkBox_charging_stopFillingWhenCloseInfoDlg.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_veifyReaderPassword
            // 
            this.checkBox_charging_veifyReaderPassword.AutoSize = true;
            this.checkBox_charging_veifyReaderPassword.Location = new System.Drawing.Point(18, 577);
            this.checkBox_charging_veifyReaderPassword.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_charging_veifyReaderPassword.Name = "checkBox_charging_veifyReaderPassword";
            this.checkBox_charging_veifyReaderPassword.Size = new System.Drawing.Size(248, 25);
            this.checkBox_charging_veifyReaderPassword.TabIndex = 13;
            this.checkBox_charging_veifyReaderPassword.Text = "启用读者密码验证 (&P)";
            this.checkBox_charging_veifyReaderPassword.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_autoClearTextbox
            // 
            this.checkBox_charging_autoClearTextbox.AutoSize = true;
            this.checkBox_charging_autoClearTextbox.Location = new System.Drawing.Point(18, 313);
            this.checkBox_charging_autoClearTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_autoClearTextbox.Name = "checkBox_charging_autoClearTextbox";
            this.checkBox_charging_autoClearTextbox.Size = new System.Drawing.Size(279, 25);
            this.checkBox_charging_autoClearTextbox.TabIndex = 7;
            this.checkBox_charging_autoClearTextbox.Text = "自动清除输入框中内容(&C)";
            this.checkBox_charging_autoClearTextbox.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_autoSwitchReaderBarcode
            // 
            this.checkBox_charging_autoSwitchReaderBarcode.AutoSize = true;
            this.checkBox_charging_autoSwitchReaderBarcode.Location = new System.Drawing.Point(18, 208);
            this.checkBox_charging_autoSwitchReaderBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_autoSwitchReaderBarcode.Name = "checkBox_charging_autoSwitchReaderBarcode";
            this.checkBox_charging_autoSwitchReaderBarcode.Size = new System.Drawing.Size(531, 25);
            this.checkBox_charging_autoSwitchReaderBarcode.TabIndex = 4;
            this.checkBox_charging_autoSwitchReaderBarcode.Text = "在册条码号文本框中输入读者证条码号时自动切换(&I)";
            this.checkBox_charging_autoSwitchReaderBarcode.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_noBiblioAndItem
            // 
            this.checkBox_charging_noBiblioAndItem.AutoSize = true;
            this.checkBox_charging_noBiblioAndItem.Location = new System.Drawing.Point(18, 479);
            this.checkBox_charging_noBiblioAndItem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_noBiblioAndItem.Name = "checkBox_charging_noBiblioAndItem";
            this.checkBox_charging_noBiblioAndItem.Size = new System.Drawing.Size(258, 25);
            this.checkBox_charging_noBiblioAndItem.TabIndex = 11;
            this.checkBox_charging_noBiblioAndItem.Text = "不显示书目和册信息(&B)";
            this.checkBox_charging_noBiblioAndItem.UseVisualStyleBackColor = true;
            this.checkBox_charging_noBiblioAndItem.CheckedChanged += new System.EventHandler(this.checkBox_charging_noBiblioAndItem_CheckedChanged);
            // 
            // checkBox_charging_greenInfoDlgNotOccur
            // 
            this.checkBox_charging_greenInfoDlgNotOccur.AutoSize = true;
            this.checkBox_charging_greenInfoDlgNotOccur.Location = new System.Drawing.Point(18, 406);
            this.checkBox_charging_greenInfoDlgNotOccur.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_greenInfoDlgNotOccur.Name = "checkBox_charging_greenInfoDlgNotOccur";
            this.checkBox_charging_greenInfoDlgNotOccur.Size = new System.Drawing.Size(237, 25);
            this.checkBox_charging_greenInfoDlgNotOccur.TabIndex = 9;
            this.checkBox_charging_greenInfoDlgNotOccur.Text = "不出现绿色信息窗(&G)";
            this.checkBox_charging_greenInfoDlgNotOccur.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_autoUppercaseBarcode
            // 
            this.checkBox_charging_autoUppercaseBarcode.AutoSize = true;
            this.checkBox_charging_autoUppercaseBarcode.Location = new System.Drawing.Point(18, 243);
            this.checkBox_charging_autoUppercaseBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_autoUppercaseBarcode.Name = "checkBox_charging_autoUppercaseBarcode";
            this.checkBox_charging_autoUppercaseBarcode.Size = new System.Drawing.Size(384, 25);
            this.checkBox_charging_autoUppercaseBarcode.TabIndex = 5;
            this.checkBox_charging_autoUppercaseBarcode.Text = "自动把输入的条码字符串转为大写(&U)";
            this.checkBox_charging_autoUppercaseBarcode.UseVisualStyleBackColor = true;
            // 
            // comboBox_charging_displayFormat
            // 
            this.comboBox_charging_displayFormat.FormattingEnabled = true;
            this.comboBox_charging_displayFormat.Items.AddRange(new object[] {
            "HTML",
            "纯文本"});
            this.comboBox_charging_displayFormat.Location = new System.Drawing.Point(310, 24);
            this.comboBox_charging_displayFormat.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_charging_displayFormat.Name = "comboBox_charging_displayFormat";
            this.comboBox_charging_displayFormat.Size = new System.Drawing.Size(268, 29);
            this.comboBox_charging_displayFormat.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 30);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(138, 21);
            this.label7.TabIndex = 0;
            this.label7.Text = "显示格式(&D):";
            // 
            // checkBox_charging_doubleItemInputAsEnd
            // 
            this.checkBox_charging_doubleItemInputAsEnd.AutoSize = true;
            this.checkBox_charging_doubleItemInputAsEnd.Location = new System.Drawing.Point(18, 278);
            this.checkBox_charging_doubleItemInputAsEnd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_doubleItemInputAsEnd.Name = "checkBox_charging_doubleItemInputAsEnd";
            this.checkBox_charging_doubleItemInputAsEnd.Size = new System.Drawing.Size(426, 25);
            this.checkBox_charging_doubleItemInputAsEnd.TabIndex = 6;
            this.checkBox_charging_doubleItemInputAsEnd.Text = "连续输入相同册条码号被作为切换信号(&D)";
            this.checkBox_charging_doubleItemInputAsEnd.UseVisualStyleBackColor = true;
            // 
            // checkBox_charging_verifyBarcode
            // 
            this.checkBox_charging_verifyBarcode.AutoSize = true;
            this.checkBox_charging_verifyBarcode.Location = new System.Drawing.Point(18, 173);
            this.checkBox_charging_verifyBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_verifyBarcode.Name = "checkBox_charging_verifyBarcode";
            this.checkBox_charging_verifyBarcode.Size = new System.Drawing.Size(237, 25);
            this.checkBox_charging_verifyBarcode.TabIndex = 3;
            this.checkBox_charging_verifyBarcode.Text = "校验输入的条码号(&V)";
            this.checkBox_charging_verifyBarcode.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 117);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(264, 21);
            this.label5.TabIndex = 1;
            this.label5.Text = "信息对话框的不透明度(&O):";
            // 
            // numericUpDown_charging_infoDlgOpacity
            // 
            this.numericUpDown_charging_infoDlgOpacity.Location = new System.Drawing.Point(310, 115);
            this.numericUpDown_charging_infoDlgOpacity.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_charging_infoDlgOpacity.Name = "numericUpDown_charging_infoDlgOpacity";
            this.numericUpDown_charging_infoDlgOpacity.Size = new System.Drawing.Size(100, 31);
            this.numericUpDown_charging_infoDlgOpacity.TabIndex = 2;
            this.numericUpDown_charging_infoDlgOpacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_charging_infoDlgOpacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // checkBox_charging_force
            // 
            this.checkBox_charging_force.AutoSize = true;
            this.checkBox_charging_force.Location = new System.Drawing.Point(18, 73);
            this.checkBox_charging_force.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_charging_force.Name = "checkBox_charging_force";
            this.checkBox_charging_force.Size = new System.Drawing.Size(111, 25);
            this.checkBox_charging_force.TabIndex = 0;
            this.checkBox_charging_force.Text = "容错(&F)";
            this.checkBox_charging_force.UseVisualStyleBackColor = true;
            // 
            // tabPage_quickCharging
            // 
            this.tabPage_quickCharging.AutoScroll = true;
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_faceInputMultipleHits);
            this.tabPage_quickCharging.Controls.Add(this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds);
            this.tabPage_quickCharging.Controls.Add(this.label43);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_allowFreeSequence);
            this.tabPage_quickCharging.Controls.Add(this.comboBox_quickCharging_displayFormat);
            this.tabPage_quickCharging.Controls.Add(this.comboBox_quickCharging_stateSpeak);
            this.tabPage_quickCharging.Controls.Add(this.label34);
            this.tabPage_quickCharging.Controls.Add(this.comboBox_quickCharging_displayStyle);
            this.tabPage_quickCharging.Controls.Add(this.label30);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_logOperTime);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_isbnBorrow);
            this.tabPage_quickCharging.Controls.Add(this.groupBox_quickCharging_selectItemDialog);
            this.tabPage_quickCharging.Controls.Add(this.label27);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_speakBookTitle);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_speakNameWhenLoadReaderRecord);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_noBorrowHistory);
            this.tabPage_quickCharging.Controls.Add(this.checkBox_quickCharging_verifyBarcode);
            this.tabPage_quickCharging.Location = new System.Drawing.Point(4, 85);
            this.tabPage_quickCharging.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_quickCharging.Name = "tabPage_quickCharging";
            this.tabPage_quickCharging.Size = new System.Drawing.Size(905, 459);
            this.tabPage_quickCharging.TabIndex = 17;
            this.tabPage_quickCharging.Text = "快捷出纳";
            this.tabPage_quickCharging.UseVisualStyleBackColor = true;
            // 
            // checkBox_quickCharging_faceInputMultipleHits
            // 
            this.checkBox_quickCharging_faceInputMultipleHits.AutoSize = true;
            this.checkBox_quickCharging_faceInputMultipleHits.Location = new System.Drawing.Point(22, 368);
            this.checkBox_quickCharging_faceInputMultipleHits.Name = "checkBox_quickCharging_faceInputMultipleHits";
            this.checkBox_quickCharging_faceInputMultipleHits.Size = new System.Drawing.Size(300, 25);
            this.checkBox_quickCharging_faceInputMultipleHits.TabIndex = 16;
            this.checkBox_quickCharging_faceInputMultipleHits.Text = "人脸识别时允许命中多个(&M)";
            this.checkBox_quickCharging_faceInputMultipleHits.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds
            // 
            this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds.Location = new System.Drawing.Point(339, 333);
            this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds.Name = "numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds";
            this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds.Size = new System.Drawing.Size(145, 31);
            this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds.TabIndex = 15;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(18, 335);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(315, 21);
            this.label43.TabIndex = 14;
            this.label43.Text = "自动触发人脸识别前的延时秒数:";
            // 
            // checkBox_quickCharging_allowFreeSequence
            // 
            this.checkBox_quickCharging_allowFreeSequence.AutoSize = true;
            this.checkBox_quickCharging_allowFreeSequence.Location = new System.Drawing.Point(434, 124);
            this.checkBox_quickCharging_allowFreeSequence.Name = "checkBox_quickCharging_allowFreeSequence";
            this.checkBox_quickCharging_allowFreeSequence.Size = new System.Drawing.Size(321, 25);
            this.checkBox_quickCharging_allowFreeSequence.TabIndex = 13;
            this.checkBox_quickCharging_allowFreeSequence.Text = "借书时允许先输入册条码号(&F)";
            this.checkBox_quickCharging_allowFreeSequence.UseVisualStyleBackColor = true;
            // 
            // comboBox_quickCharging_displayFormat
            // 
            this.comboBox_quickCharging_displayFormat.FormattingEnabled = true;
            this.comboBox_quickCharging_displayFormat.Items.AddRange(new object[] {
            "HTML",
            "卡片"});
            this.comboBox_quickCharging_displayFormat.Location = new System.Drawing.Point(166, 24);
            this.comboBox_quickCharging_displayFormat.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_quickCharging_displayFormat.Name = "comboBox_quickCharging_displayFormat";
            this.comboBox_quickCharging_displayFormat.Size = new System.Drawing.Size(268, 29);
            this.comboBox_quickCharging_displayFormat.TabIndex = 1;
            // 
            // comboBox_quickCharging_stateSpeak
            // 
            this.comboBox_quickCharging_stateSpeak.FormattingEnabled = true;
            this.comboBox_quickCharging_stateSpeak.Items.AddRange(new object[] {
            "[不朗读]",
            "状态",
            "状态+内容"});
            this.comboBox_quickCharging_stateSpeak.Location = new System.Drawing.Point(166, 271);
            this.comboBox_quickCharging_stateSpeak.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_quickCharging_stateSpeak.Name = "comboBox_quickCharging_stateSpeak";
            this.comboBox_quickCharging_stateSpeak.Size = new System.Drawing.Size(268, 29);
            this.comboBox_quickCharging_stateSpeak.TabIndex = 9;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(18, 276);
            this.label34.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(138, 21);
            this.label34.TabIndex = 8;
            this.label34.Text = "朗读状态(&S):";
            // 
            // comboBox_quickCharging_displayStyle
            // 
            this.comboBox_quickCharging_displayStyle.FormattingEnabled = true;
            this.comboBox_quickCharging_displayStyle.Items.AddRange(new object[] {
            "dark",
            "light"});
            this.comboBox_quickCharging_displayStyle.Location = new System.Drawing.Point(166, 66);
            this.comboBox_quickCharging_displayStyle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_quickCharging_displayStyle.Name = "comboBox_quickCharging_displayStyle";
            this.comboBox_quickCharging_displayStyle.Size = new System.Drawing.Size(268, 29);
            this.comboBox_quickCharging_displayStyle.TabIndex = 3;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(18, 72);
            this.label30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(138, 21);
            this.label30.TabIndex = 2;
            this.label30.Text = "显示风格(&S):";
            // 
            // checkBox_quickCharging_logOperTime
            // 
            this.checkBox_quickCharging_logOperTime.AutoSize = true;
            this.checkBox_quickCharging_logOperTime.Location = new System.Drawing.Point(22, 657);
            this.checkBox_quickCharging_logOperTime.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_quickCharging_logOperTime.Name = "checkBox_quickCharging_logOperTime";
            this.checkBox_quickCharging_logOperTime.Size = new System.Drawing.Size(279, 25);
            this.checkBox_quickCharging_logOperTime.TabIndex = 12;
            this.checkBox_quickCharging_logOperTime.Text = "在日志中记载操作耗时(&L)";
            this.checkBox_quickCharging_logOperTime.UseVisualStyleBackColor = true;
            // 
            // checkBox_quickCharging_isbnBorrow
            // 
            this.checkBox_quickCharging_isbnBorrow.AutoSize = true;
            this.checkBox_quickCharging_isbnBorrow.Location = new System.Drawing.Point(22, 422);
            this.checkBox_quickCharging_isbnBorrow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_quickCharging_isbnBorrow.Name = "checkBox_quickCharging_isbnBorrow";
            this.checkBox_quickCharging_isbnBorrow.Size = new System.Drawing.Size(303, 25);
            this.checkBox_quickCharging_isbnBorrow.TabIndex = 10;
            this.checkBox_quickCharging_isbnBorrow.Text = "启用 ISBN 借书还书功能(&I)";
            this.checkBox_quickCharging_isbnBorrow.UseVisualStyleBackColor = true;
            this.checkBox_quickCharging_isbnBorrow.CheckedChanged += new System.EventHandler(this.checkBox_quickCharging_isbnBorrow_CheckedChanged);
            // 
            // groupBox_quickCharging_selectItemDialog
            // 
            this.groupBox_quickCharging_selectItemDialog.Controls.Add(this.checkBox_quickCharging_autoOperItemDialogSingleItem);
            this.groupBox_quickCharging_selectItemDialog.Location = new System.Drawing.Point(22, 464);
            this.groupBox_quickCharging_selectItemDialog.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_quickCharging_selectItemDialog.Name = "groupBox_quickCharging_selectItemDialog";
            this.groupBox_quickCharging_selectItemDialog.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_quickCharging_selectItemDialog.Size = new System.Drawing.Size(462, 175);
            this.groupBox_quickCharging_selectItemDialog.TabIndex = 11;
            this.groupBox_quickCharging_selectItemDialog.TabStop = false;
            this.groupBox_quickCharging_selectItemDialog.Text = " 选择册记录对话框 ";
            // 
            // checkBox_quickCharging_autoOperItemDialogSingleItem
            // 
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.AutoSize = true;
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.Location = new System.Drawing.Point(27, 49);
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.Name = "checkBox_quickCharging_autoOperItemDialogSingleItem";
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.Size = new System.Drawing.Size(321, 25);
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.TabIndex = 0;
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.Text = "自动操作唯一可用的册记录(&A)";
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(18, 30);
            this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(138, 21);
            this.label27.TabIndex = 0;
            this.label27.Text = "显示格式(&D):";
            // 
            // checkBox_quickCharging_speakBookTitle
            // 
            this.checkBox_quickCharging_speakBookTitle.AutoSize = true;
            this.checkBox_quickCharging_speakBookTitle.Location = new System.Drawing.Point(22, 234);
            this.checkBox_quickCharging_speakBookTitle.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_quickCharging_speakBookTitle.Name = "checkBox_quickCharging_speakBookTitle";
            this.checkBox_quickCharging_speakBookTitle.Size = new System.Drawing.Size(164, 25);
            this.checkBox_quickCharging_speakBookTitle.TabIndex = 7;
            this.checkBox_quickCharging_speakBookTitle.Text = "朗读书名 (&P)";
            this.checkBox_quickCharging_speakBookTitle.UseVisualStyleBackColor = true;
            // 
            // checkBox_quickCharging_speakNameWhenLoadReaderRecord
            // 
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.AutoSize = true;
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Location = new System.Drawing.Point(22, 196);
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Name = "checkBox_quickCharging_speakNameWhenLoadReaderRecord";
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Size = new System.Drawing.Size(437, 25);
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.TabIndex = 6;
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Text = "朗读读者姓名，当装载读者记录的时刻 (&S)";
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.UseVisualStyleBackColor = true;
            // 
            // checkBox_quickCharging_noBorrowHistory
            // 
            this.checkBox_quickCharging_noBorrowHistory.AutoSize = true;
            this.checkBox_quickCharging_noBorrowHistory.Location = new System.Drawing.Point(22, 159);
            this.checkBox_quickCharging_noBorrowHistory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_quickCharging_noBorrowHistory.Name = "checkBox_quickCharging_noBorrowHistory";
            this.checkBox_quickCharging_noBorrowHistory.Size = new System.Drawing.Size(321, 25);
            this.checkBox_quickCharging_noBorrowHistory.TabIndex = 5;
            this.checkBox_quickCharging_noBorrowHistory.Text = "读者信息中不显示借阅历史(&H)";
            this.checkBox_quickCharging_noBorrowHistory.UseVisualStyleBackColor = true;
            // 
            // checkBox_quickCharging_verifyBarcode
            // 
            this.checkBox_quickCharging_verifyBarcode.AutoSize = true;
            this.checkBox_quickCharging_verifyBarcode.Location = new System.Drawing.Point(22, 124);
            this.checkBox_quickCharging_verifyBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_quickCharging_verifyBarcode.Name = "checkBox_quickCharging_verifyBarcode";
            this.checkBox_quickCharging_verifyBarcode.Size = new System.Drawing.Size(237, 25);
            this.checkBox_quickCharging_verifyBarcode.TabIndex = 4;
            this.checkBox_quickCharging_verifyBarcode.Text = "校验输入的条码号(&V)";
            this.checkBox_quickCharging_verifyBarcode.UseVisualStyleBackColor = true;
            // 
            // tabPage_itemManagement
            // 
            this.tabPage_itemManagement.AutoScroll = true;
            this.tabPage_itemManagement.Controls.Add(this.label_forceVerifyDataComment);
            this.tabPage_itemManagement.Controls.Add(this.textBox_itemManagement_maxPicWidth);
            this.tabPage_itemManagement.Controls.Add(this.label23);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_displayOtherLibraryItem);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_linkedRecordReadonly);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_showItemQuickInputPanel);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_showQueryPanel);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_verifyDataWhenSaving);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_searchDupWhenSaving);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_cataloging);
            this.tabPage_itemManagement.Controls.Add(this.checkBox_itemManagement_verifyItemBarcode);
            this.tabPage_itemManagement.Location = new System.Drawing.Point(4, 85);
            this.tabPage_itemManagement.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_itemManagement.Name = "tabPage_itemManagement";
            this.tabPage_itemManagement.Size = new System.Drawing.Size(905, 459);
            this.tabPage_itemManagement.TabIndex = 5;
            this.tabPage_itemManagement.Text = "种册";
            this.tabPage_itemManagement.UseVisualStyleBackColor = true;
            // 
            // label_forceVerifyDataComment
            // 
            this.label_forceVerifyDataComment.AutoSize = true;
            this.label_forceVerifyDataComment.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_forceVerifyDataComment.Location = new System.Drawing.Point(389, 112);
            this.label_forceVerifyDataComment.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_forceVerifyDataComment.Name = "label_forceVerifyDataComment";
            this.label_forceVerifyDataComment.Size = new System.Drawing.Size(0, 21);
            this.label_forceVerifyDataComment.TabIndex = 10;
            // 
            // textBox_itemManagement_maxPicWidth
            // 
            this.textBox_itemManagement_maxPicWidth.Location = new System.Drawing.Point(423, 409);
            this.textBox_itemManagement_maxPicWidth.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_itemManagement_maxPicWidth.Name = "textBox_itemManagement_maxPicWidth";
            this.textBox_itemManagement_maxPicWidth.Size = new System.Drawing.Size(160, 31);
            this.textBox_itemManagement_maxPicWidth.TabIndex = 9;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(18, 415);
            this.label23.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(383, 21);
            this.label23.TabIndex = 8;
            this.label23.Text = "自动限定Paste的图片宽度[像素数](&W):";
            // 
            // checkBox_itemManagement_displayOtherLibraryItem
            // 
            this.checkBox_itemManagement_displayOtherLibraryItem.AutoSize = true;
            this.checkBox_itemManagement_displayOtherLibraryItem.Location = new System.Drawing.Point(17, 345);
            this.checkBox_itemManagement_displayOtherLibraryItem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_displayOtherLibraryItem.Name = "checkBox_itemManagement_displayOtherLibraryItem";
            this.checkBox_itemManagement_displayOtherLibraryItem.Size = new System.Drawing.Size(279, 25);
            this.checkBox_itemManagement_displayOtherLibraryItem.TabIndex = 7;
            this.checkBox_itemManagement_displayOtherLibraryItem.Text = "显示其他分馆的册记录(&O)";
            this.checkBox_itemManagement_displayOtherLibraryItem.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_linkedRecordReadonly
            // 
            this.checkBox_itemManagement_linkedRecordReadonly.AutoSize = true;
            this.checkBox_itemManagement_linkedRecordReadonly.Location = new System.Drawing.Point(17, 290);
            this.checkBox_itemManagement_linkedRecordReadonly.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_linkedRecordReadonly.Name = "checkBox_itemManagement_linkedRecordReadonly";
            this.checkBox_itemManagement_linkedRecordReadonly.Size = new System.Drawing.Size(342, 25);
            this.checkBox_itemManagement_linkedRecordReadonly.TabIndex = 6;
            this.checkBox_itemManagement_linkedRecordReadonly.Text = "副本书目记录显示为只读状态(&R)";
            this.checkBox_itemManagement_linkedRecordReadonly.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_showItemQuickInputPanel
            // 
            this.checkBox_itemManagement_showItemQuickInputPanel.AutoSize = true;
            this.checkBox_itemManagement_showItemQuickInputPanel.Location = new System.Drawing.Point(17, 240);
            this.checkBox_itemManagement_showItemQuickInputPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_showItemQuickInputPanel.Name = "checkBox_itemManagement_showItemQuickInputPanel";
            this.checkBox_itemManagement_showItemQuickInputPanel.Size = new System.Drawing.Size(279, 25);
            this.checkBox_itemManagement_showItemQuickInputPanel.TabIndex = 5;
            this.checkBox_itemManagement_showItemQuickInputPanel.Text = "显示条码快速输入面板(&B)";
            this.checkBox_itemManagement_showItemQuickInputPanel.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_showQueryPanel
            // 
            this.checkBox_itemManagement_showQueryPanel.AutoSize = true;
            this.checkBox_itemManagement_showQueryPanel.Location = new System.Drawing.Point(17, 205);
            this.checkBox_itemManagement_showQueryPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_showQueryPanel.Name = "checkBox_itemManagement_showQueryPanel";
            this.checkBox_itemManagement_showQueryPanel.Size = new System.Drawing.Size(195, 25);
            this.checkBox_itemManagement_showQueryPanel.TabIndex = 4;
            this.checkBox_itemManagement_showQueryPanel.Text = "显示检索面板(&Q)";
            this.checkBox_itemManagement_showQueryPanel.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_verifyDataWhenSaving
            // 
            this.checkBox_itemManagement_verifyDataWhenSaving.AutoSize = true;
            this.checkBox_itemManagement_verifyDataWhenSaving.Location = new System.Drawing.Point(17, 112);
            this.checkBox_itemManagement_verifyDataWhenSaving.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_verifyDataWhenSaving.Name = "checkBox_itemManagement_verifyDataWhenSaving";
            this.checkBox_itemManagement_verifyDataWhenSaving.Size = new System.Drawing.Size(342, 25);
            this.checkBox_itemManagement_verifyDataWhenSaving.TabIndex = 2;
            this.checkBox_itemManagement_verifyDataWhenSaving.Text = "保存书目记录时自动校验数据(&V)";
            this.checkBox_itemManagement_verifyDataWhenSaving.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_searchDupWhenSaving
            // 
            this.checkBox_itemManagement_searchDupWhenSaving.AutoSize = true;
            this.checkBox_itemManagement_searchDupWhenSaving.Location = new System.Drawing.Point(17, 147);
            this.checkBox_itemManagement_searchDupWhenSaving.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_searchDupWhenSaving.Name = "checkBox_itemManagement_searchDupWhenSaving";
            this.checkBox_itemManagement_searchDupWhenSaving.Size = new System.Drawing.Size(300, 25);
            this.checkBox_itemManagement_searchDupWhenSaving.TabIndex = 3;
            this.checkBox_itemManagement_searchDupWhenSaving.Text = "保存书目记录时自动查重(&S)";
            this.checkBox_itemManagement_searchDupWhenSaving.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_cataloging
            // 
            this.checkBox_itemManagement_cataloging.AutoSize = true;
            this.checkBox_itemManagement_cataloging.Location = new System.Drawing.Point(17, 77);
            this.checkBox_itemManagement_cataloging.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_cataloging.Name = "checkBox_itemManagement_cataloging";
            this.checkBox_itemManagement_cataloging.Size = new System.Drawing.Size(153, 25);
            this.checkBox_itemManagement_cataloging.TabIndex = 1;
            this.checkBox_itemManagement_cataloging.Text = "编目功能(&C)";
            this.checkBox_itemManagement_cataloging.UseVisualStyleBackColor = true;
            // 
            // checkBox_itemManagement_verifyItemBarcode
            // 
            this.checkBox_itemManagement_verifyItemBarcode.AutoSize = true;
            this.checkBox_itemManagement_verifyItemBarcode.Location = new System.Drawing.Point(17, 42);
            this.checkBox_itemManagement_verifyItemBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_itemManagement_verifyItemBarcode.Name = "checkBox_itemManagement_verifyItemBarcode";
            this.checkBox_itemManagement_verifyItemBarcode.Size = new System.Drawing.Size(258, 25);
            this.checkBox_itemManagement_verifyItemBarcode.TabIndex = 0;
            this.checkBox_itemManagement_verifyItemBarcode.Text = "校验输入的册条码号(&V)";
            this.checkBox_itemManagement_verifyItemBarcode.UseVisualStyleBackColor = true;
            // 
            // tabPage_ui
            // 
            this.tabPage_ui.AutoScroll = true;
            this.tabPage_ui.Controls.Add(this.textBox_ui_loginWelcomeText);
            this.tabPage_ui.Controls.Add(this.label48);
            this.tabPage_ui.Controls.Add(this.checkBox_ui_printLabelMode);
            this.tabPage_ui.Controls.Add(this.checkBox_ui_fixedPanelAnimationEnabled);
            this.tabPage_ui.Controls.Add(this.button_ui_getDefaultFont);
            this.tabPage_ui.Controls.Add(this.textBox_ui_defaultFont);
            this.tabPage_ui.Controls.Add(this.label16);
            this.tabPage_ui.Controls.Add(this.checkBox_ui_hideFixedPanel);
            this.tabPage_ui.Controls.Add(this.comboBox_ui_fixedPanelDock);
            this.tabPage_ui.Controls.Add(this.label6);
            this.tabPage_ui.Location = new System.Drawing.Point(4, 85);
            this.tabPage_ui.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_ui.Name = "tabPage_ui";
            this.tabPage_ui.Size = new System.Drawing.Size(905, 459);
            this.tabPage_ui.TabIndex = 4;
            this.tabPage_ui.Text = "外观";
            this.tabPage_ui.UseVisualStyleBackColor = true;
            // 
            // textBox_ui_loginWelcomeText
            // 
            this.textBox_ui_loginWelcomeText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_ui_loginWelcomeText.Location = new System.Drawing.Point(253, 376);
            this.textBox_ui_loginWelcomeText.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_ui_loginWelcomeText.Name = "textBox_ui_loginWelcomeText";
            this.textBox_ui_loginWelcomeText.Size = new System.Drawing.Size(633, 31);
            this.textBox_ui_loginWelcomeText.TabIndex = 9;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(5, 381);
            this.label48.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(201, 21);
            this.label48.TabIndex = 8;
            this.label48.Text = "登录画面欢迎语(&W):";
            // 
            // checkBox_ui_printLabelMode
            // 
            this.checkBox_ui_printLabelMode.AutoSize = true;
            this.checkBox_ui_printLabelMode.Location = new System.Drawing.Point(9, 328);
            this.checkBox_ui_printLabelMode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_ui_printLabelMode.Name = "checkBox_ui_printLabelMode";
            this.checkBox_ui_printLabelMode.Size = new System.Drawing.Size(195, 25);
            this.checkBox_ui_printLabelMode.TabIndex = 7;
            this.checkBox_ui_printLabelMode.Text = "标签打印模式(&L)";
            this.checkBox_ui_printLabelMode.UseVisualStyleBackColor = true;
            // 
            // checkBox_ui_fixedPanelAnimationEnabled
            // 
            this.checkBox_ui_fixedPanelAnimationEnabled.AutoSize = true;
            this.checkBox_ui_fixedPanelAnimationEnabled.Location = new System.Drawing.Point(7, 115);
            this.checkBox_ui_fixedPanelAnimationEnabled.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_ui_fixedPanelAnimationEnabled.Name = "checkBox_ui_fixedPanelAnimationEnabled";
            this.checkBox_ui_fixedPanelAnimationEnabled.Size = new System.Drawing.Size(237, 25);
            this.checkBox_ui_fixedPanelAnimationEnabled.TabIndex = 3;
            this.checkBox_ui_fixedPanelAnimationEnabled.Text = "固定面板活动动画(&A)";
            this.checkBox_ui_fixedPanelAnimationEnabled.UseVisualStyleBackColor = true;
            // 
            // button_ui_getDefaultFont
            // 
            this.button_ui_getDefaultFont.Location = new System.Drawing.Point(560, 192);
            this.button_ui_getDefaultFont.Margin = new System.Windows.Forms.Padding(5);
            this.button_ui_getDefaultFont.Name = "button_ui_getDefaultFont";
            this.button_ui_getDefaultFont.Size = new System.Drawing.Size(88, 40);
            this.button_ui_getDefaultFont.TabIndex = 6;
            this.button_ui_getDefaultFont.Text = "...";
            this.button_ui_getDefaultFont.UseVisualStyleBackColor = true;
            this.button_ui_getDefaultFont.Click += new System.EventHandler(this.button_ui_getDefaultFont_Click);
            // 
            // textBox_ui_defaultFont
            // 
            this.textBox_ui_defaultFont.Location = new System.Drawing.Point(253, 196);
            this.textBox_ui_defaultFont.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_ui_defaultFont.Name = "textBox_ui_defaultFont";
            this.textBox_ui_defaultFont.Size = new System.Drawing.Size(292, 31);
            this.textBox_ui_defaultFont.TabIndex = 5;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(5, 201);
            this.label16.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(138, 21);
            this.label16.TabIndex = 4;
            this.label16.Text = "默认字体(&D):";
            // 
            // checkBox_ui_hideFixedPanel
            // 
            this.checkBox_ui_hideFixedPanel.AutoSize = true;
            this.checkBox_ui_hideFixedPanel.Location = new System.Drawing.Point(7, 80);
            this.checkBox_ui_hideFixedPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_ui_hideFixedPanel.Name = "checkBox_ui_hideFixedPanel";
            this.checkBox_ui_hideFixedPanel.Size = new System.Drawing.Size(195, 25);
            this.checkBox_ui_hideFixedPanel.TabIndex = 2;
            this.checkBox_ui_hideFixedPanel.Text = "隐藏固定面板(&H)";
            this.checkBox_ui_hideFixedPanel.UseVisualStyleBackColor = true;
            this.checkBox_ui_hideFixedPanel.CheckedChanged += new System.EventHandler(this.checkBox_ui_hideFixedPanel_CheckedChanged);
            // 
            // comboBox_ui_fixedPanelDock
            // 
            this.comboBox_ui_fixedPanelDock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_ui_fixedPanelDock.FormattingEnabled = true;
            this.comboBox_ui_fixedPanelDock.Items.AddRange(new object[] {
            "Top",
            "Bottom",
            "Left",
            "Right"});
            this.comboBox_ui_fixedPanelDock.Location = new System.Drawing.Point(254, 26);
            this.comboBox_ui_fixedPanelDock.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_ui_fixedPanelDock.Name = "comboBox_ui_fixedPanelDock";
            this.comboBox_ui_fixedPanelDock.Size = new System.Drawing.Size(134, 29);
            this.comboBox_ui_fixedPanelDock.TabIndex = 1;
            this.comboBox_ui_fixedPanelDock.SelectedIndexChanged += new System.EventHandler(this.comboBox_ui_fixedPanelDock_SelectedIndexChanged);
            this.comboBox_ui_fixedPanelDock.SizeChanged += new System.EventHandler(this.comboBox_ui_fixedPanelDock_SizeChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 31);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(222, 21);
            this.label6.TabIndex = 0;
            this.label6.Text = "固定面板停靠方向(&F):";
            // 
            // tabPage_passgate
            // 
            this.tabPage_passgate.AutoScroll = true;
            this.tabPage_passgate.Controls.Add(this.numericUpDown_passgate_maxListItemsCount);
            this.tabPage_passgate.Controls.Add(this.label8);
            this.tabPage_passgate.Location = new System.Drawing.Point(4, 85);
            this.tabPage_passgate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_passgate.Name = "tabPage_passgate";
            this.tabPage_passgate.Size = new System.Drawing.Size(905, 459);
            this.tabPage_passgate.TabIndex = 6;
            this.tabPage_passgate.Text = "入馆登记";
            this.tabPage_passgate.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_passgate_maxListItemsCount
            // 
            this.numericUpDown_passgate_maxListItemsCount.Location = new System.Drawing.Point(213, 19);
            this.numericUpDown_passgate_maxListItemsCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_passgate_maxListItemsCount.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_passgate_maxListItemsCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_passgate_maxListItemsCount.Name = "numericUpDown_passgate_maxListItemsCount";
            this.numericUpDown_passgate_maxListItemsCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_passgate_maxListItemsCount.TabIndex = 1;
            this.numericUpDown_passgate_maxListItemsCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 23);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(180, 21);
            this.label8.TabIndex = 0;
            this.label8.Text = "列表最大行数(&M):";
            // 
            // tabPage_search
            // 
            this.tabPage_search.AutoScroll = true;
            this.tabPage_search.Controls.Add(this.groupBox6);
            this.tabPage_search.Controls.Add(this.groupBox5);
            this.tabPage_search.Controls.Add(this.groupBox4);
            this.tabPage_search.Controls.Add(this.checkBox_search_useExistDetailWindow);
            this.tabPage_search.Controls.Add(this.groupBox3);
            this.tabPage_search.Controls.Add(this.groupBox2);
            this.tabPage_search.Controls.Add(this.groupBox1);
            this.tabPage_search.Location = new System.Drawing.Point(4, 85);
            this.tabPage_search.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_search.Name = "tabPage_search";
            this.tabPage_search.Size = new System.Drawing.Size(905, 459);
            this.tabPage_search.TabIndex = 7;
            this.tabPage_search.Text = "检索";
            this.tabPage_search.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.checkBox_search_hideCommentMatchStyleAndDbName);
            this.groupBox6.Controls.Add(this.checkBox_search_commentPushFilling);
            this.groupBox6.Controls.Add(this.label19);
            this.groupBox6.Controls.Add(this.numericUpDown_search_maxCommentResultCount);
            this.groupBox6.Location = new System.Drawing.Point(7, 1231);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox6.Size = new System.Drawing.Size(864, 175);
            this.groupBox6.TabIndex = 6;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "评注查询窗 ";
            // 
            // checkBox_search_hideCommentMatchStyleAndDbName
            // 
            this.checkBox_search_hideCommentMatchStyleAndDbName.AutoSize = true;
            this.checkBox_search_hideCommentMatchStyleAndDbName.Location = new System.Drawing.Point(26, 87);
            this.checkBox_search_hideCommentMatchStyleAndDbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_hideCommentMatchStyleAndDbName.Name = "checkBox_search_hideCommentMatchStyleAndDbName";
            this.checkBox_search_hideCommentMatchStyleAndDbName.Size = new System.Drawing.Size(342, 25);
            this.checkBox_search_hideCommentMatchStyleAndDbName.TabIndex = 2;
            this.checkBox_search_hideCommentMatchStyleAndDbName.Text = "隐藏数据库名和匹配方式列表(&H)";
            this.checkBox_search_hideCommentMatchStyleAndDbName.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_commentPushFilling
            // 
            this.checkBox_search_commentPushFilling.AutoSize = true;
            this.checkBox_search_commentPushFilling.Location = new System.Drawing.Point(26, 122);
            this.checkBox_search_commentPushFilling.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_commentPushFilling.Name = "checkBox_search_commentPushFilling";
            this.checkBox_search_commentPushFilling.Size = new System.Drawing.Size(258, 25);
            this.checkBox_search_commentPushFilling.TabIndex = 3;
            this.checkBox_search_commentPushFilling.Text = "推动式装入浏览列表(&P)";
            this.checkBox_search_commentPushFilling.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(20, 37);
            this.label19.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(180, 21);
            this.label19.TabIndex = 0;
            this.label19.Text = "最大命中条数(&I):";
            // 
            // numericUpDown_search_maxCommentResultCount
            // 
            this.numericUpDown_search_maxCommentResultCount.Location = new System.Drawing.Point(249, 33);
            this.numericUpDown_search_maxCommentResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_maxCommentResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_maxCommentResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_maxCommentResultCount.Name = "numericUpDown_search_maxCommentResultCount";
            this.numericUpDown_search_maxCommentResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_maxCommentResultCount.TabIndex = 1;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBox_search_hideIssueMatchStyleAndDbName);
            this.groupBox5.Controls.Add(this.checkBox_search_issuePushFilling);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Controls.Add(this.numericUpDown_search_maxIssueResultCount);
            this.groupBox5.Location = new System.Drawing.Point(7, 1020);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox5.Size = new System.Drawing.Size(864, 175);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "期查询窗 ";
            // 
            // checkBox_search_hideIssueMatchStyleAndDbName
            // 
            this.checkBox_search_hideIssueMatchStyleAndDbName.AutoSize = true;
            this.checkBox_search_hideIssueMatchStyleAndDbName.Location = new System.Drawing.Point(26, 87);
            this.checkBox_search_hideIssueMatchStyleAndDbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_hideIssueMatchStyleAndDbName.Name = "checkBox_search_hideIssueMatchStyleAndDbName";
            this.checkBox_search_hideIssueMatchStyleAndDbName.Size = new System.Drawing.Size(342, 25);
            this.checkBox_search_hideIssueMatchStyleAndDbName.TabIndex = 2;
            this.checkBox_search_hideIssueMatchStyleAndDbName.Text = "隐藏数据库名和匹配方式列表(&H)";
            this.checkBox_search_hideIssueMatchStyleAndDbName.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_issuePushFilling
            // 
            this.checkBox_search_issuePushFilling.AutoSize = true;
            this.checkBox_search_issuePushFilling.Location = new System.Drawing.Point(26, 122);
            this.checkBox_search_issuePushFilling.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_issuePushFilling.Name = "checkBox_search_issuePushFilling";
            this.checkBox_search_issuePushFilling.Size = new System.Drawing.Size(258, 25);
            this.checkBox_search_issuePushFilling.TabIndex = 3;
            this.checkBox_search_issuePushFilling.Text = "推动式装入浏览列表(&P)";
            this.checkBox_search_issuePushFilling.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(20, 37);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(180, 21);
            this.label18.TabIndex = 0;
            this.label18.Text = "最大命中条数(&I):";
            // 
            // numericUpDown_search_maxIssueResultCount
            // 
            this.numericUpDown_search_maxIssueResultCount.Location = new System.Drawing.Point(249, 33);
            this.numericUpDown_search_maxIssueResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_maxIssueResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_maxIssueResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_maxIssueResultCount.Name = "numericUpDown_search_maxIssueResultCount";
            this.numericUpDown_search_maxIssueResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_maxIssueResultCount.TabIndex = 1;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBox_search_hideOrderMatchStyleAndDbName);
            this.groupBox4.Controls.Add(this.checkBox_search_orderPushFilling);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.numericUpDown_search_maxOrderResultCount);
            this.groupBox4.Location = new System.Drawing.Point(7, 816);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(864, 175);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "订购查询窗 ";
            // 
            // checkBox_search_hideOrderMatchStyleAndDbName
            // 
            this.checkBox_search_hideOrderMatchStyleAndDbName.AutoSize = true;
            this.checkBox_search_hideOrderMatchStyleAndDbName.Location = new System.Drawing.Point(26, 87);
            this.checkBox_search_hideOrderMatchStyleAndDbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_hideOrderMatchStyleAndDbName.Name = "checkBox_search_hideOrderMatchStyleAndDbName";
            this.checkBox_search_hideOrderMatchStyleAndDbName.Size = new System.Drawing.Size(342, 25);
            this.checkBox_search_hideOrderMatchStyleAndDbName.TabIndex = 2;
            this.checkBox_search_hideOrderMatchStyleAndDbName.Text = "隐藏数据库名和匹配方式列表(&H)";
            this.checkBox_search_hideOrderMatchStyleAndDbName.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_orderPushFilling
            // 
            this.checkBox_search_orderPushFilling.AutoSize = true;
            this.checkBox_search_orderPushFilling.Location = new System.Drawing.Point(26, 122);
            this.checkBox_search_orderPushFilling.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_orderPushFilling.Name = "checkBox_search_orderPushFilling";
            this.checkBox_search_orderPushFilling.Size = new System.Drawing.Size(258, 25);
            this.checkBox_search_orderPushFilling.TabIndex = 3;
            this.checkBox_search_orderPushFilling.Text = "推动式装入浏览列表(&P)";
            this.checkBox_search_orderPushFilling.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(20, 37);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(180, 21);
            this.label17.TabIndex = 0;
            this.label17.Text = "最大命中条数(&I):";
            // 
            // numericUpDown_search_maxOrderResultCount
            // 
            this.numericUpDown_search_maxOrderResultCount.Location = new System.Drawing.Point(249, 33);
            this.numericUpDown_search_maxOrderResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_maxOrderResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_maxOrderResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_maxOrderResultCount.Name = "numericUpDown_search_maxOrderResultCount";
            this.numericUpDown_search_maxOrderResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_maxOrderResultCount.TabIndex = 1;
            // 
            // checkBox_search_useExistDetailWindow
            // 
            this.checkBox_search_useExistDetailWindow.AutoSize = true;
            this.checkBox_search_useExistDetailWindow.Location = new System.Drawing.Point(7, 31);
            this.checkBox_search_useExistDetailWindow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_useExistDetailWindow.Name = "checkBox_search_useExistDetailWindow";
            this.checkBox_search_useExistDetailWindow.Size = new System.Drawing.Size(468, 25);
            this.checkBox_search_useExistDetailWindow.TabIndex = 0;
            this.checkBox_search_useExistDetailWindow.Text = "在浏览框中双击时优先装入已打开的详细窗(&E)";
            this.checkBox_search_useExistDetailWindow.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBox_search_itemFilterLibraryCode);
            this.groupBox3.Controls.Add(this.checkBox_search_hideItemMatchStyleAndDbName);
            this.groupBox3.Controls.Add(this.checkBox_search_itemPushFilling);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.numericUpDown_search_maxItemResultCount);
            this.groupBox3.Location = new System.Drawing.Point(7, 567);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(864, 208);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "实体查询窗 ";
            // 
            // checkBox_search_itemFilterLibraryCode
            // 
            this.checkBox_search_itemFilterLibraryCode.AutoSize = true;
            this.checkBox_search_itemFilterLibraryCode.Location = new System.Drawing.Point(26, 153);
            this.checkBox_search_itemFilterLibraryCode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_itemFilterLibraryCode.Name = "checkBox_search_itemFilterLibraryCode";
            this.checkBox_search_itemFilterLibraryCode.Size = new System.Drawing.Size(532, 25);
            this.checkBox_search_itemFilterLibraryCode.TabIndex = 4;
            this.checkBox_search_itemFilterLibraryCode.Text = "只看本分馆册[根据当前账户的馆代码来进行过滤](&F)";
            this.checkBox_search_itemFilterLibraryCode.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_hideItemMatchStyleAndDbName
            // 
            this.checkBox_search_hideItemMatchStyleAndDbName.AutoSize = true;
            this.checkBox_search_hideItemMatchStyleAndDbName.Location = new System.Drawing.Point(26, 87);
            this.checkBox_search_hideItemMatchStyleAndDbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_hideItemMatchStyleAndDbName.Name = "checkBox_search_hideItemMatchStyleAndDbName";
            this.checkBox_search_hideItemMatchStyleAndDbName.Size = new System.Drawing.Size(342, 25);
            this.checkBox_search_hideItemMatchStyleAndDbName.TabIndex = 2;
            this.checkBox_search_hideItemMatchStyleAndDbName.Text = "隐藏数据库名和匹配方式列表(&H)";
            this.checkBox_search_hideItemMatchStyleAndDbName.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_itemPushFilling
            // 
            this.checkBox_search_itemPushFilling.AutoSize = true;
            this.checkBox_search_itemPushFilling.Location = new System.Drawing.Point(26, 122);
            this.checkBox_search_itemPushFilling.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_itemPushFilling.Name = "checkBox_search_itemPushFilling";
            this.checkBox_search_itemPushFilling.Size = new System.Drawing.Size(258, 25);
            this.checkBox_search_itemPushFilling.TabIndex = 3;
            this.checkBox_search_itemPushFilling.Text = "推动式装入浏览列表(&P)";
            this.checkBox_search_itemPushFilling.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(20, 37);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(180, 21);
            this.label11.TabIndex = 0;
            this.label11.Text = "最大命中条数(&I):";
            // 
            // numericUpDown_search_maxItemResultCount
            // 
            this.numericUpDown_search_maxItemResultCount.Location = new System.Drawing.Point(249, 33);
            this.numericUpDown_search_maxItemResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_maxItemResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_maxItemResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_maxItemResultCount.Name = "numericUpDown_search_maxItemResultCount";
            this.numericUpDown_search_maxItemResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_maxItemResultCount.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_search_hideReaderMatchStyle);
            this.groupBox2.Controls.Add(this.checkBox_search_readerPushFilling);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.numericUpDown_search_maxReaderResultCount);
            this.groupBox2.Location = new System.Drawing.Point(7, 376);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(864, 161);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " 读者查询窗 ";
            // 
            // checkBox_search_hideReaderMatchStyle
            // 
            this.checkBox_search_hideReaderMatchStyle.AutoSize = true;
            this.checkBox_search_hideReaderMatchStyle.Location = new System.Drawing.Point(26, 73);
            this.checkBox_search_hideReaderMatchStyle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_hideReaderMatchStyle.Name = "checkBox_search_hideReaderMatchStyle";
            this.checkBox_search_hideReaderMatchStyle.Size = new System.Drawing.Size(237, 25);
            this.checkBox_search_hideReaderMatchStyle.TabIndex = 2;
            this.checkBox_search_hideReaderMatchStyle.Text = "隐藏匹配方式列表(&M)";
            this.checkBox_search_hideReaderMatchStyle.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_readerPushFilling
            // 
            this.checkBox_search_readerPushFilling.AutoSize = true;
            this.checkBox_search_readerPushFilling.Location = new System.Drawing.Point(26, 108);
            this.checkBox_search_readerPushFilling.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_readerPushFilling.Name = "checkBox_search_readerPushFilling";
            this.checkBox_search_readerPushFilling.Size = new System.Drawing.Size(258, 25);
            this.checkBox_search_readerPushFilling.TabIndex = 3;
            this.checkBox_search_readerPushFilling.Text = "推动式装入浏览列表(&P)";
            this.checkBox_search_readerPushFilling.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(20, 31);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(180, 21);
            this.label10.TabIndex = 0;
            this.label10.Text = "最大命中条数(&R):";
            // 
            // numericUpDown_search_maxReaderResultCount
            // 
            this.numericUpDown_search_maxReaderResultCount.Location = new System.Drawing.Point(249, 30);
            this.numericUpDown_search_maxReaderResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_maxReaderResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_maxReaderResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_maxReaderResultCount.Name = "numericUpDown_search_maxReaderResultCount";
            this.numericUpDown_search_maxReaderResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_maxReaderResultCount.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label36);
            this.groupBox1.Controls.Add(this.numericUpDown_search_multiline_maxBiblioResultCount);
            this.groupBox1.Controls.Add(this.checkBox_search_biblioPushFilling);
            this.groupBox1.Controls.Add(this.checkBox_search_hideBiblioMatchStyle);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.numericUpDown_search_maxBiblioResultCount);
            this.groupBox1.Location = new System.Drawing.Point(7, 80);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(864, 267);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 书目查询窗 ";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(20, 180);
            this.label36.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(411, 21);
            this.label36.TabIndex = 4;
            this.label36.Text = "多行检索时每一个检索词最大命中条数(&M):";
            // 
            // numericUpDown_search_multiline_maxBiblioResultCount
            // 
            this.numericUpDown_search_multiline_maxBiblioResultCount.Location = new System.Drawing.Point(249, 215);
            this.numericUpDown_search_multiline_maxBiblioResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_multiline_maxBiblioResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_multiline_maxBiblioResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_multiline_maxBiblioResultCount.Name = "numericUpDown_search_multiline_maxBiblioResultCount";
            this.numericUpDown_search_multiline_maxBiblioResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_multiline_maxBiblioResultCount.TabIndex = 5;
            // 
            // checkBox_search_biblioPushFilling
            // 
            this.checkBox_search_biblioPushFilling.AutoSize = true;
            this.checkBox_search_biblioPushFilling.Location = new System.Drawing.Point(26, 119);
            this.checkBox_search_biblioPushFilling.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_biblioPushFilling.Name = "checkBox_search_biblioPushFilling";
            this.checkBox_search_biblioPushFilling.Size = new System.Drawing.Size(258, 25);
            this.checkBox_search_biblioPushFilling.TabIndex = 3;
            this.checkBox_search_biblioPushFilling.Text = "推动式装入浏览列表(&P)";
            this.checkBox_search_biblioPushFilling.UseVisualStyleBackColor = true;
            // 
            // checkBox_search_hideBiblioMatchStyle
            // 
            this.checkBox_search_hideBiblioMatchStyle.AutoSize = true;
            this.checkBox_search_hideBiblioMatchStyle.Location = new System.Drawing.Point(26, 84);
            this.checkBox_search_hideBiblioMatchStyle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_search_hideBiblioMatchStyle.Name = "checkBox_search_hideBiblioMatchStyle";
            this.checkBox_search_hideBiblioMatchStyle.Size = new System.Drawing.Size(237, 25);
            this.checkBox_search_hideBiblioMatchStyle.TabIndex = 2;
            this.checkBox_search_hideBiblioMatchStyle.Text = "隐藏匹配方式列表(&M)";
            this.checkBox_search_hideBiblioMatchStyle.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 37);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(180, 21);
            this.label9.TabIndex = 0;
            this.label9.Text = "最大命中条数(&B):";
            // 
            // numericUpDown_search_maxBiblioResultCount
            // 
            this.numericUpDown_search_maxBiblioResultCount.Location = new System.Drawing.Point(249, 33);
            this.numericUpDown_search_maxBiblioResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_search_maxBiblioResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_search_maxBiblioResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_search_maxBiblioResultCount.Name = "numericUpDown_search_maxBiblioResultCount";
            this.numericUpDown_search_maxBiblioResultCount.Size = new System.Drawing.Size(165, 31);
            this.numericUpDown_search_maxBiblioResultCount.TabIndex = 1;
            // 
            // tabPage_print
            // 
            this.tabPage_print.AutoScroll = true;
            this.tabPage_print.Controls.Add(this.button_print_findProject);
            this.tabPage_print.Controls.Add(this.label_print_projectNameMessage);
            this.tabPage_print.Controls.Add(this.textBox_print_projectName);
            this.tabPage_print.Controls.Add(this.label13);
            this.tabPage_print.Controls.Add(this.button_print_projectManage);
            this.tabPage_print.Controls.Add(this.checkBox_print_pausePrint);
            this.tabPage_print.Controls.Add(this.comboBox_print_prnPort);
            this.tabPage_print.Controls.Add(this.label12);
            this.tabPage_print.Controls.Add(this.toolStrip_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 85);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(905, 459);
            this.tabPage_print.TabIndex = 8;
            this.tabPage_print.Text = "凭条打印";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print_findProject
            // 
            this.button_print_findProject.Location = new System.Drawing.Point(604, 276);
            this.button_print_findProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_print_findProject.Name = "button_print_findProject";
            this.button_print_findProject.Size = new System.Drawing.Size(76, 38);
            this.button_print_findProject.TabIndex = 9;
            this.button_print_findProject.Text = "...";
            this.button_print_findProject.UseVisualStyleBackColor = true;
            this.button_print_findProject.Click += new System.EventHandler(this.button_print_findProject_Click);
            // 
            // label_print_projectNameMessage
            // 
            this.label_print_projectNameMessage.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_print_projectNameMessage.Location = new System.Drawing.Point(209, 317);
            this.label_print_projectNameMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_print_projectNameMessage.Name = "label_print_projectNameMessage";
            this.label_print_projectNameMessage.Size = new System.Drawing.Size(389, 89);
            this.label_print_projectNameMessage.TabIndex = 8;
            // 
            // textBox_print_projectName
            // 
            this.textBox_print_projectName.Location = new System.Drawing.Point(213, 276);
            this.textBox_print_projectName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_print_projectName.Name = "textBox_print_projectName";
            this.textBox_print_projectName.Size = new System.Drawing.Size(382, 31);
            this.textBox_print_projectName.TabIndex = 7;
            this.textBox_print_projectName.TextChanged += new System.EventHandler(this.textBox_print_projectName_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(17, 282);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(117, 21);
            this.label13.TabIndex = 6;
            this.label13.Text = "方案名(&N):";
            // 
            // button_print_projectManage
            // 
            this.button_print_projectManage.Location = new System.Drawing.Point(20, 210);
            this.button_print_projectManage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_print_projectManage.Name = "button_print_projectManage";
            this.button_print_projectManage.Size = new System.Drawing.Size(194, 38);
            this.button_print_projectManage.TabIndex = 5;
            this.button_print_projectManage.Text = "方案管理(&P)...";
            this.button_print_projectManage.UseVisualStyleBackColor = true;
            this.button_print_projectManage.Click += new System.EventHandler(this.button_print_projectManage_Click);
            // 
            // checkBox_print_pausePrint
            // 
            this.checkBox_print_pausePrint.AutoSize = true;
            this.checkBox_print_pausePrint.Location = new System.Drawing.Point(20, 150);
            this.checkBox_print_pausePrint.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_print_pausePrint.Name = "checkBox_print_pausePrint";
            this.checkBox_print_pausePrint.Size = new System.Drawing.Size(153, 25);
            this.checkBox_print_pausePrint.TabIndex = 4;
            this.checkBox_print_pausePrint.Text = "暂停打印(&P)";
            this.checkBox_print_pausePrint.UseVisualStyleBackColor = true;
            // 
            // comboBox_print_prnPort
            // 
            this.comboBox_print_prnPort.FormattingEnabled = true;
            this.comboBox_print_prnPort.Items.AddRange(new object[] {
            "LPT1",
            "LPT2",
            "LPT3"});
            this.comboBox_print_prnPort.Location = new System.Drawing.Point(213, 82);
            this.comboBox_print_prnPort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_print_prnPort.Name = "comboBox_print_prnPort";
            this.comboBox_print_prnPort.Size = new System.Drawing.Size(165, 29);
            this.comboBox_print_prnPort.TabIndex = 3;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(17, 87);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(159, 21);
            this.label12.TabIndex = 1;
            this.label12.Text = "打印机端口(&L):";
            // 
            // toolStrip_print
            // 
            this.toolStrip_print.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_print.AutoSize = false;
            this.toolStrip_print.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_print.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_print.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_managePrintScript});
            this.toolStrip_print.Location = new System.Drawing.Point(0, 24);
            this.toolStrip_print.Name = "toolStrip_print";
            this.toolStrip_print.Size = new System.Drawing.Size(496, 35);
            this.toolStrip_print.TabIndex = 0;
            this.toolStrip_print.Visible = false;
            // 
            // toolStripDropDownButton_managePrintScript
            // 
            this.toolStripDropDownButton_managePrintScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_managePrintScript.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_print_editCharingPrintCs,
            this.MenuItem_print_editCharingPrintCsRef});
            this.toolStripDropDownButton_managePrintScript.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_managePrintScript.Image")));
            this.toolStripDropDownButton_managePrintScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_managePrintScript.Name = "toolStripDropDownButton_managePrintScript";
            this.toolStripDropDownButton_managePrintScript.Size = new System.Drawing.Size(201, 29);
            this.toolStripDropDownButton_managePrintScript.Text = "管理打印脚本代码";
            // 
            // MenuItem_print_editCharingPrintCs
            // 
            this.MenuItem_print_editCharingPrintCs.Name = "MenuItem_print_editCharingPrintCs";
            this.MenuItem_print_editCharingPrintCs.Size = new System.Drawing.Size(333, 40);
            this.MenuItem_print_editCharingPrintCs.Text = "charging_print.cs";
            this.MenuItem_print_editCharingPrintCs.Click += new System.EventHandler(this.MenuItem_print_editCharingPrintCs_Click);
            // 
            // MenuItem_print_editCharingPrintCsRef
            // 
            this.MenuItem_print_editCharingPrintCsRef.Name = "MenuItem_print_editCharingPrintCsRef";
            this.MenuItem_print_editCharingPrintCsRef.Size = new System.Drawing.Size(333, 40);
            this.MenuItem_print_editCharingPrintCsRef.Text = "charging_print.cs.ref";
            this.MenuItem_print_editCharingPrintCsRef.Click += new System.EventHandler(this.MenuItem_print_editCharingPrintCsRef_Click);
            // 
            // tabPage_amerce
            // 
            this.tabPage_amerce.AutoScroll = true;
            this.tabPage_amerce.Controls.Add(this.comboBox_amerce_layout);
            this.tabPage_amerce.Controls.Add(this.label22);
            this.tabPage_amerce.Controls.Add(this.comboBox_amerce_interface);
            this.tabPage_amerce.Controls.Add(this.label15);
            this.tabPage_amerce.Location = new System.Drawing.Point(4, 85);
            this.tabPage_amerce.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_amerce.Name = "tabPage_amerce";
            this.tabPage_amerce.Size = new System.Drawing.Size(905, 459);
            this.tabPage_amerce.TabIndex = 10;
            this.tabPage_amerce.Text = "违约/交费";
            this.tabPage_amerce.UseVisualStyleBackColor = true;
            // 
            // comboBox_amerce_layout
            // 
            this.comboBox_amerce_layout.FormattingEnabled = true;
            this.comboBox_amerce_layout.Items.AddRange(new object[] {
            "左右分布",
            "上下分布"});
            this.comboBox_amerce_layout.Location = new System.Drawing.Point(225, 89);
            this.comboBox_amerce_layout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_amerce_layout.Name = "comboBox_amerce_layout";
            this.comboBox_amerce_layout.Size = new System.Drawing.Size(300, 29);
            this.comboBox_amerce_layout.TabIndex = 3;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(4, 94);
            this.label22.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(159, 21);
            this.label22.TabIndex = 2;
            this.label22.Text = "交费窗布局(&L):";
            // 
            // comboBox_amerce_interface
            // 
            this.comboBox_amerce_interface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_amerce_interface.FormattingEnabled = true;
            this.comboBox_amerce_interface.Items.AddRange(new object[] {
            "<无>",
            "ipc://CardCenterChannel/CardCenterServer",
            "迪科远望"});
            this.comboBox_amerce_interface.Location = new System.Drawing.Point(225, 23);
            this.comboBox_amerce_interface.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_amerce_interface.Name = "comboBox_amerce_interface";
            this.comboBox_amerce_interface.Size = new System.Drawing.Size(587, 29);
            this.comboBox_amerce_interface.TabIndex = 1;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(4, 28);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(181, 21);
            this.label15.TabIndex = 0;
            this.label15.Text = "IC卡扣款接口(&I):";
            // 
            // tabPage_accept
            // 
            this.tabPage_accept.AutoScroll = true;
            this.tabPage_accept.Controls.Add(this.checkBox_accept_singleClickLoadDetail);
            this.tabPage_accept.Location = new System.Drawing.Point(4, 85);
            this.tabPage_accept.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_accept.Name = "tabPage_accept";
            this.tabPage_accept.Size = new System.Drawing.Size(905, 459);
            this.tabPage_accept.TabIndex = 11;
            this.tabPage_accept.Text = "验收";
            this.tabPage_accept.UseVisualStyleBackColor = true;
            // 
            // checkBox_accept_singleClickLoadDetail
            // 
            this.checkBox_accept_singleClickLoadDetail.AutoSize = true;
            this.checkBox_accept_singleClickLoadDetail.Location = new System.Drawing.Point(4, 24);
            this.checkBox_accept_singleClickLoadDetail.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_accept_singleClickLoadDetail.Name = "checkBox_accept_singleClickLoadDetail";
            this.checkBox_accept_singleClickLoadDetail.Size = new System.Drawing.Size(489, 25);
            this.checkBox_accept_singleClickLoadDetail.TabIndex = 0;
            this.checkBox_accept_singleClickLoadDetail.Text = "在浏览框单击鼠标左键即可将记录装入详细窗(&S)";
            this.checkBox_accept_singleClickLoadDetail.UseVisualStyleBackColor = true;
            // 
            // tabPage_cardReader
            // 
            this.tabPage_cardReader.AutoScroll = true;
            this.tabPage_cardReader.Controls.Add(this.groupBox_rfidTest);
            this.tabPage_cardReader.Controls.Add(this.groupBox_rfidReader);
            this.tabPage_cardReader.Controls.Add(this.groupBox_idcardReader);
            this.tabPage_cardReader.Location = new System.Drawing.Point(4, 85);
            this.tabPage_cardReader.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_cardReader.Name = "tabPage_cardReader";
            this.tabPage_cardReader.Size = new System.Drawing.Size(905, 459);
            this.tabPage_cardReader.TabIndex = 12;
            this.tabPage_cardReader.Text = "读卡器";
            this.tabPage_cardReader.UseVisualStyleBackColor = true;
            this.tabPage_cardReader.SizeChanged += new System.EventHandler(this.tabPage_cardReader_SizeChanged);
            this.tabPage_cardReader.DoubleClick += new System.EventHandler(this.tabPage_cardReader_DoubleClick);
            // 
            // groupBox_rfidTest
            // 
            this.groupBox_rfidTest.BackColor = System.Drawing.Color.DarkGray;
            this.groupBox_rfidTest.Controls.Add(this.checkBox_rfidTest_returnPostUndoEAS);
            this.groupBox_rfidTest.Controls.Add(this.checkBox_rfidTest_returnAPI);
            this.groupBox_rfidTest.Controls.Add(this.checkBox_rfidTest_returnPreEAS);
            this.groupBox_rfidTest.Controls.Add(this.checkBox_rfidTest_borrowEAS);
            this.groupBox_rfidTest.Enabled = false;
            this.groupBox_rfidTest.ForeColor = System.Drawing.Color.White;
            this.groupBox_rfidTest.Location = new System.Drawing.Point(5, 788);
            this.groupBox_rfidTest.Name = "groupBox_rfidTest";
            this.groupBox_rfidTest.Size = new System.Drawing.Size(865, 247);
            this.groupBox_rfidTest.TabIndex = 3;
            this.groupBox_rfidTest.TabStop = false;
            this.groupBox_rfidTest.Text = "RFID 测试功能";
            // 
            // checkBox_rfidTest_returnPostUndoEAS
            // 
            this.checkBox_rfidTest_returnPostUndoEAS.AutoSize = true;
            this.checkBox_rfidTest_returnPostUndoEAS.Location = new System.Drawing.Point(21, 144);
            this.checkBox_rfidTest_returnPostUndoEAS.Name = "checkBox_rfidTest_returnPostUndoEAS";
            this.checkBox_rfidTest_returnPostUndoEAS.Size = new System.Drawing.Size(623, 25);
            this.checkBox_rfidTest_returnPostUndoEAS.TabIndex = 3;
            this.checkBox_rfidTest_returnPostUndoEAS.Text = "还书收尾 Undo EAS 时刻报错(注: 仅当 API 报错后才会发生)";
            this.checkBox_rfidTest_returnPostUndoEAS.UseVisualStyleBackColor = true;
            // 
            // checkBox_rfidTest_returnAPI
            // 
            this.checkBox_rfidTest_returnAPI.AutoSize = true;
            this.checkBox_rfidTest_returnAPI.Location = new System.Drawing.Point(21, 113);
            this.checkBox_rfidTest_returnAPI.Name = "checkBox_rfidTest_returnAPI";
            this.checkBox_rfidTest_returnAPI.Size = new System.Drawing.Size(217, 25);
            this.checkBox_rfidTest_returnAPI.TabIndex = 2;
            this.checkBox_rfidTest_returnAPI.Text = "还书 API 时刻报错";
            this.checkBox_rfidTest_returnAPI.UseVisualStyleBackColor = true;
            // 
            // checkBox_rfidTest_returnPreEAS
            // 
            this.checkBox_rfidTest_returnPreEAS.AutoSize = true;
            this.checkBox_rfidTest_returnPreEAS.Location = new System.Drawing.Point(21, 82);
            this.checkBox_rfidTest_returnPreEAS.Name = "checkBox_rfidTest_returnPreEAS";
            this.checkBox_rfidTest_returnPreEAS.Size = new System.Drawing.Size(301, 25);
            this.checkBox_rfidTest_returnPreEAS.TabIndex = 1;
            this.checkBox_rfidTest_returnPreEAS.Text = "还书开始修改 EAS 时刻报错";
            this.checkBox_rfidTest_returnPreEAS.UseVisualStyleBackColor = true;
            // 
            // checkBox_rfidTest_borrowEAS
            // 
            this.checkBox_rfidTest_borrowEAS.AutoSize = true;
            this.checkBox_rfidTest_borrowEAS.Location = new System.Drawing.Point(21, 41);
            this.checkBox_rfidTest_borrowEAS.Name = "checkBox_rfidTest_borrowEAS";
            this.checkBox_rfidTest_borrowEAS.Size = new System.Drawing.Size(301, 25);
            this.checkBox_rfidTest_borrowEAS.TabIndex = 0;
            this.checkBox_rfidTest_borrowEAS.Text = "借书完成修改 EAS 时刻报错";
            this.checkBox_rfidTest_borrowEAS.UseVisualStyleBackColor = true;
            // 
            // groupBox_rfidReader
            // 
            this.groupBox_rfidReader.Controls.Add(this.comboBox_rfid_tagCachePolicy);
            this.groupBox_rfidReader.Controls.Add(this.label_rfid_tagCachePolicy);
            this.groupBox_rfidReader.Controls.Add(this.groupBox_uhf);
            this.groupBox_rfidReader.Controls.Add(this.button_cardReader_setRfidUrlDefaultValue);
            this.groupBox_rfidReader.Controls.Add(this.textBox_cardReader_rfidCenterUrl);
            this.groupBox_rfidReader.Location = new System.Drawing.Point(5, 205);
            this.groupBox_rfidReader.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_rfidReader.Name = "groupBox_rfidReader";
            this.groupBox_rfidReader.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_rfidReader.Size = new System.Drawing.Size(865, 575);
            this.groupBox_rfidReader.TabIndex = 1;
            this.groupBox_rfidReader.TabStop = false;
            this.groupBox_rfidReader.Text = " RFID 读写器接口 URL ";
            // 
            // comboBox_rfid_tagCachePolicy
            // 
            this.comboBox_rfid_tagCachePolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_rfid_tagCachePolicy.Enabled = false;
            this.comboBox_rfid_tagCachePolicy.FormattingEnabled = true;
            this.comboBox_rfid_tagCachePolicy.Items.AddRange(new object[] {
            "不缓存",
            "部分缓存",
            "要缓存"});
            this.comboBox_rfid_tagCachePolicy.Location = new System.Drawing.Point(226, 126);
            this.comboBox_rfid_tagCachePolicy.Name = "comboBox_rfid_tagCachePolicy";
            this.comboBox_rfid_tagCachePolicy.Size = new System.Drawing.Size(387, 29);
            this.comboBox_rfid_tagCachePolicy.TabIndex = 4;
            // 
            // label_rfid_tagCachePolicy
            // 
            this.label_rfid_tagCachePolicy.AutoSize = true;
            this.label_rfid_tagCachePolicy.Enabled = false;
            this.label_rfid_tagCachePolicy.Location = new System.Drawing.Point(7, 129);
            this.label_rfid_tagCachePolicy.Name = "label_rfid_tagCachePolicy";
            this.label_rfid_tagCachePolicy.Size = new System.Drawing.Size(180, 21);
            this.label_rfid_tagCachePolicy.TabIndex = 3;
            this.label_rfid_tagCachePolicy.Text = "标签缓存策略(&C):";
            // 
            // groupBox_uhf
            // 
            this.groupBox_uhf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_uhf.Controls.Add(this.label42);
            this.groupBox_uhf.Controls.Add(this.numericUpDown_rfid_inventoryIdleSeconds);
            this.groupBox_uhf.Controls.Add(this.checkBox_uhf_onlyEpcCharging);
            this.groupBox_uhf.Controls.Add(this.label41);
            this.groupBox_uhf.Controls.Add(this.label40);
            this.groupBox_uhf.Controls.Add(this.numericUpDown_uhf_rssi);
            this.groupBox_uhf.Controls.Add(this.checkedComboBox_uhf_elements);
            this.groupBox_uhf.Controls.Add(this.label_uhf_elements);
            this.groupBox_uhf.Controls.Add(this.checkBox_uhf_bookTagWriteUserBank);
            this.groupBox_uhf.Controls.Add(this.checkBox_uhf_warningWhenDataFormatMismatch);
            this.groupBox_uhf.Controls.Add(this.comboBox_uhf_dataFormat);
            this.groupBox_uhf.Controls.Add(this.label39);
            this.groupBox_uhf.Enabled = false;
            this.groupBox_uhf.Location = new System.Drawing.Point(11, 181);
            this.groupBox_uhf.Name = "groupBox_uhf";
            this.groupBox_uhf.Size = new System.Drawing.Size(840, 356);
            this.groupBox_uhf.TabIndex = 2;
            this.groupBox_uhf.TabStop = false;
            this.groupBox_uhf.Text = " UHF(超高频)标签 ";
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.BackColor = System.Drawing.Color.DarkGray;
            this.label42.ForeColor = System.Drawing.Color.White;
            this.label42.Location = new System.Drawing.Point(16, 308);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(180, 21);
            this.label42.TabIndex = 10;
            this.label42.Text = "轮询间隔秒数(&L):";
            // 
            // numericUpDown_rfid_inventoryIdleSeconds
            // 
            this.numericUpDown_rfid_inventoryIdleSeconds.BackColor = System.Drawing.Color.DarkGray;
            this.numericUpDown_rfid_inventoryIdleSeconds.ForeColor = System.Drawing.Color.White;
            this.numericUpDown_rfid_inventoryIdleSeconds.Location = new System.Drawing.Point(215, 306);
            this.numericUpDown_rfid_inventoryIdleSeconds.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown_rfid_inventoryIdleSeconds.Name = "numericUpDown_rfid_inventoryIdleSeconds";
            this.numericUpDown_rfid_inventoryIdleSeconds.Size = new System.Drawing.Size(155, 31);
            this.numericUpDown_rfid_inventoryIdleSeconds.TabIndex = 11;
            // 
            // checkBox_uhf_onlyEpcCharging
            // 
            this.checkBox_uhf_onlyEpcCharging.AutoSize = true;
            this.checkBox_uhf_onlyEpcCharging.Location = new System.Drawing.Point(20, 264);
            this.checkBox_uhf_onlyEpcCharging.Name = "checkBox_uhf_onlyEpcCharging";
            this.checkBox_uhf_onlyEpcCharging.Size = new System.Drawing.Size(324, 25);
            this.checkBox_uhf_onlyEpcCharging.TabIndex = 9;
            this.checkBox_uhf_onlyEpcCharging.Text = "“仅读入 EPC Bank”加速借还";
            this.checkBox_uhf_onlyEpcCharging.UseVisualStyleBackColor = true;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(376, 215);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(159, 21);
            this.label41.TabIndex = 8;
            this.label41.Text = "(0 表示不过滤)";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(16, 215);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(151, 21);
            this.label40.TabIndex = 6;
            this.label40.Text = "RSSI 阈值(&R):";
            // 
            // numericUpDown_uhf_rssi
            // 
            this.numericUpDown_uhf_rssi.Location = new System.Drawing.Point(215, 213);
            this.numericUpDown_uhf_rssi.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDown_uhf_rssi.Name = "numericUpDown_uhf_rssi";
            this.numericUpDown_uhf_rssi.Size = new System.Drawing.Size(155, 31);
            this.numericUpDown_uhf_rssi.TabIndex = 7;
            // 
            // checkedComboBox_uhf_elements
            // 
            this.checkedComboBox_uhf_elements.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_uhf_elements.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_uhf_elements.Location = new System.Drawing.Point(215, 121);
            this.checkedComboBox_uhf_elements.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_uhf_elements.Name = "checkedComboBox_uhf_elements";
            this.checkedComboBox_uhf_elements.ReadOnly = false;
            this.checkedComboBox_uhf_elements.Size = new System.Drawing.Size(619, 24);
            this.checkedComboBox_uhf_elements.TabIndex = 4;
            this.checkedComboBox_uhf_elements.TextChanged += new System.EventHandler(this.checkedComboBox_uhf_elements_TextChanged);
            // 
            // label_uhf_elements
            // 
            this.label_uhf_elements.AutoSize = true;
            this.label_uhf_elements.Location = new System.Drawing.Point(46, 121);
            this.label_uhf_elements.Name = "label_uhf_elements";
            this.label_uhf_elements.Size = new System.Drawing.Size(138, 21);
            this.label_uhf_elements.TabIndex = 3;
            this.label_uhf_elements.Text = "的元素名(&E):";
            // 
            // checkBox_uhf_bookTagWriteUserBank
            // 
            this.checkBox_uhf_bookTagWriteUserBank.AutoSize = true;
            this.checkBox_uhf_bookTagWriteUserBank.Location = new System.Drawing.Point(20, 93);
            this.checkBox_uhf_bookTagWriteUserBank.Name = "checkBox_uhf_bookTagWriteUserBank";
            this.checkBox_uhf_bookTagWriteUserBank.Size = new System.Drawing.Size(326, 25);
            this.checkBox_uhf_bookTagWriteUserBank.TabIndex = 2;
            this.checkBox_uhf_bookTagWriteUserBank.Text = "图书标签要写入 User Bank(&U)";
            this.checkBox_uhf_bookTagWriteUserBank.UseVisualStyleBackColor = true;
            this.checkBox_uhf_bookTagWriteUserBank.CheckedChanged += new System.EventHandler(this.checkBox_uhf_bookTagWriteUserBank_CheckedChanged);
            // 
            // checkBox_uhf_warningWhenDataFormatMismatch
            // 
            this.checkBox_uhf_warningWhenDataFormatMismatch.AutoSize = true;
            this.checkBox_uhf_warningWhenDataFormatMismatch.Location = new System.Drawing.Point(20, 174);
            this.checkBox_uhf_warningWhenDataFormatMismatch.Name = "checkBox_uhf_warningWhenDataFormatMismatch";
            this.checkBox_uhf_warningWhenDataFormatMismatch.Size = new System.Drawing.Size(384, 25);
            this.checkBox_uhf_warningWhenDataFormatMismatch.TabIndex = 5;
            this.checkBox_uhf_warningWhenDataFormatMismatch.Text = "覆盖格式不同的标签原内容前警告(&W)";
            this.checkBox_uhf_warningWhenDataFormatMismatch.UseVisualStyleBackColor = true;
            // 
            // comboBox_uhf_dataFormat
            // 
            this.comboBox_uhf_dataFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_uhf_dataFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_uhf_dataFormat.FormattingEnabled = true;
            this.comboBox_uhf_dataFormat.Items.AddRange(new object[] {
            "高校联盟格式",
            "国标格式",
            "望湖洞庭"});
            this.comboBox_uhf_dataFormat.Location = new System.Drawing.Point(215, 38);
            this.comboBox_uhf_dataFormat.Name = "comboBox_uhf_dataFormat";
            this.comboBox_uhf_dataFormat.Size = new System.Drawing.Size(619, 29);
            this.comboBox_uhf_dataFormat.TabIndex = 1;
            this.comboBox_uhf_dataFormat.SelectedIndexChanged += new System.EventHandler(this.comboBox_uhf_dataFormat_SelectedIndexChanged);
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(16, 41);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(138, 21);
            this.label39.TabIndex = 0;
            this.label39.Text = "写入格式(&S):";
            // 
            // button_cardReader_setRfidUrlDefaultValue
            // 
            this.button_cardReader_setRfidUrlDefaultValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cardReader_setRfidUrlDefaultValue.Location = new System.Drawing.Point(661, 72);
            this.button_cardReader_setRfidUrlDefaultValue.Margin = new System.Windows.Forms.Padding(5);
            this.button_cardReader_setRfidUrlDefaultValue.Name = "button_cardReader_setRfidUrlDefaultValue";
            this.button_cardReader_setRfidUrlDefaultValue.Size = new System.Drawing.Size(193, 40);
            this.button_cardReader_setRfidUrlDefaultValue.TabIndex = 1;
            this.button_cardReader_setRfidUrlDefaultValue.Text = "设为常用值";
            this.button_cardReader_setRfidUrlDefaultValue.UseVisualStyleBackColor = true;
            this.button_cardReader_setRfidUrlDefaultValue.Click += new System.EventHandler(this.button_cardReader_setRfidUrlDefaultValue_Click);
            // 
            // textBox_cardReader_rfidCenterUrl
            // 
            this.textBox_cardReader_rfidCenterUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cardReader_rfidCenterUrl.Location = new System.Drawing.Point(11, 35);
            this.textBox_cardReader_rfidCenterUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cardReader_rfidCenterUrl.Name = "textBox_cardReader_rfidCenterUrl";
            this.textBox_cardReader_rfidCenterUrl.Size = new System.Drawing.Size(840, 31);
            this.textBox_cardReader_rfidCenterUrl.TabIndex = 0;
            this.textBox_cardReader_rfidCenterUrl.TextChanged += new System.EventHandler(this.textBox_cardReader_rfidCenterUrl_TextChanged);
            // 
            // groupBox_idcardReader
            // 
            this.groupBox_idcardReader.Controls.Add(this.button_cardReader_setIdcardUrlDefaultValue);
            this.groupBox_idcardReader.Controls.Add(this.textBox_cardReader_idcardReaderUrl);
            this.groupBox_idcardReader.Location = new System.Drawing.Point(5, 28);
            this.groupBox_idcardReader.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_idcardReader.Name = "groupBox_idcardReader";
            this.groupBox_idcardReader.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_idcardReader.Size = new System.Drawing.Size(865, 154);
            this.groupBox_idcardReader.TabIndex = 0;
            this.groupBox_idcardReader.TabStop = false;
            this.groupBox_idcardReader.Text = " 身份证读卡器接口 URL ";
            // 
            // button_cardReader_setIdcardUrlDefaultValue
            // 
            this.button_cardReader_setIdcardUrlDefaultValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cardReader_setIdcardUrlDefaultValue.Location = new System.Drawing.Point(661, 72);
            this.button_cardReader_setIdcardUrlDefaultValue.Margin = new System.Windows.Forms.Padding(5);
            this.button_cardReader_setIdcardUrlDefaultValue.Name = "button_cardReader_setIdcardUrlDefaultValue";
            this.button_cardReader_setIdcardUrlDefaultValue.Size = new System.Drawing.Size(193, 40);
            this.button_cardReader_setIdcardUrlDefaultValue.TabIndex = 1;
            this.button_cardReader_setIdcardUrlDefaultValue.Text = "设为常用值";
            this.button_cardReader_setIdcardUrlDefaultValue.UseVisualStyleBackColor = true;
            this.button_cardReader_setIdcardUrlDefaultValue.Click += new System.EventHandler(this.button_cardReader_setIdcardUrlDefaultValue_Click);
            // 
            // textBox_cardReader_idcardReaderUrl
            // 
            this.textBox_cardReader_idcardReaderUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cardReader_idcardReaderUrl.Location = new System.Drawing.Point(11, 35);
            this.textBox_cardReader_idcardReaderUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cardReader_idcardReaderUrl.Name = "textBox_cardReader_idcardReaderUrl";
            this.textBox_cardReader_idcardReaderUrl.Size = new System.Drawing.Size(840, 31);
            this.textBox_cardReader_idcardReaderUrl.TabIndex = 0;
            // 
            // tabPage_patron
            // 
            this.tabPage_patron.AutoScroll = true;
            this.tabPage_patron.Controls.Add(this.checkBox_patron_disableBioKeyboardSimulation);
            this.tabPage_patron.Controls.Add(this.checkBox_patron_disableIdcardReaderKeyboardSimulation);
            this.tabPage_patron.Controls.Add(this.checkBox_patron_autoRetryReaderCard);
            this.tabPage_patron.Controls.Add(this.checkBox_patron_verifyBarcode);
            this.tabPage_patron.Controls.Add(this.checkBox_patron_displaySetReaderBarcodeDialog);
            this.tabPage_patron.Location = new System.Drawing.Point(4, 85);
            this.tabPage_patron.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_patron.Name = "tabPage_patron";
            this.tabPage_patron.Size = new System.Drawing.Size(905, 459);
            this.tabPage_patron.TabIndex = 13;
            this.tabPage_patron.Text = "读者";
            this.tabPage_patron.UseVisualStyleBackColor = true;
            // 
            // checkBox_patron_disableBioKeyboardSimulation
            // 
            this.checkBox_patron_disableBioKeyboardSimulation.AutoSize = true;
            this.checkBox_patron_disableBioKeyboardSimulation.Location = new System.Drawing.Point(5, 241);
            this.checkBox_patron_disableBioKeyboardSimulation.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_patron_disableBioKeyboardSimulation.Name = "checkBox_patron_disableBioKeyboardSimulation";
            this.checkBox_patron_disableBioKeyboardSimulation.Size = new System.Drawing.Size(511, 25);
            this.checkBox_patron_disableBioKeyboardSimulation.TabIndex = 4;
            this.checkBox_patron_disableBioKeyboardSimulation.Text = "当读者窗活动时自动关闭 掌纹、指纹 键盘仿真(&S)";
            this.checkBox_patron_disableBioKeyboardSimulation.UseVisualStyleBackColor = true;
            // 
            // checkBox_patron_disableIdcardReaderKeyboardSimulation
            // 
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.AutoSize = true;
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Location = new System.Drawing.Point(5, 206);
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Name = "checkBox_patron_disableIdcardReaderKeyboardSimulation";
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Size = new System.Drawing.Size(532, 25);
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.TabIndex = 3;
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Text = "当读者窗活动时自动关闭 身份证读卡器 键盘仿真(&S)";
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.UseVisualStyleBackColor = true;
            // 
            // checkBox_patron_autoRetryReaderCard
            // 
            this.checkBox_patron_autoRetryReaderCard.AutoSize = true;
            this.checkBox_patron_autoRetryReaderCard.Location = new System.Drawing.Point(5, 47);
            this.checkBox_patron_autoRetryReaderCard.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_patron_autoRetryReaderCard.Name = "checkBox_patron_autoRetryReaderCard";
            this.checkBox_patron_autoRetryReaderCard.Size = new System.Drawing.Size(353, 25);
            this.checkBox_patron_autoRetryReaderCard.TabIndex = 0;
            this.checkBox_patron_autoRetryReaderCard.Text = "当读卡对话框出现时 自动重试(&A)";
            this.checkBox_patron_autoRetryReaderCard.UseVisualStyleBackColor = true;
            // 
            // checkBox_patron_verifyBarcode
            // 
            this.checkBox_patron_verifyBarcode.AutoSize = true;
            this.checkBox_patron_verifyBarcode.Location = new System.Drawing.Point(5, 147);
            this.checkBox_patron_verifyBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_patron_verifyBarcode.Name = "checkBox_patron_verifyBarcode";
            this.checkBox_patron_verifyBarcode.Size = new System.Drawing.Size(237, 25);
            this.checkBox_patron_verifyBarcode.TabIndex = 2;
            this.checkBox_patron_verifyBarcode.Text = "校验输入的条码号(&V)";
            this.checkBox_patron_verifyBarcode.UseVisualStyleBackColor = true;
            // 
            // checkBox_patron_displaySetReaderBarcodeDialog
            // 
            this.checkBox_patron_displaySetReaderBarcodeDialog.AutoSize = true;
            this.checkBox_patron_displaySetReaderBarcodeDialog.Location = new System.Drawing.Point(5, 86);
            this.checkBox_patron_displaySetReaderBarcodeDialog.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_patron_displaySetReaderBarcodeDialog.Name = "checkBox_patron_displaySetReaderBarcodeDialog";
            this.checkBox_patron_displaySetReaderBarcodeDialog.Size = new System.Drawing.Size(427, 25);
            this.checkBox_patron_displaySetReaderBarcodeDialog.TabIndex = 1;
            this.checkBox_patron_displaySetReaderBarcodeDialog.Text = "出现 用身份证号设置证条码号 对话框(&I)";
            this.checkBox_patron_displaySetReaderBarcodeDialog.UseVisualStyleBackColor = true;
            // 
            // tabPage_operLog
            // 
            this.tabPage_operLog.Controls.Add(this.comboBox_operLog_level);
            this.tabPage_operLog.Controls.Add(this.label25);
            this.tabPage_operLog.Controls.Add(this.checkBox_operLog_autoCache);
            this.tabPage_operLog.Controls.Add(this.button_operLog_clearCacheDirectory);
            this.tabPage_operLog.Controls.Add(this.checkBox_operLog_displayItemBorrowHistory);
            this.tabPage_operLog.Controls.Add(this.checkBox_operLog_displayReaderBorrowHistory);
            this.tabPage_operLog.Location = new System.Drawing.Point(4, 85);
            this.tabPage_operLog.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_operLog.Name = "tabPage_operLog";
            this.tabPage_operLog.Size = new System.Drawing.Size(905, 459);
            this.tabPage_operLog.TabIndex = 15;
            this.tabPage_operLog.Text = "日志";
            this.tabPage_operLog.UseVisualStyleBackColor = true;
            // 
            // comboBox_operLog_level
            // 
            this.comboBox_operLog_level.FormattingEnabled = true;
            this.comboBox_operLog_level.Items.AddRange(new object[] {
            "0 -- 完整",
            "1 -- 简略",
            "2 -- 最简略"});
            this.comboBox_operLog_level.Location = new System.Drawing.Point(291, 243);
            this.comboBox_operLog_level.Margin = new System.Windows.Forms.Padding(5);
            this.comboBox_operLog_level.Name = "comboBox_operLog_level";
            this.comboBox_operLog_level.Size = new System.Drawing.Size(338, 29);
            this.comboBox_operLog_level.TabIndex = 5;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(7, 248);
            this.label25.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(264, 21);
            this.label25.TabIndex = 4;
            this.label25.Text = "获取日志时的详细级别(&L):";
            // 
            // checkBox_operLog_autoCache
            // 
            this.checkBox_operLog_autoCache.AutoSize = true;
            this.checkBox_operLog_autoCache.Location = new System.Drawing.Point(5, 129);
            this.checkBox_operLog_autoCache.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_operLog_autoCache.Name = "checkBox_operLog_autoCache";
            this.checkBox_operLog_autoCache.Size = new System.Drawing.Size(237, 25);
            this.checkBox_operLog_autoCache.TabIndex = 3;
            this.checkBox_operLog_autoCache.Text = "自动缓存日志文件(&A)";
            this.checkBox_operLog_autoCache.UseVisualStyleBackColor = true;
            // 
            // button_operLog_clearCacheDirectory
            // 
            this.button_operLog_clearCacheDirectory.Location = new System.Drawing.Point(5, 168);
            this.button_operLog_clearCacheDirectory.Margin = new System.Windows.Forms.Padding(5);
            this.button_operLog_clearCacheDirectory.Name = "button_operLog_clearCacheDirectory";
            this.button_operLog_clearCacheDirectory.Size = new System.Drawing.Size(367, 40);
            this.button_operLog_clearCacheDirectory.TabIndex = 2;
            this.button_operLog_clearCacheDirectory.Text = "清空日志本地缓存目录(&C)";
            this.button_operLog_clearCacheDirectory.UseVisualStyleBackColor = true;
            this.button_operLog_clearCacheDirectory.Click += new System.EventHandler(this.button_operLog_clearCacheDirectory_Click);
            // 
            // checkBox_operLog_displayItemBorrowHistory
            // 
            this.checkBox_operLog_displayItemBorrowHistory.AutoSize = true;
            this.checkBox_operLog_displayItemBorrowHistory.Location = new System.Drawing.Point(5, 72);
            this.checkBox_operLog_displayItemBorrowHistory.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_operLog_displayItemBorrowHistory.Name = "checkBox_operLog_displayItemBorrowHistory";
            this.checkBox_operLog_displayItemBorrowHistory.Size = new System.Drawing.Size(470, 25);
            this.checkBox_operLog_displayItemBorrowHistory.TabIndex = 1;
            this.checkBox_operLog_displayItemBorrowHistory.Text = "显示册借阅历史 [在册记录的解释内容中] (&I)";
            this.checkBox_operLog_displayItemBorrowHistory.UseVisualStyleBackColor = true;
            // 
            // checkBox_operLog_displayReaderBorrowHistory
            // 
            this.checkBox_operLog_displayReaderBorrowHistory.AutoSize = true;
            this.checkBox_operLog_displayReaderBorrowHistory.Location = new System.Drawing.Point(5, 33);
            this.checkBox_operLog_displayReaderBorrowHistory.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_operLog_displayReaderBorrowHistory.Name = "checkBox_operLog_displayReaderBorrowHistory";
            this.checkBox_operLog_displayReaderBorrowHistory.Size = new System.Drawing.Size(512, 25);
            this.checkBox_operLog_displayReaderBorrowHistory.TabIndex = 0;
            this.checkBox_operLog_displayReaderBorrowHistory.Text = "显示读者借阅历史 [在读者记录的解释内容中] (&H)";
            this.checkBox_operLog_displayReaderBorrowHistory.UseVisualStyleBackColor = true;
            // 
            // tabPage_global
            // 
            this.tabPage_global.AutoScroll = true;
            this.tabPage_global.Controls.Add(this.checkBox_global_disableSpeak);
            this.tabPage_global.Controls.Add(this.checkedComboBox_global_securityProtocol);
            this.tabPage_global.Controls.Add(this.label37);
            this.tabPage_global.Controls.Add(this.textBox_global_additionalLocations);
            this.tabPage_global.Controls.Add(this.label35);
            this.tabPage_global.Controls.Add(this.checkBox_global_upperInputBarcode);
            this.tabPage_global.Controls.Add(this.checkBox_global_saveOriginCoverImage);
            this.tabPage_global.Controls.Add(this.label26);
            this.tabPage_global.Controls.Add(this.checkBox_global_autoSelPinyin);
            this.tabPage_global.Controls.Add(this.checkBox_global_displayScriptErrorDialog);
            this.tabPage_global.Location = new System.Drawing.Point(4, 85);
            this.tabPage_global.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_global.Name = "tabPage_global";
            this.tabPage_global.Size = new System.Drawing.Size(905, 459);
            this.tabPage_global.TabIndex = 14;
            this.tabPage_global.Text = "全局";
            this.tabPage_global.UseVisualStyleBackColor = true;
            // 
            // checkBox_global_disableSpeak
            // 
            this.checkBox_global_disableSpeak.AutoSize = true;
            this.checkBox_global_disableSpeak.Location = new System.Drawing.Point(7, 431);
            this.checkBox_global_disableSpeak.Name = "checkBox_global_disableSpeak";
            this.checkBox_global_disableSpeak.Size = new System.Drawing.Size(153, 25);
            this.checkBox_global_disableSpeak.TabIndex = 24;
            this.checkBox_global_disableSpeak.Text = "禁用朗读(&D)";
            this.checkBox_global_disableSpeak.UseVisualStyleBackColor = true;
            // 
            // checkedComboBox_global_securityProtocol
            // 
            this.checkedComboBox_global_securityProtocol.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_global_securityProtocol.Location = new System.Drawing.Point(153, 390);
            this.checkedComboBox_global_securityProtocol.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_global_securityProtocol.Name = "checkedComboBox_global_securityProtocol";
            this.checkedComboBox_global_securityProtocol.ReadOnly = false;
            this.checkedComboBox_global_securityProtocol.Size = new System.Drawing.Size(374, 24);
            this.checkedComboBox_global_securityProtocol.TabIndex = 23;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(3, 390);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(138, 21);
            this.label37.TabIndex = 22;
            this.label37.Text = "加密协议(&S):";
            // 
            // textBox_global_additionalLocations
            // 
            this.textBox_global_additionalLocations.AcceptsReturn = true;
            this.textBox_global_additionalLocations.Location = new System.Drawing.Point(5, 247);
            this.textBox_global_additionalLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_global_additionalLocations.Multiline = true;
            this.textBox_global_additionalLocations.Name = "textBox_global_additionalLocations";
            this.textBox_global_additionalLocations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_global_additionalLocations.Size = new System.Drawing.Size(522, 114);
            this.textBox_global_additionalLocations.TabIndex = 20;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(1, 221);
            this.label35.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(263, 21);
            this.label35.TabIndex = 19;
            this.label35.Text = "附加的馆藏地：[每行一个]";
            // 
            // checkBox_global_upperInputBarcode
            // 
            this.checkBox_global_upperInputBarcode.AutoSize = true;
            this.checkBox_global_upperInputBarcode.Location = new System.Drawing.Point(5, 163);
            this.checkBox_global_upperInputBarcode.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_global_upperInputBarcode.Name = "checkBox_global_upperInputBarcode";
            this.checkBox_global_upperInputBarcode.Size = new System.Drawing.Size(384, 25);
            this.checkBox_global_upperInputBarcode.TabIndex = 18;
            this.checkBox_global_upperInputBarcode.Text = "将键盘输入的条码号自动转为大写(&U)";
            this.checkBox_global_upperInputBarcode.UseVisualStyleBackColor = true;
            // 
            // checkBox_global_saveOriginCoverImage
            // 
            this.checkBox_global_saveOriginCoverImage.AutoSize = true;
            this.checkBox_global_saveOriginCoverImage.Location = new System.Drawing.Point(5, 119);
            this.checkBox_global_saveOriginCoverImage.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_global_saveOriginCoverImage.Name = "checkBox_global_saveOriginCoverImage";
            this.checkBox_global_saveOriginCoverImage.Size = new System.Drawing.Size(279, 25);
            this.checkBox_global_saveOriginCoverImage.TabIndex = 17;
            this.checkBox_global_saveOriginCoverImage.Text = "保存封面扫描原始图像(&O)";
            this.checkBox_global_saveOriginCoverImage.UseVisualStyleBackColor = true;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(401, 28);
            this.label26.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(326, 21);
            this.label26.TabIndex = 16;
            this.label26.Text = "[本项参数在程序退出后自动清除]";
            // 
            // checkBox_global_autoSelPinyin
            // 
            this.checkBox_global_autoSelPinyin.AutoSize = true;
            this.checkBox_global_autoSelPinyin.Location = new System.Drawing.Point(5, 71);
            this.checkBox_global_autoSelPinyin.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_global_autoSelPinyin.Name = "checkBox_global_autoSelPinyin";
            this.checkBox_global_autoSelPinyin.Size = new System.Drawing.Size(300, 25);
            this.checkBox_global_autoSelPinyin.TabIndex = 15;
            this.checkBox_global_autoSelPinyin.Text = "加拼音时自动选择多音字(&A)";
            this.checkBox_global_autoSelPinyin.UseVisualStyleBackColor = true;
            // 
            // checkBox_global_displayScriptErrorDialog
            // 
            this.checkBox_global_displayScriptErrorDialog.AutoSize = true;
            this.checkBox_global_displayScriptErrorDialog.Location = new System.Drawing.Point(5, 26);
            this.checkBox_global_displayScriptErrorDialog.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_global_displayScriptErrorDialog.Name = "checkBox_global_displayScriptErrorDialog";
            this.checkBox_global_displayScriptErrorDialog.Size = new System.Drawing.Size(363, 25);
            this.checkBox_global_displayScriptErrorDialog.TabIndex = 14;
            this.checkBox_global_displayScriptErrorDialog.Text = "浏览器控件允许脚本错误对话框(&S)";
            this.checkBox_global_displayScriptErrorDialog.UseVisualStyleBackColor = true;
            // 
            // tabPage_fingerprint
            // 
            this.tabPage_fingerprint.AutoScroll = true;
            this.tabPage_fingerprint.Controls.Add(this.groupBox_palmprintUrl);
            this.tabPage_fingerprint.Controls.Add(this.groupBox_face);
            this.tabPage_fingerprint.Controls.Add(this.groupBox9);
            this.tabPage_fingerprint.Controls.Add(this.groupBox_fingerprint);
            this.tabPage_fingerprint.Controls.Add(this.button_fingerprint_clearLocalCacheFiles);
            this.tabPage_fingerprint.Location = new System.Drawing.Point(4, 85);
            this.tabPage_fingerprint.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_fingerprint.Name = "tabPage_fingerprint";
            this.tabPage_fingerprint.Size = new System.Drawing.Size(905, 459);
            this.tabPage_fingerprint.TabIndex = 16;
            this.tabPage_fingerprint.Text = "指纹、掌纹和人脸";
            this.tabPage_fingerprint.UseVisualStyleBackColor = true;
            // 
            // groupBox_palmprintUrl
            // 
            this.groupBox_palmprintUrl.Controls.Add(this.button_fingerprint_setDefaultValue_new);
            this.groupBox_palmprintUrl.Controls.Add(this.button_palmprint_setDefaulValue);
            this.groupBox_palmprintUrl.Controls.Add(this.textBox_palmprint_readerUrl);
            this.groupBox_palmprintUrl.Location = new System.Drawing.Point(5, 184);
            this.groupBox_palmprintUrl.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_palmprintUrl.Name = "groupBox_palmprintUrl";
            this.groupBox_palmprintUrl.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_palmprintUrl.Size = new System.Drawing.Size(865, 147);
            this.groupBox_palmprintUrl.TabIndex = 4;
            this.groupBox_palmprintUrl.TabStop = false;
            this.groupBox_palmprintUrl.Text = "掌纹阅读器接口 URL ";
            // 
            // button_fingerprint_setDefaultValue_new
            // 
            this.button_fingerprint_setDefaultValue_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_fingerprint_setDefaultValue_new.Location = new System.Drawing.Point(469, 70);
            this.button_fingerprint_setDefaultValue_new.Margin = new System.Windows.Forms.Padding(5);
            this.button_fingerprint_setDefaultValue_new.Name = "button_fingerprint_setDefaultValue_new";
            this.button_fingerprint_setDefaultValue_new.Size = new System.Drawing.Size(193, 40);
            this.button_fingerprint_setDefaultValue_new.TabIndex = 2;
            this.button_fingerprint_setDefaultValue_new.Text = "设为指纹常用值";
            this.button_fingerprint_setDefaultValue_new.UseVisualStyleBackColor = true;
            this.button_fingerprint_setDefaultValue_new.Click += new System.EventHandler(this.button_fingerprint_setDefaultValue_new_Click);
            // 
            // button_palmprint_setDefaulValue
            // 
            this.button_palmprint_setDefaulValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_palmprint_setDefaulValue.Location = new System.Drawing.Point(662, 69);
            this.button_palmprint_setDefaulValue.Margin = new System.Windows.Forms.Padding(5);
            this.button_palmprint_setDefaulValue.Name = "button_palmprint_setDefaulValue";
            this.button_palmprint_setDefaulValue.Size = new System.Drawing.Size(193, 40);
            this.button_palmprint_setDefaulValue.TabIndex = 1;
            this.button_palmprint_setDefaulValue.Text = "设为掌纹常用值";
            this.button_palmprint_setDefaulValue.UseVisualStyleBackColor = true;
            this.button_palmprint_setDefaulValue.Click += new System.EventHandler(this.button_palmprint_setDefaulValue_Click);
            // 
            // textBox_palmprint_readerUrl
            // 
            this.textBox_palmprint_readerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_palmprint_readerUrl.Location = new System.Drawing.Point(11, 35);
            this.textBox_palmprint_readerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_palmprint_readerUrl.Name = "textBox_palmprint_readerUrl";
            this.textBox_palmprint_readerUrl.Size = new System.Drawing.Size(844, 31);
            this.textBox_palmprint_readerUrl.TabIndex = 0;
            // 
            // groupBox_face
            // 
            this.groupBox_face.Controls.Add(this.checkBox_face_savePhotoWhileRegister);
            this.groupBox_face.Controls.Add(this.linkLabel_installFaceCenter);
            this.groupBox_face.Controls.Add(this.button_face_setDefaultValue);
            this.groupBox_face.Controls.Add(this.textBox_face_readerUrl);
            this.groupBox_face.Location = new System.Drawing.Point(5, 341);
            this.groupBox_face.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_face.Name = "groupBox_face";
            this.groupBox_face.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_face.Size = new System.Drawing.Size(865, 147);
            this.groupBox_face.TabIndex = 3;
            this.groupBox_face.TabStop = false;
            this.groupBox_face.Text = "人脸识别接口 URL ";
            // 
            // checkBox_face_savePhotoWhileRegister
            // 
            this.checkBox_face_savePhotoWhileRegister.AutoSize = true;
            this.checkBox_face_savePhotoWhileRegister.Location = new System.Drawing.Point(9, 73);
            this.checkBox_face_savePhotoWhileRegister.Name = "checkBox_face_savePhotoWhileRegister";
            this.checkBox_face_savePhotoWhileRegister.Size = new System.Drawing.Size(258, 25);
            this.checkBox_face_savePhotoWhileRegister.TabIndex = 3;
            this.checkBox_face_savePhotoWhileRegister.Text = "注册时保留人脸照片(&S)";
            this.checkBox_face_savePhotoWhileRegister.UseVisualStyleBackColor = true;
            // 
            // linkLabel_installFaceCenter
            // 
            this.linkLabel_installFaceCenter.AutoSize = true;
            this.linkLabel_installFaceCenter.Location = new System.Drawing.Point(8, 101);
            this.linkLabel_installFaceCenter.Name = "linkLabel_installFaceCenter";
            this.linkLabel_installFaceCenter.Size = new System.Drawing.Size(321, 21);
            this.linkLabel_installFaceCenter.TabIndex = 2;
            this.linkLabel_installFaceCenter.TabStop = true;
            this.linkLabel_installFaceCenter.Text = "下载安装 人脸中心(FaceCenter)";
            this.linkLabel_installFaceCenter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_installFaceCenter_LinkClicked);
            // 
            // button_face_setDefaultValue
            // 
            this.button_face_setDefaultValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_face_setDefaultValue.Location = new System.Drawing.Point(662, 67);
            this.button_face_setDefaultValue.Margin = new System.Windows.Forms.Padding(5);
            this.button_face_setDefaultValue.Name = "button_face_setDefaultValue";
            this.button_face_setDefaultValue.Size = new System.Drawing.Size(193, 40);
            this.button_face_setDefaultValue.TabIndex = 1;
            this.button_face_setDefaultValue.Text = "设为常用值";
            this.button_face_setDefaultValue.UseVisualStyleBackColor = true;
            this.button_face_setDefaultValue.Click += new System.EventHandler(this.button_face_setDefaultValue_Click);
            // 
            // textBox_face_readerUrl
            // 
            this.textBox_face_readerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_face_readerUrl.Location = new System.Drawing.Point(11, 35);
            this.textBox_face_readerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_face_readerUrl.Name = "textBox_face_readerUrl";
            this.textBox_face_readerUrl.Size = new System.Drawing.Size(844, 31);
            this.textBox_face_readerUrl.TabIndex = 0;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.textBox_fingerprint_password);
            this.groupBox9.Controls.Add(this.textBox_fingerprint_userName);
            this.groupBox9.Controls.Add(this.label21);
            this.groupBox9.Controls.Add(this.label24);
            this.groupBox9.Location = new System.Drawing.Point(17, 546);
            this.groupBox9.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox9.Size = new System.Drawing.Size(565, 168);
            this.groupBox9.TabIndex = 2;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = " 代理帐户(用于初始化指纹缓存) ";
            // 
            // textBox_fingerprint_password
            // 
            this.textBox_fingerprint_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_fingerprint_password.Location = new System.Drawing.Point(230, 100);
            this.textBox_fingerprint_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_fingerprint_password.Name = "textBox_fingerprint_password";
            this.textBox_fingerprint_password.PasswordChar = '*';
            this.textBox_fingerprint_password.Size = new System.Drawing.Size(283, 31);
            this.textBox_fingerprint_password.TabIndex = 8;
            // 
            // textBox_fingerprint_userName
            // 
            this.textBox_fingerprint_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_fingerprint_userName.Location = new System.Drawing.Point(230, 52);
            this.textBox_fingerprint_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_fingerprint_userName.Name = "textBox_fingerprint_userName";
            this.textBox_fingerprint_userName.Size = new System.Drawing.Size(283, 31);
            this.textBox_fingerprint_userName.TabIndex = 6;
            this.textBox_fingerprint_userName.TextChanged += new System.EventHandler(this.textBox_fingerprint_userName_TextChanged);
            // 
            // label21
            // 
            this.label21.Location = new System.Drawing.Point(51, 105);
            this.label21.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(137, 31);
            this.label21.TabIndex = 7;
            this.label21.Text = "密码(&P)：";
            // 
            // label24
            // 
            this.label24.Location = new System.Drawing.Point(51, 58);
            this.label24.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(137, 31);
            this.label24.TabIndex = 5;
            this.label24.Text = "用户名(&U)：";
            // 
            // groupBox_fingerprint
            // 
            this.groupBox_fingerprint.Controls.Add(this.linkLabel_installFingerprintCenter);
            this.groupBox_fingerprint.Controls.Add(this.button_fingerprint_setDefaultValue);
            this.groupBox_fingerprint.Controls.Add(this.textBox_fingerprint_readerUrl);
            this.groupBox_fingerprint.Location = new System.Drawing.Point(5, 24);
            this.groupBox_fingerprint.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox_fingerprint.Name = "groupBox_fingerprint";
            this.groupBox_fingerprint.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox_fingerprint.Size = new System.Drawing.Size(865, 147);
            this.groupBox_fingerprint.TabIndex = 0;
            this.groupBox_fingerprint.TabStop = false;
            this.groupBox_fingerprint.Text = " 指纹阅读器接口 URL ";
            // 
            // linkLabel_installFingerprintCenter
            // 
            this.linkLabel_installFingerprintCenter.AutoSize = true;
            this.linkLabel_installFingerprintCenter.Location = new System.Drawing.Point(7, 71);
            this.linkLabel_installFingerprintCenter.Name = "linkLabel_installFingerprintCenter";
            this.linkLabel_installFingerprintCenter.Size = new System.Drawing.Size(398, 21);
            this.linkLabel_installFingerprintCenter.TabIndex = 3;
            this.linkLabel_installFingerprintCenter.TabStop = true;
            this.linkLabel_installFingerprintCenter.Text = "下载安装 指纹中心(FingerprintCenter)";
            this.linkLabel_installFingerprintCenter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_installFingerprintCenter_LinkClicked);
            // 
            // button_fingerprint_setDefaultValue
            // 
            this.button_fingerprint_setDefaultValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_fingerprint_setDefaultValue.Location = new System.Drawing.Point(662, 67);
            this.button_fingerprint_setDefaultValue.Margin = new System.Windows.Forms.Padding(5);
            this.button_fingerprint_setDefaultValue.Name = "button_fingerprint_setDefaultValue";
            this.button_fingerprint_setDefaultValue.Size = new System.Drawing.Size(193, 40);
            this.button_fingerprint_setDefaultValue.TabIndex = 1;
            this.button_fingerprint_setDefaultValue.Text = "设为常用值";
            this.button_fingerprint_setDefaultValue.UseVisualStyleBackColor = true;
            this.button_fingerprint_setDefaultValue.Click += new System.EventHandler(this.button_fingerprint_defaultValue_Click);
            // 
            // textBox_fingerprint_readerUrl
            // 
            this.textBox_fingerprint_readerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_fingerprint_readerUrl.Location = new System.Drawing.Point(11, 35);
            this.textBox_fingerprint_readerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_fingerprint_readerUrl.Name = "textBox_fingerprint_readerUrl";
            this.textBox_fingerprint_readerUrl.Size = new System.Drawing.Size(844, 31);
            this.textBox_fingerprint_readerUrl.TabIndex = 0;
            // 
            // button_fingerprint_clearLocalCacheFiles
            // 
            this.button_fingerprint_clearLocalCacheFiles.Location = new System.Drawing.Point(17, 497);
            this.button_fingerprint_clearLocalCacheFiles.Margin = new System.Windows.Forms.Padding(5);
            this.button_fingerprint_clearLocalCacheFiles.Name = "button_fingerprint_clearLocalCacheFiles";
            this.button_fingerprint_clearLocalCacheFiles.Size = new System.Drawing.Size(340, 40);
            this.button_fingerprint_clearLocalCacheFiles.TabIndex = 1;
            this.button_fingerprint_clearLocalCacheFiles.Text = "清除指纹本地缓存文件(&C)";
            this.button_fingerprint_clearLocalCacheFiles.UseVisualStyleBackColor = true;
            this.button_fingerprint_clearLocalCacheFiles.Click += new System.EventHandler(this.button_fingerprint_clearLocalCacheFiles_Click);
            // 
            // tabPage_labelPrint
            // 
            this.tabPage_labelPrint.Controls.Add(this.comboBox_labelPrint_accessNoSource);
            this.tabPage_labelPrint.Controls.Add(this.label28);
            this.tabPage_labelPrint.Location = new System.Drawing.Point(4, 85);
            this.tabPage_labelPrint.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_labelPrint.Name = "tabPage_labelPrint";
            this.tabPage_labelPrint.Size = new System.Drawing.Size(905, 459);
            this.tabPage_labelPrint.TabIndex = 18;
            this.tabPage_labelPrint.Text = "标签打印";
            this.tabPage_labelPrint.UseVisualStyleBackColor = true;
            // 
            // comboBox_labelPrint_accessNoSource
            // 
            this.comboBox_labelPrint_accessNoSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_labelPrint_accessNoSource.FormattingEnabled = true;
            this.comboBox_labelPrint_accessNoSource.Items.AddRange(new object[] {
            "从册记录",
            "从书目记录",
            "顺次从册记录、书目记录"});
            this.comboBox_labelPrint_accessNoSource.Location = new System.Drawing.Point(242, 17);
            this.comboBox_labelPrint_accessNoSource.Margin = new System.Windows.Forms.Padding(5);
            this.comboBox_labelPrint_accessNoSource.Name = "comboBox_labelPrint_accessNoSource";
            this.comboBox_labelPrint_accessNoSource.Size = new System.Drawing.Size(286, 29);
            this.comboBox_labelPrint_accessNoSource.TabIndex = 1;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(7, 23);
            this.label28.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(201, 21);
            this.label28.TabIndex = 0;
            this.label28.Text = "如何获得索取号(&A):";
            // 
            // tabPage_message
            // 
            this.tabPage_message.AutoScroll = true;
            this.tabPage_message.Controls.Add(this.label_message_denysharebiblio);
            this.tabPage_message.Controls.Add(this.groupBox_dp2mserver);
            this.tabPage_message.Controls.Add(this.groupBox_message_compactShelf);
            this.tabPage_message.Controls.Add(this.label_message_shareBiblio_comment);
            this.tabPage_message.Controls.Add(this.checkBox_message_shareBiblio);
            this.tabPage_message.Location = new System.Drawing.Point(4, 85);
            this.tabPage_message.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_message.Name = "tabPage_message";
            this.tabPage_message.Size = new System.Drawing.Size(905, 459);
            this.tabPage_message.TabIndex = 19;
            this.tabPage_message.Text = "消息";
            this.tabPage_message.UseVisualStyleBackColor = true;
            // 
            // groupBox_dp2mserver
            // 
            this.groupBox_dp2mserver.Controls.Add(this.textBox_message_userName);
            this.groupBox_dp2mserver.Controls.Add(this.label33);
            this.groupBox_dp2mserver.Controls.Add(this.button_message_setDefaultUrl);
            this.groupBox_dp2mserver.Controls.Add(this.label32);
            this.groupBox_dp2mserver.Controls.Add(this.textBox_message_dp2MServerUrl);
            this.groupBox_dp2mserver.Controls.Add(this.textBox_message_password);
            this.groupBox_dp2mserver.Controls.Add(this.label29);
            this.groupBox_dp2mserver.Location = new System.Drawing.Point(20, 162);
            this.groupBox_dp2mserver.Name = "groupBox_dp2mserver";
            this.groupBox_dp2mserver.Size = new System.Drawing.Size(835, 221);
            this.groupBox_dp2mserver.TabIndex = 27;
            this.groupBox_dp2mserver.TabStop = false;
            this.groupBox_dp2mserver.Text = "消息服务器";
            // 
            // textBox_message_userName
            // 
            this.textBox_message_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_message_userName.Location = new System.Drawing.Point(214, 118);
            this.textBox_message_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_message_userName.Name = "textBox_message_userName";
            this.textBox_message_userName.Size = new System.Drawing.Size(283, 31);
            this.textBox_message_userName.TabIndex = 20;
            // 
            // label33
            // 
            this.label33.Location = new System.Drawing.Point(19, 121);
            this.label33.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(137, 31);
            this.label33.TabIndex = 19;
            this.label33.Text = "用户名(&U)：";
            // 
            // button_message_setDefaultUrl
            // 
            this.button_message_setDefaultUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_message_setDefaultUrl.Location = new System.Drawing.Point(632, 24);
            this.button_message_setDefaultUrl.Margin = new System.Windows.Forms.Padding(5);
            this.button_message_setDefaultUrl.Name = "button_message_setDefaultUrl";
            this.button_message_setDefaultUrl.Size = new System.Drawing.Size(180, 40);
            this.button_message_setDefaultUrl.TabIndex = 23;
            this.button_message_setDefaultUrl.Text = "设为常用值";
            this.button_message_setDefaultUrl.UseVisualStyleBackColor = true;
            this.button_message_setDefaultUrl.Click += new System.EventHandler(this.button_message_setDefaultUrl_Click);
            // 
            // label32
            // 
            this.label32.Location = new System.Drawing.Point(19, 168);
            this.label32.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(137, 31);
            this.label32.TabIndex = 21;
            this.label32.Text = "密码(&P)：";
            // 
            // textBox_message_dp2MServerUrl
            // 
            this.textBox_message_dp2MServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_dp2MServerUrl.Location = new System.Drawing.Point(22, 71);
            this.textBox_message_dp2MServerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_message_dp2MServerUrl.Name = "textBox_message_dp2MServerUrl";
            this.textBox_message_dp2MServerUrl.Size = new System.Drawing.Size(790, 31);
            this.textBox_message_dp2MServerUrl.TabIndex = 18;
            // 
            // textBox_message_password
            // 
            this.textBox_message_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_message_password.Location = new System.Drawing.Point(214, 165);
            this.textBox_message_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_message_password.Name = "textBox_message_password";
            this.textBox_message_password.PasswordChar = '*';
            this.textBox_message_password.Size = new System.Drawing.Size(283, 31);
            this.textBox_message_password.TabIndex = 22;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(19, 43);
            this.label29.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(249, 21);
            this.label29.TabIndex = 17;
            this.label29.Text = "dp2MServer 服务器 URL:";
            // 
            // groupBox_message_compactShelf
            // 
            this.groupBox_message_compactShelf.Controls.Add(this.textBox_message_shelfAccount);
            this.groupBox_message_compactShelf.Controls.Add(this.label38);
            this.groupBox_message_compactShelf.Location = new System.Drawing.Point(20, 408);
            this.groupBox_message_compactShelf.Name = "groupBox_message_compactShelf";
            this.groupBox_message_compactShelf.Size = new System.Drawing.Size(552, 117);
            this.groupBox_message_compactShelf.TabIndex = 26;
            this.groupBox_message_compactShelf.TabStop = false;
            this.groupBox_message_compactShelf.Text = "密集书架";
            // 
            // textBox_message_shelfAccount
            // 
            this.textBox_message_shelfAccount.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_message_shelfAccount.Location = new System.Drawing.Point(192, 45);
            this.textBox_message_shelfAccount.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_message_shelfAccount.Name = "textBox_message_shelfAccount";
            this.textBox_message_shelfAccount.Size = new System.Drawing.Size(283, 31);
            this.textBox_message_shelfAccount.TabIndex = 25;
            // 
            // label38
            // 
            this.label38.Location = new System.Drawing.Point(8, 48);
            this.label38.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(174, 31);
            this.label38.TabIndex = 24;
            this.label38.Text = "书架账户(&A)：";
            // 
            // label_message_shareBiblio_comment
            // 
            this.label_message_shareBiblio_comment.Location = new System.Drawing.Point(10, 82);
            this.label_message_shareBiblio_comment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message_shareBiblio_comment.Name = "label_message_shareBiblio_comment";
            this.label_message_shareBiblio_comment.Size = new System.Drawing.Size(845, 77);
            this.label_message_shareBiblio_comment.TabIndex = 16;
            this.label_message_shareBiblio_comment.Text = "共享书目数据，将允许 Internet 上他人检索获取您的全部书目数据，同时也允许您检索获取他人的书目数据";
            // 
            // checkBox_message_shareBiblio
            // 
            this.checkBox_message_shareBiblio.AutoSize = true;
            this.checkBox_message_shareBiblio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_message_shareBiblio.Location = new System.Drawing.Point(20, 45);
            this.checkBox_message_shareBiblio.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_message_shareBiblio.Name = "checkBox_message_shareBiblio";
            this.checkBox_message_shareBiblio.Size = new System.Drawing.Size(193, 25);
            this.checkBox_message_shareBiblio.TabIndex = 15;
            this.checkBox_message_shareBiblio.Text = "共享书目数据(&S)";
            this.checkBox_message_shareBiblio.CheckedChanged += new System.EventHandler(this.checkBox_message_shareBiblio_CheckedChanged);
            // 
            // tabPage_z3950
            // 
            this.tabPage_z3950.Controls.Add(this.groupBox7);
            this.tabPage_z3950.Controls.Add(this.button_z3950_servers);
            this.tabPage_z3950.Location = new System.Drawing.Point(4, 85);
            this.tabPage_z3950.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_z3950.Name = "tabPage_z3950";
            this.tabPage_z3950.Size = new System.Drawing.Size(905, 459);
            this.tabPage_z3950.TabIndex = 20;
            this.tabPage_z3950.Text = "Z39.50";
            this.tabPage_z3950.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.button_ucs_testUpload);
            this.groupBox7.Controls.Add(this.textBox_ucs_databaseName);
            this.groupBox7.Controls.Add(this.label47);
            this.groupBox7.Controls.Add(this.textBox_ucs_password);
            this.groupBox7.Controls.Add(this.label46);
            this.groupBox7.Controls.Add(this.textBox_ucs_userName);
            this.groupBox7.Controls.Add(this.label45);
            this.groupBox7.Controls.Add(this.textBox_ucs_apiUrl);
            this.groupBox7.Controls.Add(this.label44);
            this.groupBox7.Location = new System.Drawing.Point(19, 99);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(635, 323);
            this.groupBox7.TabIndex = 1;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "UCS 上传接口";
            // 
            // button_ucs_testUpload
            // 
            this.button_ucs_testUpload.Location = new System.Drawing.Point(172, 234);
            this.button_ucs_testUpload.Name = "button_ucs_testUpload";
            this.button_ucs_testUpload.Size = new System.Drawing.Size(219, 35);
            this.button_ucs_testUpload.TabIndex = 8;
            this.button_ucs_testUpload.Text = "测试上传记录";
            this.button_ucs_testUpload.UseVisualStyleBackColor = true;
            this.button_ucs_testUpload.Click += new System.EventHandler(this.button_ucs_testUpload_Click);
            // 
            // textBox_ucs_databaseName
            // 
            this.textBox_ucs_databaseName.Location = new System.Drawing.Point(172, 79);
            this.textBox_ucs_databaseName.Name = "textBox_ucs_databaseName";
            this.textBox_ucs_databaseName.Size = new System.Drawing.Size(303, 31);
            this.textBox_ucs_databaseName.TabIndex = 7;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(17, 82);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(105, 21);
            this.label47.TabIndex = 6;
            this.label47.Text = "数据库名:";
            // 
            // textBox_ucs_password
            // 
            this.textBox_ucs_password.Location = new System.Drawing.Point(172, 167);
            this.textBox_ucs_password.Name = "textBox_ucs_password";
            this.textBox_ucs_password.PasswordChar = '*';
            this.textBox_ucs_password.Size = new System.Drawing.Size(303, 31);
            this.textBox_ucs_password.TabIndex = 5;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(17, 170);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(63, 21);
            this.label46.TabIndex = 4;
            this.label46.Text = "密码:";
            // 
            // textBox_ucs_userName
            // 
            this.textBox_ucs_userName.Location = new System.Drawing.Point(172, 130);
            this.textBox_ucs_userName.Name = "textBox_ucs_userName";
            this.textBox_ucs_userName.Size = new System.Drawing.Size(303, 31);
            this.textBox_ucs_userName.TabIndex = 3;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(17, 133);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(84, 21);
            this.label45.TabIndex = 2;
            this.label45.Text = "用户名:";
            // 
            // textBox_ucs_apiUrl
            // 
            this.textBox_ucs_apiUrl.Location = new System.Drawing.Point(172, 42);
            this.textBox_ucs_apiUrl.Name = "textBox_ucs_apiUrl";
            this.textBox_ucs_apiUrl.Size = new System.Drawing.Size(417, 31);
            this.textBox_ucs_apiUrl.TabIndex = 1;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(17, 45);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(98, 21);
            this.label44.TabIndex = 0;
            this.label44.Text = "API URL:";
            // 
            // button_z3950_servers
            // 
            this.button_z3950_servers.Location = new System.Drawing.Point(4, 28);
            this.button_z3950_servers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_z3950_servers.Name = "button_z3950_servers";
            this.button_z3950_servers.Size = new System.Drawing.Size(650, 43);
            this.button_z3950_servers.TabIndex = 0;
            this.button_z3950_servers.Text = "服务器列表 ...";
            this.button_z3950_servers.UseVisualStyleBackColor = true;
            this.button_z3950_servers.Click += new System.EventHandler(this.button_z3950_servers_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(643, 577);
            this.button_OK.Margin = new System.Windows.Forms.Padding(5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(137, 38);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(792, 577);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(137, 38);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label_message_denysharebiblio
            // 
            this.label_message_denysharebiblio.AutoSize = true;
            this.label_message_denysharebiblio.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message_denysharebiblio.Location = new System.Drawing.Point(222, 48);
            this.label_message_denysharebiblio.Name = "label_message_denysharebiblio";
            this.label_message_denysharebiblio.Size = new System.Drawing.Size(0, 21);
            this.label_message_denysharebiblio.TabIndex = 28;
            // 
            // CfgDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(948, 633);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "CfgDlg";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "参数配置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CfgDlg_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CfgDlg_FormClosed);
            this.Load += new System.EventHandler(this.CfgDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_server.ResumeLayout(false);
            this.tabPage_server.PerformLayout();
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.tabPage_defaultAccount.ResumeLayout(false);
            this.tabPage_defaultAccount.PerformLayout();
            this.tabPage_cacheManage.ResumeLayout(false);
            this.tabPage_charging.ResumeLayout(false);
            this.tabPage_charging.PerformLayout();
            this.groupBox_charging_selectItemDialog.ResumeLayout(false);
            this.groupBox_charging_selectItemDialog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_charging_infoDlgOpacity)).EndInit();
            this.tabPage_quickCharging.ResumeLayout(false);
            this.tabPage_quickCharging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds)).EndInit();
            this.groupBox_quickCharging_selectItemDialog.ResumeLayout(false);
            this.groupBox_quickCharging_selectItemDialog.PerformLayout();
            this.tabPage_itemManagement.ResumeLayout(false);
            this.tabPage_itemManagement.PerformLayout();
            this.tabPage_ui.ResumeLayout(false);
            this.tabPage_ui.PerformLayout();
            this.tabPage_passgate.ResumeLayout(false);
            this.tabPage_passgate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_passgate_maxListItemsCount)).EndInit();
            this.tabPage_search.ResumeLayout(false);
            this.tabPage_search.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxCommentResultCount)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxIssueResultCount)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxOrderResultCount)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxItemResultCount)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxReaderResultCount)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_multiline_maxBiblioResultCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_search_maxBiblioResultCount)).EndInit();
            this.tabPage_print.ResumeLayout(false);
            this.tabPage_print.PerformLayout();
            this.toolStrip_print.ResumeLayout(false);
            this.toolStrip_print.PerformLayout();
            this.tabPage_amerce.ResumeLayout(false);
            this.tabPage_amerce.PerformLayout();
            this.tabPage_accept.ResumeLayout(false);
            this.tabPage_accept.PerformLayout();
            this.tabPage_cardReader.ResumeLayout(false);
            this.groupBox_rfidTest.ResumeLayout(false);
            this.groupBox_rfidTest.PerformLayout();
            this.groupBox_rfidReader.ResumeLayout(false);
            this.groupBox_rfidReader.PerformLayout();
            this.groupBox_uhf.ResumeLayout(false);
            this.groupBox_uhf.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_rfid_inventoryIdleSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_uhf_rssi)).EndInit();
            this.groupBox_idcardReader.ResumeLayout(false);
            this.groupBox_idcardReader.PerformLayout();
            this.tabPage_patron.ResumeLayout(false);
            this.tabPage_patron.PerformLayout();
            this.tabPage_operLog.ResumeLayout(false);
            this.tabPage_operLog.PerformLayout();
            this.tabPage_global.ResumeLayout(false);
            this.tabPage_global.PerformLayout();
            this.tabPage_fingerprint.ResumeLayout(false);
            this.groupBox_palmprintUrl.ResumeLayout(false);
            this.groupBox_palmprintUrl.PerformLayout();
            this.groupBox_face.ResumeLayout(false);
            this.groupBox_face.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox_fingerprint.ResumeLayout(false);
            this.groupBox_fingerprint.PerformLayout();
            this.tabPage_labelPrint.ResumeLayout(false);
            this.tabPage_labelPrint.PerformLayout();
            this.tabPage_message.ResumeLayout(false);
            this.tabPage_message.PerformLayout();
            this.groupBox_dp2mserver.ResumeLayout(false);
            this.groupBox_dp2mserver.PerformLayout();
            this.groupBox_message_compactShelf.ResumeLayout(false);
            this.groupBox_message_compactShelf.PerformLayout();
            this.tabPage_z3950.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_server;
        private System.Windows.Forms.TabPage tabPage_defaultAccount;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_server_dp2LibraryServerUrl;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox checkBox_defaulAccount_savePasswordShort;
        public System.Windows.Forms.TextBox textBox_defaultAccount_password;
        public System.Windows.Forms.TextBox textBox_defaultAccount_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox_defaultAccount_location;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_defaultAccount_isReader;
        private System.Windows.Forms.TabPage tabPage_cacheManage;
        private System.Windows.Forms.Button button_clearValueTableCache;
        private System.Windows.Forms.Button button_reloadBiblioDbFromInfos;
        private System.Windows.Forms.TabPage tabPage_charging;
        private System.Windows.Forms.CheckBox checkBox_charging_force;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown_charging_infoDlgOpacity;
        private System.Windows.Forms.CheckBox checkBox_charging_verifyBarcode;
        private System.Windows.Forms.TabPage tabPage_ui;
        private System.Windows.Forms.ComboBox comboBox_ui_fixedPanelDock;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox_ui_hideFixedPanel;
        private System.Windows.Forms.TabPage tabPage_itemManagement;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_verifyItemBarcode;
        private System.Windows.Forms.CheckBox checkBox_charging_doubleItemInputAsEnd;
        private System.Windows.Forms.CheckBox checkBox_defaultAccount_occurPerStart;
        public System.Windows.Forms.CheckBox checkBox_defaulAccount_savePasswordLong;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_charging_displayFormat;
        private System.Windows.Forms.CheckBox checkBox_charging_autoUppercaseBarcode;
        private System.Windows.Forms.CheckBox checkBox_charging_greenInfoDlgNotOccur;
        private System.Windows.Forms.CheckBox checkBox_charging_noBiblioAndItem;
        private System.Windows.Forms.TabPage tabPage_passgate;
        private System.Windows.Forms.NumericUpDown numericUpDown_passgate_maxListItemsCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_cataloging;
        private System.Windows.Forms.Button button_reloadBiblioDbProperties;
        private System.Windows.Forms.Button button_reloadReaderDbNames;
        private System.Windows.Forms.Button button_reloadUtilDbProperties;
        private System.Windows.Forms.Button button_downloadPinyinXmlFile;
        private System.Windows.Forms.Button buttondownloadIsbnXmlFile;
        private System.Windows.Forms.TabPage tabPage_search;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_maxBiblioResultCount;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_maxReaderResultCount;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_maxItemResultCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolStrip toolStrip_print;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_managePrintScript;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_print_editCharingPrintCs;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_print_editCharingPrintCsRef;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBox_print_prnPort;
        private System.Windows.Forms.CheckBox checkBox_print_pausePrint;
        private System.Windows.Forms.Button button_print_projectManage;
        private System.Windows.Forms.TextBox textBox_print_projectName;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label_print_projectNameMessage;
        private System.Windows.Forms.Button button_print_findProject;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_search_hideBiblioMatchStyle;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_charging_autoSwitchReaderBarcode;
        private System.Windows.Forms.CheckBox checkBox_search_biblioPushFilling;
        private System.Windows.Forms.CheckBox checkBox_search_readerPushFilling;
        private System.Windows.Forms.CheckBox checkBox_search_itemPushFilling;
        private System.Windows.Forms.CheckBox checkBox_charging_autoClearTextbox;
        private System.Windows.Forms.CheckBox checkBox_search_hideItemMatchStyleAndDbName;
        private System.Windows.Forms.CheckBox checkBox_search_useExistDetailWindow;
        private System.Windows.Forms.CheckBox checkBox_search_hideReaderMatchStyle;
        private System.Windows.Forms.TabPage tabPage_amerce;
        private System.Windows.Forms.ComboBox comboBox_amerce_interface;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TabPage tabPage_accept;
        private System.Windows.Forms.CheckBox checkBox_accept_singleClickLoadDetail;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_searchDupWhenSaving;
        private System.Windows.Forms.TextBox textBox_ui_defaultFont;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button button_ui_getDefaultFont;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_verifyDataWhenSaving;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_showItemQuickInputPanel;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_showQueryPanel;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox checkBox_search_hideCommentMatchStyleAndDbName;
        private System.Windows.Forms.CheckBox checkBox_search_commentPushFilling;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_maxCommentResultCount;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox checkBox_search_hideIssueMatchStyleAndDbName;
        private System.Windows.Forms.CheckBox checkBox_search_issuePushFilling;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_maxIssueResultCount;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBox_search_hideOrderMatchStyleAndDbName;
        private System.Windows.Forms.CheckBox checkBox_search_orderPushFilling;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_maxOrderResultCount;
        private System.Windows.Forms.TextBox textBox_server_authorNumber_gcatUrl;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBox_server_pinyin_gcatUrl;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TabPage tabPage_cardReader;
        private System.Windows.Forms.TextBox textBox_cardReader_idcardReaderUrl;
        private System.Windows.Forms.TabPage tabPage_patron;
        private System.Windows.Forms.CheckBox checkBox_patron_displaySetReaderBarcodeDialog;
        private System.Windows.Forms.CheckBox checkBox_patron_verifyBarcode;
        private System.Windows.Forms.CheckBox checkBox_patron_autoRetryReaderCard;
        private System.Windows.Forms.CheckBox checkBox_patron_disableIdcardReaderKeyboardSimulation;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_linkedRecordReadonly;
        private System.Windows.Forms.CheckBox checkBox_charging_veifyReaderPassword;
        private System.Windows.Forms.CheckBox checkBox_charging_stopFillingWhenCloseInfoDlg;
        private System.Windows.Forms.TabPage tabPage_global;
        private System.Windows.Forms.CheckBox checkBox_global_displayScriptErrorDialog;
        private System.Windows.Forms.TabPage tabPage_operLog;
        private System.Windows.Forms.CheckBox checkBox_operLog_displayReaderBorrowHistory;
        private System.Windows.Forms.CheckBox checkBox_operLog_displayItemBorrowHistory;
        private System.Windows.Forms.Button button_operLog_clearCacheDirectory;
        private System.Windows.Forms.CheckBox checkBox_operLog_autoCache;
        private System.Windows.Forms.ComboBox comboBox_amerce_layout;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.CheckBox checkBox_itemManagement_displayOtherLibraryItem;
        private System.Windows.Forms.TextBox textBox_itemManagement_maxPicWidth;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TabPage tabPage_fingerprint;
        private System.Windows.Forms.TextBox textBox_fingerprint_readerUrl;
        private System.Windows.Forms.Button button_fingerprint_setDefaultValue;
        private System.Windows.Forms.Button button_fingerprint_clearLocalCacheFiles;
        private System.Windows.Forms.GroupBox groupBox_fingerprint;
        private System.Windows.Forms.GroupBox groupBox_idcardReader;
        private System.Windows.Forms.Button button_cardReader_setIdcardUrlDefaultValue;
        private System.Windows.Forms.GroupBox groupBox9;
        public System.Windows.Forms.TextBox textBox_fingerprint_password;
        public System.Windows.Forms.TextBox textBox_fingerprint_userName;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.ComboBox comboBox_operLog_level;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.CheckBox checkBox_global_autoSelPinyin;
        private System.Windows.Forms.CheckBox checkBox_charging_speakNameWhenLoadReaderRecord;
        private System.Windows.Forms.CheckBox checkBox_charging_patronBarcodeAllowHanzi;
        private System.Windows.Forms.CheckBox checkBox_charging_noBorrowHistory;
        private System.Windows.Forms.TabPage tabPage_quickCharging;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_verifyBarcode;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_noBorrowHistory;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_speakNameWhenLoadReaderRecord;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_speakBookTitle;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TabPage tabPage_labelPrint;
        private System.Windows.Forms.ComboBox comboBox_labelPrint_accessNoSource;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.ToolStrip toolStrip_server;
        private System.Windows.Forms.ToolStripButton toolStripButton_server_setXeServer;
        private System.Windows.Forms.ToolStripButton toolStripButton_server_setHongnibaServer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox groupBox_quickCharging_selectItemDialog;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_autoOperItemDialogSingleItem;
        private System.Windows.Forms.GroupBox groupBox_charging_selectItemDialog;
        private System.Windows.Forms.CheckBox checkBox_charging_autoOperItemDialogSingleItem;
        private System.Windows.Forms.CheckBox checkBox_charging_isbnBorrow;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_isbnBorrow;
        private System.Windows.Forms.TabPage tabPage_message;
        private System.Windows.Forms.Label label_message_shareBiblio_comment;
        public System.Windows.Forms.CheckBox checkBox_message_shareBiblio;
        private System.Windows.Forms.TextBox textBox_message_dp2MServerUrl;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_logOperTime;
        private System.Windows.Forms.ComboBox comboBox_quickCharging_displayStyle;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox textBox_server_greenPackage;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.CheckBox checkBox_ui_fixedPanelAnimationEnabled;
        public System.Windows.Forms.TextBox textBox_message_password;
        public System.Windows.Forms.TextBox textBox_message_userName;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.ComboBox comboBox_quickCharging_stateSpeak;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.ComboBox comboBox_quickCharging_displayFormat;
        private System.Windows.Forms.Button button_message_setDefaultUrl;
        private System.Windows.Forms.CheckBox checkBox_global_saveOriginCoverImage;
        private System.Windows.Forms.CheckBox checkBox_global_upperInputBarcode;
        private System.Windows.Forms.Label label_forceVerifyDataComment;
        private System.Windows.Forms.CheckBox checkBox_ui_printLabelMode;
        private System.Windows.Forms.GroupBox groupBox_face;
        private System.Windows.Forms.Button button_face_setDefaultValue;
        private System.Windows.Forms.TextBox textBox_face_readerUrl;
        private System.Windows.Forms.GroupBox groupBox_rfidReader;
        private System.Windows.Forms.Button button_cardReader_setRfidUrlDefaultValue;
        private System.Windows.Forms.TextBox textBox_cardReader_rfidCenterUrl;
        private System.Windows.Forms.TabPage tabPage_z3950;
        private System.Windows.Forms.Button button_z3950_servers;
        private System.Windows.Forms.TextBox textBox_global_additionalLocations;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Button button_server_fillPinyinUrl;
        private System.Windows.Forms.Button button_server_fillAuthorNumberUrl;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.NumericUpDown numericUpDown_search_multiline_maxBiblioResultCount;
        private System.Windows.Forms.GroupBox groupBox_palmprintUrl;
        private System.Windows.Forms.Button button_palmprint_setDefaulValue;
        private System.Windows.Forms.TextBox textBox_palmprint_readerUrl;
        private System.Windows.Forms.CheckBox checkBox_search_itemFilterLibraryCode;
        private System.Windows.Forms.Label label37;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_global_securityProtocol;
        private System.Windows.Forms.CheckBox checkBox_global_disableSpeak;
        private System.Windows.Forms.LinkLabel linkLabel_installFaceCenter;
        private System.Windows.Forms.LinkLabel linkLabel_installFingerprintCenter;
        private System.Windows.Forms.CheckBox checkBox_patron_disableBioKeyboardSimulation;
        private System.Windows.Forms.Button button_fingerprint_setDefaultValue_new;
        private System.Windows.Forms.CheckBox checkBox_face_savePhotoWhileRegister;
        private System.Windows.Forms.GroupBox groupBox_message_compactShelf;
        public System.Windows.Forms.TextBox textBox_message_shelfAccount;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.GroupBox groupBox_uhf;
        private System.Windows.Forms.CheckBox checkBox_uhf_bookTagWriteUserBank;
        private System.Windows.Forms.CheckBox checkBox_uhf_warningWhenDataFormatMismatch;
        private System.Windows.Forms.ComboBox comboBox_uhf_dataFormat;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Label label_uhf_elements;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_uhf_elements;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.NumericUpDown numericUpDown_uhf_rssi;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.GroupBox groupBox_rfidTest;
        private System.Windows.Forms.CheckBox checkBox_rfidTest_returnPreEAS;
        private System.Windows.Forms.CheckBox checkBox_rfidTest_borrowEAS;
        private System.Windows.Forms.CheckBox checkBox_rfidTest_returnAPI;
        private System.Windows.Forms.CheckBox checkBox_rfidTest_returnPostUndoEAS;
        private System.Windows.Forms.CheckBox checkBox_uhf_onlyEpcCharging;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.NumericUpDown numericUpDown_rfid_inventoryIdleSeconds;
        private System.Windows.Forms.GroupBox groupBox_dp2mserver;
        private System.Windows.Forms.ComboBox comboBox_rfid_tagCachePolicy;
        private System.Windows.Forms.Label label_rfid_tagCachePolicy;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_allowFreeSequence;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.NumericUpDown numericUpDown_quickCharging_autoTriggerFaceInputDelaySeconds;
        private System.Windows.Forms.CheckBox checkBox_quickCharging_faceInputMultipleHits;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.TextBox textBox_ucs_apiUrl;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.TextBox textBox_ucs_password;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.TextBox textBox_ucs_userName;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.TextBox textBox_ucs_databaseName;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Button button_ucs_testUpload;
        private System.Windows.Forms.TextBox textBox_ui_loginWelcomeText;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label_message_denysharebiblio;
    }
}