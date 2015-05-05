namespace dp2Catalog
{
    partial class SystemCfgForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemCfgForm));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_dtlp = new System.Windows.Forms.TabPage();
            this.button_dtlp_dupCfg = new System.Windows.Forms.Button();
            this.checkBox_dtlpSavePassword = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_dtlpDefaultPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_dtlpDefaultUserName = new System.Windows.Forms.TextBox();
            this.button_dtlp_hostManage = new System.Windows.Forms.Button();
            this.tabPage_dp2library = new System.Windows.Forms.TabPage();
            this.button_dp2library_searchDup_findDefaultStartPath = new System.Windows.Forms.Button();
            this.textBox_dp2library_searchDup_defaultStartPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_dp2library_serverManage = new System.Windows.Forms.Button();
            this.tabPage_dp2SearchForm = new System.Windows.Forms.TabPage();
            this.checkBox_dp2SearchForm_useExistDetailWindow = new System.Windows.Forms.CheckBox();
            this.numericUpDown_dp2library_searchMaxCount = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_dp2SearchForm_layout = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage_amazonSearchForm = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.tabComboBox_amazon_defaultServer = new DigitalPlatform.CommonControl.TabComboBox();
            this.checkBox_amazon_alwaysUseFullElementSet = new System.Windows.Forms.CheckBox();
            this.tabPage_ui = new System.Windows.Forms.TabPage();
            this.button_ui_getDefaultFont = new System.Windows.Forms.Button();
            this.textBox_ui_defaultFont = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.checkBox_ui_hideFixedPanel = new System.Windows.Forms.CheckBox();
            this.comboBox_ui_fixedPanelDock = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage_marcDetailForm = new System.Windows.Forms.TabPage();
            this.checkBox_marcDetailForm_verifyDataWhenSaving = new System.Windows.Forms.CheckBox();
            this.tabPage_server = new System.Windows.Forms.TabPage();
            this.textBox_server_pinyin_gcatUrl = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox_server_authorNumber_gcatUrl = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.tabPage_global = new System.Windows.Forms.TabPage();
            this.checkBox_global_autoSelPinyin = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_dtlp.SuspendLayout();
            this.tabPage_dp2library.SuspendLayout();
            this.tabPage_dp2SearchForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_dp2library_searchMaxCount)).BeginInit();
            this.tabPage_amazonSearchForm.SuspendLayout();
            this.tabPage_ui.SuspendLayout();
            this.tabPage_marcDetailForm.SuspendLayout();
            this.tabPage_server.SuspendLayout();
            this.tabPage_global.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(332, 268);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(272, 268);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_dtlp);
            this.tabControl_main.Controls.Add(this.tabPage_dp2library);
            this.tabControl_main.Controls.Add(this.tabPage_dp2SearchForm);
            this.tabControl_main.Controls.Add(this.tabPage_amazonSearchForm);
            this.tabControl_main.Controls.Add(this.tabPage_ui);
            this.tabControl_main.Controls.Add(this.tabPage_marcDetailForm);
            this.tabControl_main.Controls.Add(this.tabPage_server);
            this.tabControl_main.Controls.Add(this.tabPage_global);
            this.tabControl_main.Location = new System.Drawing.Point(6, 6);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(382, 258);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_dtlp
            // 
            this.tabPage_dtlp.Controls.Add(this.button_dtlp_dupCfg);
            this.tabPage_dtlp.Controls.Add(this.checkBox_dtlpSavePassword);
            this.tabPage_dtlp.Controls.Add(this.label1);
            this.tabPage_dtlp.Controls.Add(this.textBox_dtlpDefaultPassword);
            this.tabPage_dtlp.Controls.Add(this.label2);
            this.tabPage_dtlp.Controls.Add(this.textBox_dtlpDefaultUserName);
            this.tabPage_dtlp.Controls.Add(this.button_dtlp_hostManage);
            this.tabPage_dtlp.Location = new System.Drawing.Point(4, 22);
            this.tabPage_dtlp.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_dtlp.Name = "tabPage_dtlp";
            this.tabPage_dtlp.Size = new System.Drawing.Size(374, 232);
            this.tabPage_dtlp.TabIndex = 0;
            this.tabPage_dtlp.Text = "DTLP协议";
            this.tabPage_dtlp.UseVisualStyleBackColor = true;
            // 
            // button_dtlp_dupCfg
            // 
            this.button_dtlp_dupCfg.Location = new System.Drawing.Point(4, 40);
            this.button_dtlp_dupCfg.Margin = new System.Windows.Forms.Padding(2);
            this.button_dtlp_dupCfg.Name = "button_dtlp_dupCfg";
            this.button_dtlp_dupCfg.Size = new System.Drawing.Size(220, 22);
            this.button_dtlp_dupCfg.TabIndex = 1;
            this.button_dtlp_dupCfg.Text = "查重方案配置(&D) ...";
            this.button_dtlp_dupCfg.Click += new System.EventHandler(this.button_dtlp_dupCfg_Click);
            // 
            // checkBox_dtlpSavePassword
            // 
            this.checkBox_dtlpSavePassword.Location = new System.Drawing.Point(68, 143);
            this.checkBox_dtlpSavePassword.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_dtlpSavePassword.Name = "checkBox_dtlpSavePassword";
            this.checkBox_dtlpSavePassword.Size = new System.Drawing.Size(126, 19);
            this.checkBox_dtlpSavePassword.TabIndex = 6;
            this.checkBox_dtlpSavePassword.Text = "记住密码(&R)";
            this.checkBox_dtlpSavePassword.CheckedChanged += new System.EventHandler(this.checkBox_dtlpSavePassword_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(2, 92);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "用户名:";
            // 
            // textBox_dtlpDefaultPassword
            // 
            this.textBox_dtlpDefaultPassword.Location = new System.Drawing.Point(68, 118);
            this.textBox_dtlpDefaultPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dtlpDefaultPassword.Name = "textBox_dtlpDefaultPassword";
            this.textBox_dtlpDefaultPassword.PasswordChar = '*';
            this.textBox_dtlpDefaultPassword.Size = new System.Drawing.Size(127, 21);
            this.textBox_dtlpDefaultPassword.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(2, 124);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "密码:";
            // 
            // textBox_dtlpDefaultUserName
            // 
            this.textBox_dtlpDefaultUserName.Location = new System.Drawing.Point(68, 86);
            this.textBox_dtlpDefaultUserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dtlpDefaultUserName.Name = "textBox_dtlpDefaultUserName";
            this.textBox_dtlpDefaultUserName.Size = new System.Drawing.Size(127, 21);
            this.textBox_dtlpDefaultUserName.TabIndex = 3;
            // 
            // button_dtlp_hostManage
            // 
            this.button_dtlp_hostManage.Location = new System.Drawing.Point(4, 13);
            this.button_dtlp_hostManage.Margin = new System.Windows.Forms.Padding(2);
            this.button_dtlp_hostManage.Name = "button_dtlp_hostManage";
            this.button_dtlp_hostManage.Size = new System.Drawing.Size(220, 22);
            this.button_dtlp_hostManage.TabIndex = 0;
            this.button_dtlp_hostManage.Text = "服务器地址管理(&H) ...";
            this.button_dtlp_hostManage.Click += new System.EventHandler(this.button_hostManage_Click);
            // 
            // tabPage_dp2library
            // 
            this.tabPage_dp2library.Controls.Add(this.button_dp2library_searchDup_findDefaultStartPath);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_searchDup_defaultStartPath);
            this.tabPage_dp2library.Controls.Add(this.label4);
            this.tabPage_dp2library.Controls.Add(this.button_dp2library_serverManage);
            this.tabPage_dp2library.Location = new System.Drawing.Point(4, 22);
            this.tabPage_dp2library.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_dp2library.Name = "tabPage_dp2library";
            this.tabPage_dp2library.Size = new System.Drawing.Size(374, 232);
            this.tabPage_dp2library.TabIndex = 1;
            this.tabPage_dp2library.Text = "dp2library协议";
            this.tabPage_dp2library.UseVisualStyleBackColor = true;
            // 
            // button_dp2library_searchDup_findDefaultStartPath
            // 
            this.button_dp2library_searchDup_findDefaultStartPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_dp2library_searchDup_findDefaultStartPath.Location = new System.Drawing.Point(340, 70);
            this.button_dp2library_searchDup_findDefaultStartPath.Margin = new System.Windows.Forms.Padding(2);
            this.button_dp2library_searchDup_findDefaultStartPath.Name = "button_dp2library_searchDup_findDefaultStartPath";
            this.button_dp2library_searchDup_findDefaultStartPath.Size = new System.Drawing.Size(30, 21);
            this.button_dp2library_searchDup_findDefaultStartPath.TabIndex = 5;
            this.button_dp2library_searchDup_findDefaultStartPath.Text = "...";
            this.button_dp2library_searchDup_findDefaultStartPath.UseVisualStyleBackColor = true;
            this.button_dp2library_searchDup_findDefaultStartPath.Click += new System.EventHandler(this.button_dp2library_searchDup_findDefaultStartPath_Click);
            // 
            // textBox_dp2library_searchDup_defaultStartPath
            // 
            this.textBox_dp2library_searchDup_defaultStartPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2library_searchDup_defaultStartPath.Location = new System.Drawing.Point(4, 71);
            this.textBox_dp2library_searchDup_defaultStartPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dp2library_searchDup_defaultStartPath.Name = "textBox_dp2library_searchDup_defaultStartPath";
            this.textBox_dp2library_searchDup_defaultStartPath.Size = new System.Drawing.Size(332, 21);
            this.textBox_dp2library_searchDup_defaultStartPath.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 56);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "备选的查重起点路径(&S):";
            // 
            // button_dp2library_serverManage
            // 
            this.button_dp2library_serverManage.Location = new System.Drawing.Point(4, 13);
            this.button_dp2library_serverManage.Margin = new System.Windows.Forms.Padding(2);
            this.button_dp2library_serverManage.Name = "button_dp2library_serverManage";
            this.button_dp2library_serverManage.Size = new System.Drawing.Size(206, 22);
            this.button_dp2library_serverManage.TabIndex = 0;
            this.button_dp2library_serverManage.Text = "服务器和缺省帐户管理...";
            this.button_dp2library_serverManage.UseVisualStyleBackColor = true;
            this.button_dp2library_serverManage.Click += new System.EventHandler(this.button_dp2library_serverManage_Click);
            // 
            // tabPage_dp2SearchForm
            // 
            this.tabPage_dp2SearchForm.Controls.Add(this.checkBox_dp2SearchForm_useExistDetailWindow);
            this.tabPage_dp2SearchForm.Controls.Add(this.numericUpDown_dp2library_searchMaxCount);
            this.tabPage_dp2SearchForm.Controls.Add(this.label3);
            this.tabPage_dp2SearchForm.Controls.Add(this.comboBox_dp2SearchForm_layout);
            this.tabPage_dp2SearchForm.Controls.Add(this.label5);
            this.tabPage_dp2SearchForm.Location = new System.Drawing.Point(4, 22);
            this.tabPage_dp2SearchForm.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_dp2SearchForm.Name = "tabPage_dp2SearchForm";
            this.tabPage_dp2SearchForm.Size = new System.Drawing.Size(374, 232);
            this.tabPage_dp2SearchForm.TabIndex = 2;
            this.tabPage_dp2SearchForm.Text = "dp2检索窗";
            this.tabPage_dp2SearchForm.UseVisualStyleBackColor = true;
            // 
            // checkBox_dp2SearchForm_useExistDetailWindow
            // 
            this.checkBox_dp2SearchForm_useExistDetailWindow.AutoSize = true;
            this.checkBox_dp2SearchForm_useExistDetailWindow.Location = new System.Drawing.Point(4, 100);
            this.checkBox_dp2SearchForm_useExistDetailWindow.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_dp2SearchForm_useExistDetailWindow.Name = "checkBox_dp2SearchForm_useExistDetailWindow";
            this.checkBox_dp2SearchForm_useExistDetailWindow.Size = new System.Drawing.Size(270, 16);
            this.checkBox_dp2SearchForm_useExistDetailWindow.TabIndex = 4;
            this.checkBox_dp2SearchForm_useExistDetailWindow.Text = "在浏览框中双击时优先装入已打开的记录窗(&E)";
            this.checkBox_dp2SearchForm_useExistDetailWindow.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_dp2library_searchMaxCount
            // 
            this.numericUpDown_dp2library_searchMaxCount.Location = new System.Drawing.Point(147, 55);
            this.numericUpDown_dp2library_searchMaxCount.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_dp2library_searchMaxCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_dp2library_searchMaxCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_dp2library_searchMaxCount.Name = "numericUpDown_dp2library_searchMaxCount";
            this.numericUpDown_dp2library_searchMaxCount.Size = new System.Drawing.Size(90, 21);
            this.numericUpDown_dp2library_searchMaxCount.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 56);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "命中记录最大条数(&R):";
            // 
            // comboBox_dp2SearchForm_layout
            // 
            this.comboBox_dp2SearchForm_layout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_dp2SearchForm_layout.FormattingEnabled = true;
            this.comboBox_dp2SearchForm_layout.Items.AddRange(new object[] {
            "资源树最大",
            "浏览框最大(横向)",
            "浏览框最大(竖向)"});
            this.comboBox_dp2SearchForm_layout.Location = new System.Drawing.Point(147, 12);
            this.comboBox_dp2SearchForm_layout.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_dp2SearchForm_layout.Name = "comboBox_dp2SearchForm_layout";
            this.comboBox_dp2SearchForm_layout.Size = new System.Drawing.Size(145, 20);
            this.comboBox_dp2SearchForm_layout.TabIndex = 1;
            this.comboBox_dp2SearchForm_layout.SizeChanged += new System.EventHandler(this.comboBox_dp2SearchForm_layout_SizeChanged);
            this.comboBox_dp2SearchForm_layout.TextChanged += new System.EventHandler(this.comboBox_dp2SearchForm_layout_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 15);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "布局方式(&L);";
            // 
            // tabPage_amazonSearchForm
            // 
            this.tabPage_amazonSearchForm.Controls.Add(this.label7);
            this.tabPage_amazonSearchForm.Controls.Add(this.tabComboBox_amazon_defaultServer);
            this.tabPage_amazonSearchForm.Controls.Add(this.checkBox_amazon_alwaysUseFullElementSet);
            this.tabPage_amazonSearchForm.Location = new System.Drawing.Point(4, 22);
            this.tabPage_amazonSearchForm.Name = "tabPage_amazonSearchForm";
            this.tabPage_amazonSearchForm.Size = new System.Drawing.Size(374, 232);
            this.tabPage_amazonSearchForm.TabIndex = 6;
            this.tabPage_amazonSearchForm.Text = "亚马逊检索窗";
            this.tabPage_amazonSearchForm.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "首选服务器(&S)";
            // 
            // tabComboBox_amazon_defaultServer
            // 
            this.tabComboBox_amazon_defaultServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabComboBox_amazon_defaultServer.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_amazon_defaultServer.FormattingEnabled = true;
            this.tabComboBox_amazon_defaultServer.Location = new System.Drawing.Point(105, 20);
            this.tabComboBox_amazon_defaultServer.Name = "tabComboBox_amazon_defaultServer";
            this.tabComboBox_amazon_defaultServer.Size = new System.Drawing.Size(256, 22);
            this.tabComboBox_amazon_defaultServer.TabIndex = 1;
            this.tabComboBox_amazon_defaultServer.SelectedIndexChanged += new System.EventHandler(this.tabComboBox_amazon_defaultServer_SelectedIndexChanged);
            // 
            // checkBox_amazon_alwaysUseFullElementSet
            // 
            this.checkBox_amazon_alwaysUseFullElementSet.AutoSize = true;
            this.checkBox_amazon_alwaysUseFullElementSet.Location = new System.Drawing.Point(7, 63);
            this.checkBox_amazon_alwaysUseFullElementSet.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_amazon_alwaysUseFullElementSet.Name = "checkBox_amazon_alwaysUseFullElementSet";
            this.checkBox_amazon_alwaysUseFullElementSet.Size = new System.Drawing.Size(222, 16);
            this.checkBox_amazon_alwaysUseFullElementSet.TabIndex = 2;
            this.checkBox_amazon_alwaysUseFullElementSet.Text = "在获取浏览记录阶段即获得全记录(&F)";
            this.checkBox_amazon_alwaysUseFullElementSet.UseVisualStyleBackColor = true;
            // 
            // tabPage_ui
            // 
            this.tabPage_ui.Controls.Add(this.button_ui_getDefaultFont);
            this.tabPage_ui.Controls.Add(this.textBox_ui_defaultFont);
            this.tabPage_ui.Controls.Add(this.label16);
            this.tabPage_ui.Controls.Add(this.checkBox_ui_hideFixedPanel);
            this.tabPage_ui.Controls.Add(this.comboBox_ui_fixedPanelDock);
            this.tabPage_ui.Controls.Add(this.label6);
            this.tabPage_ui.Location = new System.Drawing.Point(4, 22);
            this.tabPage_ui.Name = "tabPage_ui";
            this.tabPage_ui.Size = new System.Drawing.Size(374, 232);
            this.tabPage_ui.TabIndex = 3;
            this.tabPage_ui.Text = "外观";
            this.tabPage_ui.UseVisualStyleBackColor = true;
            // 
            // button_ui_getDefaultFont
            // 
            this.button_ui_getDefaultFont.Location = new System.Drawing.Point(316, 71);
            this.button_ui_getDefaultFont.Name = "button_ui_getDefaultFont";
            this.button_ui_getDefaultFont.Size = new System.Drawing.Size(48, 23);
            this.button_ui_getDefaultFont.TabIndex = 8;
            this.button_ui_getDefaultFont.Text = "...";
            this.button_ui_getDefaultFont.UseVisualStyleBackColor = true;
            this.button_ui_getDefaultFont.Click += new System.EventHandler(this.button_ui_getDefaultFont_Click);
            // 
            // textBox_ui_defaultFont
            // 
            this.textBox_ui_defaultFont.Location = new System.Drawing.Point(149, 73);
            this.textBox_ui_defaultFont.Name = "textBox_ui_defaultFont";
            this.textBox_ui_defaultFont.Size = new System.Drawing.Size(161, 21);
            this.textBox_ui_defaultFont.TabIndex = 7;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 76);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 12);
            this.label16.TabIndex = 6;
            this.label16.Text = "缺省字体(&D):";
            // 
            // checkBox_ui_hideFixedPanel
            // 
            this.checkBox_ui_hideFixedPanel.AutoSize = true;
            this.checkBox_ui_hideFixedPanel.Location = new System.Drawing.Point(14, 44);
            this.checkBox_ui_hideFixedPanel.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_ui_hideFixedPanel.Name = "checkBox_ui_hideFixedPanel";
            this.checkBox_ui_hideFixedPanel.Size = new System.Drawing.Size(114, 16);
            this.checkBox_ui_hideFixedPanel.TabIndex = 5;
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
            this.comboBox_ui_fixedPanelDock.Location = new System.Drawing.Point(149, 13);
            this.comboBox_ui_fixedPanelDock.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_ui_fixedPanelDock.Name = "comboBox_ui_fixedPanelDock";
            this.comboBox_ui_fixedPanelDock.Size = new System.Drawing.Size(75, 20);
            this.comboBox_ui_fixedPanelDock.TabIndex = 4;
            this.comboBox_ui_fixedPanelDock.SelectedIndexChanged += new System.EventHandler(this.comboBox_ui_fixedPanelDock_SelectedIndexChanged);
            this.comboBox_ui_fixedPanelDock.SizeChanged += new System.EventHandler(this.comboBox_ui_fixedPanelDock_SizeChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 16);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "固定面板停靠方向(&F):";
            // 
            // tabPage_marcDetailForm
            // 
            this.tabPage_marcDetailForm.Controls.Add(this.checkBox_marcDetailForm_verifyDataWhenSaving);
            this.tabPage_marcDetailForm.Location = new System.Drawing.Point(4, 22);
            this.tabPage_marcDetailForm.Name = "tabPage_marcDetailForm";
            this.tabPage_marcDetailForm.Size = new System.Drawing.Size(374, 232);
            this.tabPage_marcDetailForm.TabIndex = 4;
            this.tabPage_marcDetailForm.Text = "MARC记录窗";
            this.tabPage_marcDetailForm.UseVisualStyleBackColor = true;
            // 
            // checkBox_marcDetailForm_verifyDataWhenSaving
            // 
            this.checkBox_marcDetailForm_verifyDataWhenSaving.AutoSize = true;
            this.checkBox_marcDetailForm_verifyDataWhenSaving.Location = new System.Drawing.Point(12, 19);
            this.checkBox_marcDetailForm_verifyDataWhenSaving.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_marcDetailForm_verifyDataWhenSaving.Name = "checkBox_marcDetailForm_verifyDataWhenSaving";
            this.checkBox_marcDetailForm_verifyDataWhenSaving.Size = new System.Drawing.Size(198, 16);
            this.checkBox_marcDetailForm_verifyDataWhenSaving.TabIndex = 3;
            this.checkBox_marcDetailForm_verifyDataWhenSaving.Text = "保存书目记录时自动校验数据(&V)";
            this.checkBox_marcDetailForm_verifyDataWhenSaving.UseVisualStyleBackColor = true;
            // 
            // tabPage_server
            // 
            this.tabPage_server.AutoScroll = true;
            this.tabPage_server.Controls.Add(this.textBox_server_pinyin_gcatUrl);
            this.tabPage_server.Controls.Add(this.label20);
            this.tabPage_server.Controls.Add(this.textBox_server_authorNumber_gcatUrl);
            this.tabPage_server.Controls.Add(this.label14);
            this.tabPage_server.Location = new System.Drawing.Point(4, 22);
            this.tabPage_server.Name = "tabPage_server";
            this.tabPage_server.Size = new System.Drawing.Size(374, 232);
            this.tabPage_server.TabIndex = 5;
            this.tabPage_server.Text = "服务器";
            this.tabPage_server.UseVisualStyleBackColor = true;
            // 
            // textBox_server_pinyin_gcatUrl
            // 
            this.textBox_server_pinyin_gcatUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_pinyin_gcatUrl.Location = new System.Drawing.Point(5, 103);
            this.textBox_server_pinyin_gcatUrl.Name = "textBox_server_pinyin_gcatUrl";
            this.textBox_server_pinyin_gcatUrl.Size = new System.Drawing.Size(366, 21);
            this.textBox_server_pinyin_gcatUrl.TabIndex = 9;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(3, 87);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(101, 12);
            this.label20.TabIndex = 8;
            this.label20.Text = "拼音 服务器 URL:";
            // 
            // textBox_server_authorNumber_gcatUrl
            // 
            this.textBox_server_authorNumber_gcatUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_authorNumber_gcatUrl.Location = new System.Drawing.Point(5, 33);
            this.textBox_server_authorNumber_gcatUrl.Name = "textBox_server_authorNumber_gcatUrl";
            this.textBox_server_authorNumber_gcatUrl.Size = new System.Drawing.Size(366, 21);
            this.textBox_server_authorNumber_gcatUrl.TabIndex = 7;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 17);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(155, 12);
            this.label14.TabIndex = 6;
            this.label14.Text = "著者号码 GCAT 服务器 URL:";
            // 
            // tabPage_global
            // 
            this.tabPage_global.Controls.Add(this.checkBox_global_autoSelPinyin);
            this.tabPage_global.Location = new System.Drawing.Point(4, 22);
            this.tabPage_global.Name = "tabPage_global";
            this.tabPage_global.Size = new System.Drawing.Size(374, 232);
            this.tabPage_global.TabIndex = 7;
            this.tabPage_global.Text = "全局";
            this.tabPage_global.UseVisualStyleBackColor = true;
            // 
            // checkBox_global_autoSelPinyin
            // 
            this.checkBox_global_autoSelPinyin.AutoSize = true;
            this.checkBox_global_autoSelPinyin.Location = new System.Drawing.Point(3, 27);
            this.checkBox_global_autoSelPinyin.Name = "checkBox_global_autoSelPinyin";
            this.checkBox_global_autoSelPinyin.Size = new System.Drawing.Size(174, 16);
            this.checkBox_global_autoSelPinyin.TabIndex = 16;
            this.checkBox_global_autoSelPinyin.Text = "加拼音时自动选择多音字(&A)";
            this.checkBox_global_autoSelPinyin.UseVisualStyleBackColor = true;
            // 
            // SystemCfgForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(395, 297);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SystemCfgForm";
            this.ShowInTaskbar = false;
            this.Text = "参数配置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SystemCfgForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SystemCfgForm_FormClosed);
            this.Load += new System.EventHandler(this.SystemCfgForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_dtlp.ResumeLayout(false);
            this.tabPage_dtlp.PerformLayout();
            this.tabPage_dp2library.ResumeLayout(false);
            this.tabPage_dp2library.PerformLayout();
            this.tabPage_dp2SearchForm.ResumeLayout(false);
            this.tabPage_dp2SearchForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_dp2library_searchMaxCount)).EndInit();
            this.tabPage_amazonSearchForm.ResumeLayout(false);
            this.tabPage_amazonSearchForm.PerformLayout();
            this.tabPage_ui.ResumeLayout(false);
            this.tabPage_ui.PerformLayout();
            this.tabPage_marcDetailForm.ResumeLayout(false);
            this.tabPage_marcDetailForm.PerformLayout();
            this.tabPage_server.ResumeLayout(false);
            this.tabPage_server.PerformLayout();
            this.tabPage_global.ResumeLayout(false);
            this.tabPage_global.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_dtlp;
        private System.Windows.Forms.CheckBox checkBox_dtlpSavePassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dtlpDefaultPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_dtlpDefaultUserName;
        private System.Windows.Forms.Button button_dtlp_hostManage;
        private System.Windows.Forms.TabPage tabPage_dp2library;
        private System.Windows.Forms.Button button_dp2library_serverManage;
        private System.Windows.Forms.Button button_dtlp_dupCfg;
        private System.Windows.Forms.Button button_dp2library_searchDup_findDefaultStartPath;
        private System.Windows.Forms.TextBox textBox_dp2library_searchDup_defaultStartPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabPage_dp2SearchForm;
        private System.Windows.Forms.ComboBox comboBox_dp2SearchForm_layout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabPage_ui;
        private System.Windows.Forms.CheckBox checkBox_ui_hideFixedPanel;
        private System.Windows.Forms.ComboBox comboBox_ui_fixedPanelDock;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabPage_marcDetailForm;
        private System.Windows.Forms.CheckBox checkBox_marcDetailForm_verifyDataWhenSaving;
        private System.Windows.Forms.CheckBox checkBox_dp2SearchForm_useExistDetailWindow;
        private System.Windows.Forms.NumericUpDown numericUpDown_dp2library_searchMaxCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_ui_getDefaultFont;
        private System.Windows.Forms.TextBox textBox_ui_defaultFont;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TabPage tabPage_server;
        private System.Windows.Forms.TextBox textBox_server_pinyin_gcatUrl;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBox_server_authorNumber_gcatUrl;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TabPage tabPage_amazonSearchForm;
        private System.Windows.Forms.CheckBox checkBox_amazon_alwaysUseFullElementSet;
        private System.Windows.Forms.Label label7;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_amazon_defaultServer;
        private System.Windows.Forms.TabPage tabPage_global;
        private System.Windows.Forms.CheckBox checkBox_global_autoSelPinyin;
    }
}