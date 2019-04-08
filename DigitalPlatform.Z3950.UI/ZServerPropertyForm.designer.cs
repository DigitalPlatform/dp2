namespace DigitalPlatform.Z3950
{
    partial class ZServerPropertyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZServerPropertyForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.button_gotoHomepage = new System.Windows.Forms.Button();
            this.textBox_homepage = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox_initializeInfomation = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_serverPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serverName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_database = new System.Windows.Forms.TabPage();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox_notInAllDatabaseNames = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_databaseNames = new System.Windows.Forms.TextBox();
            this.tabPage_accessControl = new System.Windows.Forms.TabPage();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_groupID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_authenStyleIdpass = new System.Windows.Forms.RadioButton();
            this.radioButton_authenStyeOpen = new System.Windows.Forms.RadioButton();
            this.tabPage_search = new System.Windows.Forms.TabPage();
            this.checkBox_forceIssn8 = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox_isbn_wild = new System.Windows.Forms.CheckBox();
            this.checkBox_isbn_removeHyphen = new System.Windows.Forms.CheckBox();
            this.checkBox_isbn_addHyphen = new System.Windows.Forms.CheckBox();
            this.checkBox_isbn_forceIsbn10 = new System.Windows.Forms.CheckBox();
            this.checkBox_isbn_forceIsbn13 = new System.Windows.Forms.CheckBox();
            this.checkBox_ignoreRerenceID = new System.Windows.Forms.CheckBox();
            this.comboBox_defaultElementSetName = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.comboBox_defaultMarcSyntaxOID = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.checkBox_autoDetectMarcSyntax = new System.Windows.Forms.CheckBox();
            this.textBox_presentPerCount = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBox_alwaysUseFullElementSet = new System.Windows.Forms.CheckBox();
            this.tabPage_charset = new System.Windows.Forms.TabPage();
            this.checkBox_charNegoRecordsInSelectedCharSets = new System.Windows.Forms.CheckBox();
            this.checkBox_charNegoUTF8 = new System.Windows.Forms.CheckBox();
            this.comboBox_queryTermEncoding = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listView_recordSyntaxAndEncodingBinding = new System.Windows.Forms.ListView();
            this.columnHeader_recordSyntax = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_encoding = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_deleteBindingItem = new System.Windows.Forms.Button();
            this.checkBox_autoDetectEACC = new System.Windows.Forms.CheckBox();
            this.button_modifyBindingItem = new System.Windows.Forms.Button();
            this.button_newBindingItem = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBox_defaultEncoding = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tabPage_MARC = new System.Windows.Forms.TabPage();
            this.tabPage_unionCatalog = new System.Windows.Forms.TabPage();
            this.textBox_unionCatalog_bindingUcServerUrl = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.button_unionCatalog_findDp2Server = new System.Windows.Forms.Button();
            this.textBox_unionCatalog_bindingDp2ServerName = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            this.tabPage_database.SuspendLayout();
            this.tabPage_accessControl.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage_search.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabPage_charset.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage_unionCatalog.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_general);
            this.tabControl_main.Controls.Add(this.tabPage_database);
            this.tabControl_main.Controls.Add(this.tabPage_accessControl);
            this.tabControl_main.Controls.Add(this.tabPage_search);
            this.tabControl_main.Controls.Add(this.tabPage_charset);
            this.tabControl_main.Controls.Add(this.tabPage_MARC);
            this.tabControl_main.Controls.Add(this.tabPage_unionCatalog);
            this.tabControl_main.Location = new System.Drawing.Point(15, 15);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(573, 519);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.button_gotoHomepage);
            this.tabPage_general.Controls.Add(this.textBox_homepage);
            this.tabPage_general.Controls.Add(this.label16);
            this.tabPage_general.Controls.Add(this.textBox_initializeInfomation);
            this.tabPage_general.Controls.Add(this.label4);
            this.tabPage_general.Controls.Add(this.textBox_serverPort);
            this.tabPage_general.Controls.Add(this.label3);
            this.tabPage_general.Controls.Add(this.textBox_serverAddr);
            this.tabPage_general.Controls.Add(this.label2);
            this.tabPage_general.Controls.Add(this.textBox_serverName);
            this.tabPage_general.Controls.Add(this.label1);
            this.tabPage_general.Location = new System.Drawing.Point(4, 28);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_general.Size = new System.Drawing.Size(565, 487);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "一般属性";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // button_gotoHomepage
            // 
            this.button_gotoHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_gotoHomepage.Image = ((System.Drawing.Image)(resources.GetObject("button_gotoHomepage.Image")));
            this.button_gotoHomepage.Location = new System.Drawing.Point(525, 129);
            this.button_gotoHomepage.Name = "button_gotoHomepage";
            this.button_gotoHomepage.Size = new System.Drawing.Size(33, 33);
            this.button_gotoHomepage.TabIndex = 10;
            this.button_gotoHomepage.UseVisualStyleBackColor = true;
            this.button_gotoHomepage.Click += new System.EventHandler(this.button_gotoHomepage_Click);
            // 
            // textBox_homepage
            // 
            this.textBox_homepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_homepage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_homepage.Location = new System.Drawing.Point(147, 129);
            this.textBox_homepage.Name = "textBox_homepage";
            this.textBox_homepage.Size = new System.Drawing.Size(370, 28);
            this.textBox_homepage.TabIndex = 7;
            this.textBox_homepage.TextChanged += new System.EventHandler(this.textBox_homepage_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 135);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(107, 18);
            this.label16.TabIndex = 6;
            this.label16.Text = "Web主页(&H):";
            // 
            // textBox_initializeInfomation
            // 
            this.textBox_initializeInfomation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_initializeInfomation.Location = new System.Drawing.Point(6, 200);
            this.textBox_initializeInfomation.Multiline = true;
            this.textBox_initializeInfomation.Name = "textBox_initializeInfomation";
            this.textBox_initializeInfomation.ReadOnly = true;
            this.textBox_initializeInfomation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_initializeInfomation.Size = new System.Drawing.Size(550, 265);
            this.textBox_initializeInfomation.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 177);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "初始化信息(&I):";
            // 
            // textBox_serverPort
            // 
            this.textBox_serverPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_serverPort.Location = new System.Drawing.Point(147, 93);
            this.textBox_serverPort.Name = "textBox_serverPort";
            this.textBox_serverPort.Size = new System.Drawing.Size(143, 28);
            this.textBox_serverPort.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "端口号(&P):";
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverAddr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_serverAddr.Location = new System.Drawing.Point(147, 54);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(410, 28);
            this.textBox_serverAddr.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "地址(&A):";
            // 
            // textBox_serverName
            // 
            this.textBox_serverName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_serverName.Location = new System.Drawing.Point(147, 15);
            this.textBox_serverName.Name = "textBox_serverName";
            this.textBox_serverName.Size = new System.Drawing.Size(410, 28);
            this.textBox_serverName.TabIndex = 1;
            this.textBox_serverName.TextChanged += new System.EventHandler(this.textBox_serverName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器名(&N):";
            // 
            // tabPage_database
            // 
            this.tabPage_database.Controls.Add(this.label19);
            this.tabPage_database.Controls.Add(this.textBox_notInAllDatabaseNames);
            this.tabPage_database.Controls.Add(this.label10);
            this.tabPage_database.Controls.Add(this.label9);
            this.tabPage_database.Controls.Add(this.textBox_databaseNames);
            this.tabPage_database.Location = new System.Drawing.Point(4, 28);
            this.tabPage_database.Name = "tabPage_database";
            this.tabPage_database.Size = new System.Drawing.Size(565, 487);
            this.tabPage_database.TabIndex = 3;
            this.tabPage_database.Text = "数据库";
            this.tabPage_database.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(4, 330);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(278, 18);
            this.label19.TabIndex = 4;
            this.label19.Text = "全选时不参与检索的数据库名(&X):";
            // 
            // textBox_notInAllDatabaseNames
            // 
            this.textBox_notInAllDatabaseNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_notInAllDatabaseNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_notInAllDatabaseNames.Location = new System.Drawing.Point(4, 351);
            this.textBox_notInAllDatabaseNames.Multiline = true;
            this.textBox_notInAllDatabaseNames.Name = "textBox_notInAllDatabaseNames";
            this.textBox_notInAllDatabaseNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_notInAllDatabaseNames.Size = new System.Drawing.Size(557, 107);
            this.textBox_notInAllDatabaseNames.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 282);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(440, 18);
            this.label10.TabIndex = 2;
            this.label10.Text = "(注：可输入多个数据库名。格式为每行一个数据库名)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 26);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(116, 18);
            this.label9.TabIndex = 1;
            this.label9.Text = "数据库名(&N):";
            // 
            // textBox_databaseNames
            // 
            this.textBox_databaseNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_databaseNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_databaseNames.Location = new System.Drawing.Point(4, 51);
            this.textBox_databaseNames.Multiline = true;
            this.textBox_databaseNames.Name = "textBox_databaseNames";
            this.textBox_databaseNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_databaseNames.Size = new System.Drawing.Size(557, 222);
            this.textBox_databaseNames.TabIndex = 0;
            // 
            // tabPage_accessControl
            // 
            this.tabPage_accessControl.Controls.Add(this.textBox_password);
            this.tabPage_accessControl.Controls.Add(this.label7);
            this.tabPage_accessControl.Controls.Add(this.textBox_userName);
            this.tabPage_accessControl.Controls.Add(this.label6);
            this.tabPage_accessControl.Controls.Add(this.textBox_groupID);
            this.tabPage_accessControl.Controls.Add(this.label5);
            this.tabPage_accessControl.Controls.Add(this.groupBox1);
            this.tabPage_accessControl.Location = new System.Drawing.Point(4, 28);
            this.tabPage_accessControl.Name = "tabPage_accessControl";
            this.tabPage_accessControl.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_accessControl.Size = new System.Drawing.Size(565, 487);
            this.tabPage_accessControl.TabIndex = 1;
            this.tabPage_accessControl.Text = "权限验证";
            this.tabPage_accessControl.UseVisualStyleBackColor = true;
            // 
            // textBox_password
            // 
            this.textBox_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_password.Location = new System.Drawing.Point(123, 218);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(222, 28);
            this.textBox_password.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 220);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 18);
            this.label7.TabIndex = 5;
            this.label7.Text = "密码(&P):";
            // 
            // textBox_userName
            // 
            this.textBox_userName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_userName.Location = new System.Drawing.Point(123, 180);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(222, 28);
            this.textBox_userName.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 18);
            this.label6.TabIndex = 3;
            this.label6.Text = "用户名(&U):";
            // 
            // textBox_groupID
            // 
            this.textBox_groupID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_groupID.Location = new System.Drawing.Point(123, 142);
            this.textBox_groupID.Name = "textBox_groupID";
            this.textBox_groupID.Size = new System.Drawing.Size(222, 28);
            this.textBox_groupID.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 18);
            this.label5.TabIndex = 1;
            this.label5.Text = "&Group ID:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_authenStyleIdpass);
            this.groupBox1.Controls.Add(this.radioButton_authenStyeOpen);
            this.groupBox1.Location = new System.Drawing.Point(8, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(339, 116);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 权限验证方式 ";
            // 
            // radioButton_authenStyleIdpass
            // 
            this.radioButton_authenStyleIdpass.AutoSize = true;
            this.radioButton_authenStyleIdpass.Location = new System.Drawing.Point(21, 62);
            this.radioButton_authenStyleIdpass.Name = "radioButton_authenStyleIdpass";
            this.radioButton_authenStyleIdpass.Size = new System.Drawing.Size(96, 22);
            this.radioButton_authenStyleIdpass.TabIndex = 1;
            this.radioButton_authenStyleIdpass.TabStop = true;
            this.radioButton_authenStyleIdpass.Text = "&ID/Pass";
            this.radioButton_authenStyleIdpass.UseVisualStyleBackColor = true;
            this.radioButton_authenStyleIdpass.CheckedChanged += new System.EventHandler(this.radioButton_authenStyleIdpass_CheckedChanged);
            // 
            // radioButton_authenStyeOpen
            // 
            this.radioButton_authenStyeOpen.AutoSize = true;
            this.radioButton_authenStyeOpen.Location = new System.Drawing.Point(21, 32);
            this.radioButton_authenStyeOpen.Name = "radioButton_authenStyeOpen";
            this.radioButton_authenStyeOpen.Size = new System.Drawing.Size(69, 22);
            this.radioButton_authenStyeOpen.TabIndex = 0;
            this.radioButton_authenStyeOpen.TabStop = true;
            this.radioButton_authenStyeOpen.Text = "&Open";
            this.radioButton_authenStyeOpen.UseVisualStyleBackColor = true;
            this.radioButton_authenStyeOpen.CheckedChanged += new System.EventHandler(this.radioButton_authenStyeOpen_CheckedChanged);
            // 
            // tabPage_search
            // 
            this.tabPage_search.AutoScroll = true;
            this.tabPage_search.Controls.Add(this.checkBox_forceIssn8);
            this.tabPage_search.Controls.Add(this.groupBox3);
            this.tabPage_search.Controls.Add(this.checkBox_ignoreRerenceID);
            this.tabPage_search.Controls.Add(this.comboBox_defaultElementSetName);
            this.tabPage_search.Controls.Add(this.label14);
            this.tabPage_search.Controls.Add(this.comboBox_defaultMarcSyntaxOID);
            this.tabPage_search.Controls.Add(this.label12);
            this.tabPage_search.Controls.Add(this.checkBox_autoDetectMarcSyntax);
            this.tabPage_search.Controls.Add(this.textBox_presentPerCount);
            this.tabPage_search.Controls.Add(this.label8);
            this.tabPage_search.Controls.Add(this.checkBox_alwaysUseFullElementSet);
            this.tabPage_search.Location = new System.Drawing.Point(4, 28);
            this.tabPage_search.Name = "tabPage_search";
            this.tabPage_search.Size = new System.Drawing.Size(565, 487);
            this.tabPage_search.TabIndex = 2;
            this.tabPage_search.Text = "检索/获取";
            this.tabPage_search.UseVisualStyleBackColor = true;
            // 
            // checkBox_forceIssn8
            // 
            this.checkBox_forceIssn8.AutoSize = true;
            this.checkBox_forceIssn8.Location = new System.Drawing.Point(296, 272);
            this.checkBox_forceIssn8.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_forceIssn8.Name = "checkBox_forceIssn8";
            this.checkBox_forceIssn8.Size = new System.Drawing.Size(250, 22);
            this.checkBox_forceIssn8.TabIndex = 15;
            this.checkBox_forceIssn8.Text = "ISSN 自动规整为 8 位形态";
            this.checkBox_forceIssn8.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBox_isbn_wild);
            this.groupBox3.Controls.Add(this.checkBox_isbn_removeHyphen);
            this.groupBox3.Controls.Add(this.checkBox_isbn_addHyphen);
            this.groupBox3.Controls.Add(this.checkBox_isbn_forceIsbn10);
            this.groupBox3.Controls.Add(this.checkBox_isbn_forceIsbn13);
            this.groupBox3.Location = new System.Drawing.Point(8, 326);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(544, 132);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "ISBN检索前，对检索词作如下预处理";
            // 
            // checkBox_isbn_wild
            // 
            this.checkBox_isbn_wild.AutoSize = true;
            this.checkBox_isbn_wild.Location = new System.Drawing.Point(28, 99);
            this.checkBox_isbn_wild.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_isbn_wild.Name = "checkBox_isbn_wild";
            this.checkBox_isbn_wild.Size = new System.Drawing.Size(133, 22);
            this.checkBox_isbn_wild.TabIndex = 4;
            this.checkBox_isbn_wild.Text = "野蛮匹配(&W)";
            this.checkBox_isbn_wild.UseVisualStyleBackColor = true;
            // 
            // checkBox_isbn_removeHyphen
            // 
            this.checkBox_isbn_removeHyphen.AutoSize = true;
            this.checkBox_isbn_removeHyphen.Location = new System.Drawing.Point(288, 64);
            this.checkBox_isbn_removeHyphen.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_isbn_removeHyphen.Name = "checkBox_isbn_removeHyphen";
            this.checkBox_isbn_removeHyphen.Size = new System.Drawing.Size(133, 22);
            this.checkBox_isbn_removeHyphen.TabIndex = 3;
            this.checkBox_isbn_removeHyphen.Text = "去除横杠(&D)";
            this.checkBox_isbn_removeHyphen.UseVisualStyleBackColor = true;
            this.checkBox_isbn_removeHyphen.CheckedChanged += new System.EventHandler(this.checkBox_isbn_removeHyphen_CheckedChanged);
            // 
            // checkBox_isbn_addHyphen
            // 
            this.checkBox_isbn_addHyphen.AutoSize = true;
            this.checkBox_isbn_addHyphen.Location = new System.Drawing.Point(28, 64);
            this.checkBox_isbn_addHyphen.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_isbn_addHyphen.Name = "checkBox_isbn_addHyphen";
            this.checkBox_isbn_addHyphen.Size = new System.Drawing.Size(133, 22);
            this.checkBox_isbn_addHyphen.TabIndex = 2;
            this.checkBox_isbn_addHyphen.Text = "加入横杠(&H)";
            this.checkBox_isbn_addHyphen.UseVisualStyleBackColor = true;
            this.checkBox_isbn_addHyphen.CheckedChanged += new System.EventHandler(this.checkBox_isbn_addHyphen_CheckedChanged);
            // 
            // checkBox_isbn_forceIsbn10
            // 
            this.checkBox_isbn_forceIsbn10.AutoSize = true;
            this.checkBox_isbn_forceIsbn10.Location = new System.Drawing.Point(288, 32);
            this.checkBox_isbn_forceIsbn10.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_isbn_forceIsbn10.Name = "checkBox_isbn_forceIsbn10";
            this.checkBox_isbn_forceIsbn10.Size = new System.Drawing.Size(178, 22);
            this.checkBox_isbn_forceIsbn10.TabIndex = 1;
            this.checkBox_isbn_forceIsbn10.Text = "规整为 10 位形态";
            this.checkBox_isbn_forceIsbn10.UseVisualStyleBackColor = true;
            this.checkBox_isbn_forceIsbn10.CheckedChanged += new System.EventHandler(this.checkBox_isbn_forceIsbn10_CheckedChanged);
            // 
            // checkBox_isbn_forceIsbn13
            // 
            this.checkBox_isbn_forceIsbn13.AutoSize = true;
            this.checkBox_isbn_forceIsbn13.Location = new System.Drawing.Point(28, 32);
            this.checkBox_isbn_forceIsbn13.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_isbn_forceIsbn13.Name = "checkBox_isbn_forceIsbn13";
            this.checkBox_isbn_forceIsbn13.Size = new System.Drawing.Size(178, 22);
            this.checkBox_isbn_forceIsbn13.TabIndex = 0;
            this.checkBox_isbn_forceIsbn13.Text = "规整为 13 位形态";
            this.checkBox_isbn_forceIsbn13.UseVisualStyleBackColor = true;
            this.checkBox_isbn_forceIsbn13.CheckedChanged += new System.EventHandler(this.checkBox_isbn_forceIsbn13_CheckedChanged);
            // 
            // checkBox_ignoreRerenceID
            // 
            this.checkBox_ignoreRerenceID.AutoSize = true;
            this.checkBox_ignoreRerenceID.Location = new System.Drawing.Point(8, 272);
            this.checkBox_ignoreRerenceID.Name = "checkBox_ignoreRerenceID";
            this.checkBox_ignoreRerenceID.Size = new System.Drawing.Size(241, 22);
            this.checkBox_ignoreRerenceID.TabIndex = 13;
            this.checkBox_ignoreRerenceID.Text = "不检查 Reference ID (&R)";
            this.checkBox_ignoreRerenceID.UseVisualStyleBackColor = true;
            // 
            // comboBox_defaultElementSetName
            // 
            this.comboBox_defaultElementSetName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_defaultElementSetName.DropDownHeight = 300;
            this.comboBox_defaultElementSetName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_defaultElementSetName.FormattingEnabled = true;
            this.comboBox_defaultElementSetName.IntegralHeight = false;
            this.comboBox_defaultElementSetName.Items.AddRange(new object[] {
            "B -- Brief(MARC records)",
            "F -- Full (MARC and OPAC records)",
            "dc --  Dublin Core (XML records)",
            "mods  -- MODS (XML records)",
            "marcxml -- MARCXML (XML records), default schema for XML",
            "opacxml -- MARCXML with holdings attached"});
            this.comboBox_defaultElementSetName.Location = new System.Drawing.Point(201, 141);
            this.comboBox_defaultElementSetName.Name = "comboBox_defaultElementSetName";
            this.comboBox_defaultElementSetName.Size = new System.Drawing.Size(349, 26);
            this.comboBox_defaultElementSetName.TabIndex = 10;
            this.comboBox_defaultElementSetName.SizeChanged += new System.EventHandler(this.comboBox_defaultElementSetName_SizeChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(4, 144);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(152, 18);
            this.label14.TabIndex = 9;
            this.label14.Text = "缺省元素集名(&E):";
            // 
            // comboBox_defaultMarcSyntaxOID
            // 
            this.comboBox_defaultMarcSyntaxOID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_defaultMarcSyntaxOID.DropDownHeight = 300;
            this.comboBox_defaultMarcSyntaxOID.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_defaultMarcSyntaxOID.FormattingEnabled = true;
            this.comboBox_defaultMarcSyntaxOID.IntegralHeight = false;
            this.comboBox_defaultMarcSyntaxOID.Items.AddRange(new object[] {
            "1.2.840.10003.5.1 -- UNIMARC",
            "1.2.840.10003.5.10 -- MARC21",
            "1.2.840.10003.5.101 -- SUTRS",
            "1.2.840.10003.5.109.10 -- XML"});
            this.comboBox_defaultMarcSyntaxOID.Location = new System.Drawing.Point(201, 58);
            this.comboBox_defaultMarcSyntaxOID.Name = "comboBox_defaultMarcSyntaxOID";
            this.comboBox_defaultMarcSyntaxOID.Size = new System.Drawing.Size(349, 26);
            this.comboBox_defaultMarcSyntaxOID.TabIndex = 3;
            this.comboBox_defaultMarcSyntaxOID.SizeChanged += new System.EventHandler(this.comboBox_defaultMarcSyntaxOID_SizeChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(4, 63);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(179, 18);
            this.label12.TabIndex = 2;
            this.label12.Text = "缺省数据格式OID(&S):";
            // 
            // checkBox_autoDetectMarcSyntax
            // 
            this.checkBox_autoDetectMarcSyntax.AutoSize = true;
            this.checkBox_autoDetectMarcSyntax.Location = new System.Drawing.Point(8, 219);
            this.checkBox_autoDetectMarcSyntax.Name = "checkBox_autoDetectMarcSyntax";
            this.checkBox_autoDetectMarcSyntax.Size = new System.Drawing.Size(241, 22);
            this.checkBox_autoDetectMarcSyntax.TabIndex = 12;
            this.checkBox_autoDetectMarcSyntax.Text = "自动探测MARC记录格式(&M)";
            this.checkBox_autoDetectMarcSyntax.UseVisualStyleBackColor = true;
            // 
            // textBox_presentPerCount
            // 
            this.textBox_presentPerCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_presentPerCount.Location = new System.Drawing.Point(201, 18);
            this.textBox_presentPerCount.Name = "textBox_presentPerCount";
            this.textBox_presentPerCount.Size = new System.Drawing.Size(113, 28);
            this.textBox_presentPerCount.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(188, 18);
            this.label8.TabIndex = 0;
            this.label8.Text = "获取记录每批条数(&C):";
            // 
            // checkBox_alwaysUseFullElementSet
            // 
            this.checkBox_alwaysUseFullElementSet.AutoSize = true;
            this.checkBox_alwaysUseFullElementSet.Location = new System.Drawing.Point(8, 176);
            this.checkBox_alwaysUseFullElementSet.Name = "checkBox_alwaysUseFullElementSet";
            this.checkBox_alwaysUseFullElementSet.Size = new System.Drawing.Size(331, 22);
            this.checkBox_alwaysUseFullElementSet.TabIndex = 11;
            this.checkBox_alwaysUseFullElementSet.Text = "在获取浏览记录阶段即获得全记录(&F)";
            this.checkBox_alwaysUseFullElementSet.UseVisualStyleBackColor = true;
            // 
            // tabPage_charset
            // 
            this.tabPage_charset.Controls.Add(this.checkBox_charNegoRecordsInSelectedCharSets);
            this.tabPage_charset.Controls.Add(this.checkBox_charNegoUTF8);
            this.tabPage_charset.Controls.Add(this.comboBox_queryTermEncoding);
            this.tabPage_charset.Controls.Add(this.label13);
            this.tabPage_charset.Controls.Add(this.groupBox2);
            this.tabPage_charset.Location = new System.Drawing.Point(4, 28);
            this.tabPage_charset.Name = "tabPage_charset";
            this.tabPage_charset.Size = new System.Drawing.Size(565, 487);
            this.tabPage_charset.TabIndex = 4;
            this.tabPage_charset.Text = "字符集";
            this.tabPage_charset.UseVisualStyleBackColor = true;
            // 
            // checkBox_charNegoRecordsInSelectedCharSets
            // 
            this.checkBox_charNegoRecordsInSelectedCharSets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_charNegoRecordsInSelectedCharSets.AutoSize = true;
            this.checkBox_charNegoRecordsInSelectedCharSets.Location = new System.Drawing.Point(8, 421);
            this.checkBox_charNegoRecordsInSelectedCharSets.Name = "checkBox_charNegoRecordsInSelectedCharSets";
            this.checkBox_charNegoRecordsInSelectedCharSets.Size = new System.Drawing.Size(574, 22);
            this.checkBox_charNegoRecordsInSelectedCharSets.TabIndex = 16;
            this.checkBox_charNegoRecordsInSelectedCharSets.Text = "若启用了字符集协商功能，令数据记录也一同采用UTF-8编码方式(&S)";
            this.checkBox_charNegoRecordsInSelectedCharSets.UseVisualStyleBackColor = true;
            // 
            // checkBox_charNegoUTF8
            // 
            this.checkBox_charNegoUTF8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_charNegoUTF8.AutoSize = true;
            this.checkBox_charNegoUTF8.Location = new System.Drawing.Point(8, 389);
            this.checkBox_charNegoUTF8.Name = "checkBox_charNegoUTF8";
            this.checkBox_charNegoUTF8.Size = new System.Drawing.Size(502, 22);
            this.checkBox_charNegoUTF8.TabIndex = 15;
            this.checkBox_charNegoUTF8.Text = "启用字符集协商功能，优先为检索词选用UTF-8编码方式(&N)";
            this.checkBox_charNegoUTF8.UseVisualStyleBackColor = true;
            this.checkBox_charNegoUTF8.CheckedChanged += new System.EventHandler(this.checkBox_charNegoUTF8_CheckedChanged);
            // 
            // comboBox_queryTermEncoding
            // 
            this.comboBox_queryTermEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_queryTermEncoding.DropDownHeight = 300;
            this.comboBox_queryTermEncoding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_queryTermEncoding.FormattingEnabled = true;
            this.comboBox_queryTermEncoding.IntegralHeight = false;
            this.comboBox_queryTermEncoding.Items.AddRange(new object[] {
            "UTF-8",
            "GB2312",
            "BIG5",
            "ks_c_5601-1987",
            "shift_jis"});
            this.comboBox_queryTermEncoding.Location = new System.Drawing.Point(204, 16);
            this.comboBox_queryTermEncoding.Name = "comboBox_queryTermEncoding";
            this.comboBox_queryTermEncoding.Size = new System.Drawing.Size(228, 26);
            this.comboBox_queryTermEncoding.TabIndex = 8;
            this.comboBox_queryTermEncoding.Text = "GB2312";
            this.comboBox_queryTermEncoding.SizeChanged += new System.EventHandler(this.comboBox_queryTermEncoding_SizeChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(4, 21);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(206, 18);
            this.label13.TabIndex = 7;
            this.label13.Text = "检索词缺省编码方式(&T):";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.listView_recordSyntaxAndEncodingBinding);
            this.groupBox2.Controls.Add(this.button_deleteBindingItem);
            this.groupBox2.Controls.Add(this.checkBox_autoDetectEACC);
            this.groupBox2.Controls.Add(this.button_modifyBindingItem);
            this.groupBox2.Controls.Add(this.button_newBindingItem);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.comboBox_defaultEncoding);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Location = new System.Drawing.Point(8, 69);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(543, 296);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " 数据记录缺省编码方式 ";
            // 
            // listView_recordSyntaxAndEncodingBinding
            // 
            this.listView_recordSyntaxAndEncodingBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_recordSyntaxAndEncodingBinding.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_recordSyntax,
            this.columnHeader_encoding});
            this.listView_recordSyntaxAndEncodingBinding.Location = new System.Drawing.Point(195, 105);
            this.listView_recordSyntaxAndEncodingBinding.Name = "listView_recordSyntaxAndEncodingBinding";
            this.listView_recordSyntaxAndEncodingBinding.Size = new System.Drawing.Size(313, 174);
            this.listView_recordSyntaxAndEncodingBinding.TabIndex = 13;
            this.listView_recordSyntaxAndEncodingBinding.UseCompatibleStateImageBehavior = false;
            this.listView_recordSyntaxAndEncodingBinding.View = System.Windows.Forms.View.Details;
            this.listView_recordSyntaxAndEncodingBinding.SelectedIndexChanged += new System.EventHandler(this.listView_recordSyntaxAndEncodingBinding_SelectedIndexChanged);
            // 
            // columnHeader_recordSyntax
            // 
            this.columnHeader_recordSyntax.Text = "数据格式";
            this.columnHeader_recordSyntax.Width = 192;
            // 
            // columnHeader_encoding
            // 
            this.columnHeader_encoding.Text = "编码方式";
            this.columnHeader_encoding.Width = 128;
            // 
            // button_deleteBindingItem
            // 
            this.button_deleteBindingItem.Enabled = false;
            this.button_deleteBindingItem.Location = new System.Drawing.Point(68, 186);
            this.button_deleteBindingItem.Name = "button_deleteBindingItem";
            this.button_deleteBindingItem.Size = new System.Drawing.Size(122, 33);
            this.button_deleteBindingItem.TabIndex = 12;
            this.button_deleteBindingItem.Text = "删除(&D)...";
            this.button_deleteBindingItem.UseVisualStyleBackColor = true;
            this.button_deleteBindingItem.Click += new System.EventHandler(this.button_deleteBindingItem_Click);
            // 
            // checkBox_autoDetectEACC
            // 
            this.checkBox_autoDetectEACC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoDetectEACC.AutoSize = true;
            this.checkBox_autoDetectEACC.Enabled = false;
            this.checkBox_autoDetectEACC.Location = new System.Drawing.Point(32, 256);
            this.checkBox_autoDetectEACC.Name = "checkBox_autoDetectEACC";
            this.checkBox_autoDetectEACC.Size = new System.Drawing.Size(313, 22);
            this.checkBox_autoDetectEACC.TabIndex = 4;
            this.checkBox_autoDetectEACC.Text = "自动转换MARC记录中的EACC字符(&E)";
            this.checkBox_autoDetectEACC.UseVisualStyleBackColor = true;
            this.checkBox_autoDetectEACC.Visible = false;
            // 
            // button_modifyBindingItem
            // 
            this.button_modifyBindingItem.Enabled = false;
            this.button_modifyBindingItem.Location = new System.Drawing.Point(68, 146);
            this.button_modifyBindingItem.Name = "button_modifyBindingItem";
            this.button_modifyBindingItem.Size = new System.Drawing.Size(122, 33);
            this.button_modifyBindingItem.TabIndex = 11;
            this.button_modifyBindingItem.Text = "修改(&M)...";
            this.button_modifyBindingItem.UseVisualStyleBackColor = true;
            this.button_modifyBindingItem.Click += new System.EventHandler(this.button_modifyBindingItem_Click);
            // 
            // button_newBindingItem
            // 
            this.button_newBindingItem.Location = new System.Drawing.Point(68, 105);
            this.button_newBindingItem.Name = "button_newBindingItem";
            this.button_newBindingItem.Size = new System.Drawing.Size(122, 33);
            this.button_newBindingItem.TabIndex = 10;
            this.button_newBindingItem.Text = "新增(&N)...";
            this.button_newBindingItem.UseVisualStyleBackColor = true;
            this.button_newBindingItem.Click += new System.EventHandler(this.button_newBindingItem_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(28, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(152, 18);
            this.label11.TabIndex = 5;
            this.label11.Text = "缺省编码方式(&E):";
            // 
            // comboBox_defaultEncoding
            // 
            this.comboBox_defaultEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_defaultEncoding.DropDownHeight = 300;
            this.comboBox_defaultEncoding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_defaultEncoding.FormattingEnabled = true;
            this.comboBox_defaultEncoding.IntegralHeight = false;
            this.comboBox_defaultEncoding.Items.AddRange(new object[] {
            "UTF-8",
            "GB2312",
            "BIG5",
            "ks_c_5601-1987",
            "shift_jis"});
            this.comboBox_defaultEncoding.Location = new System.Drawing.Point(195, 38);
            this.comboBox_defaultEncoding.Name = "comboBox_defaultEncoding";
            this.comboBox_defaultEncoding.Size = new System.Drawing.Size(228, 26);
            this.comboBox_defaultEncoding.TabIndex = 6;
            this.comboBox_defaultEncoding.Text = "GB2312";
            this.comboBox_defaultEncoding.SizeChanged += new System.EventHandler(this.comboBox_defaultEncoding_SizeChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(28, 82);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(332, 18);
            this.label15.TabIndex = 9;
            this.label15.Text = "数据格式和字符集编码方式绑定关系(&B):";
            // 
            // tabPage_MARC
            // 
            this.tabPage_MARC.Location = new System.Drawing.Point(4, 28);
            this.tabPage_MARC.Name = "tabPage_MARC";
            this.tabPage_MARC.Size = new System.Drawing.Size(565, 487);
            this.tabPage_MARC.TabIndex = 5;
            this.tabPage_MARC.Text = "MARC";
            this.tabPage_MARC.UseVisualStyleBackColor = true;
            // 
            // tabPage_unionCatalog
            // 
            this.tabPage_unionCatalog.Controls.Add(this.textBox_unionCatalog_bindingUcServerUrl);
            this.tabPage_unionCatalog.Controls.Add(this.label18);
            this.tabPage_unionCatalog.Controls.Add(this.button_unionCatalog_findDp2Server);
            this.tabPage_unionCatalog.Controls.Add(this.textBox_unionCatalog_bindingDp2ServerName);
            this.tabPage_unionCatalog.Controls.Add(this.label17);
            this.tabPage_unionCatalog.Location = new System.Drawing.Point(4, 28);
            this.tabPage_unionCatalog.Name = "tabPage_unionCatalog";
            this.tabPage_unionCatalog.Size = new System.Drawing.Size(565, 487);
            this.tabPage_unionCatalog.TabIndex = 6;
            this.tabPage_unionCatalog.Text = "联合编目";
            this.tabPage_unionCatalog.UseVisualStyleBackColor = true;
            // 
            // textBox_unionCatalog_bindingUcServerUrl
            // 
            this.textBox_unionCatalog_bindingUcServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unionCatalog_bindingUcServerUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_unionCatalog_bindingUcServerUrl.Location = new System.Drawing.Point(8, 154);
            this.textBox_unionCatalog_bindingUcServerUrl.Name = "textBox_unionCatalog_bindingUcServerUrl";
            this.textBox_unionCatalog_bindingUcServerUrl.Size = new System.Drawing.Size(470, 28);
            this.textBox_unionCatalog_bindingUcServerUrl.TabIndex = 4;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(4, 134);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(332, 18);
            this.label18.TabIndex = 3;
            this.label18.Text = "所绑定的 UnionCatalog 服务器 URL(&U):";
            // 
            // button_unionCatalog_findDp2Server
            // 
            this.button_unionCatalog_findDp2Server.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_unionCatalog_findDp2Server.Location = new System.Drawing.Point(484, 54);
            this.button_unionCatalog_findDp2Server.Name = "button_unionCatalog_findDp2Server";
            this.button_unionCatalog_findDp2Server.Size = new System.Drawing.Size(64, 33);
            this.button_unionCatalog_findDp2Server.TabIndex = 2;
            this.button_unionCatalog_findDp2Server.Text = "...";
            this.button_unionCatalog_findDp2Server.UseVisualStyleBackColor = true;
            this.button_unionCatalog_findDp2Server.Click += new System.EventHandler(this.button_unionCatalog_findDp2Server_Click);
            // 
            // textBox_unionCatalog_bindingDp2ServerName
            // 
            this.textBox_unionCatalog_bindingDp2ServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unionCatalog_bindingDp2ServerName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_unionCatalog_bindingDp2ServerName.Location = new System.Drawing.Point(8, 54);
            this.textBox_unionCatalog_bindingDp2ServerName.Name = "textBox_unionCatalog_bindingDp2ServerName";
            this.textBox_unionCatalog_bindingDp2ServerName.Size = new System.Drawing.Size(470, 28);
            this.textBox_unionCatalog_bindingDp2ServerName.TabIndex = 1;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(4, 33);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(296, 18);
            this.label17.TabIndex = 0;
            this.label17.Text = "所绑定的 dp2library 服务器名(&S):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(412, 543);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 33);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(504, 543);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 33);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // ZServerPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 591);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ZServerPropertyForm";
            this.ShowInTaskbar = false;
            this.Text = "Z39.50服务器属性";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZServerPropertyForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ZServerPropertyForm_FormClosed);
            this.Load += new System.EventHandler(this.ZServerPropertyForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.tabPage_general.PerformLayout();
            this.tabPage_database.ResumeLayout(false);
            this.tabPage_database.PerformLayout();
            this.tabPage_accessControl.ResumeLayout(false);
            this.tabPage_accessControl.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage_search.ResumeLayout(false);
            this.tabPage_search.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabPage_charset.ResumeLayout(false);
            this.tabPage_charset.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage_unionCatalog.ResumeLayout(false);
            this.tabPage_unionCatalog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.TabPage tabPage_accessControl;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TabPage tabPage_search;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_serverName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_serverAddr;
        private System.Windows.Forms.TextBox textBox_serverPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_initializeInfomation;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_authenStyeOpen;
        private System.Windows.Forms.RadioButton radioButton_authenStyleIdpass;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_groupID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox_alwaysUseFullElementSet;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_presentPerCount;
        private System.Windows.Forms.CheckBox checkBox_autoDetectMarcSyntax;
        private System.Windows.Forms.TabPage tabPage_database;
        private System.Windows.Forms.TextBox textBox_databaseNames;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabPage tabPage_charset;
        private System.Windows.Forms.CheckBox checkBox_autoDetectEACC;
        private System.Windows.Forms.ComboBox comboBox_defaultEncoding;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TabPage tabPage_MARC;
        private System.Windows.Forms.ComboBox comboBox_defaultMarcSyntaxOID;
        private System.Windows.Forms.ComboBox comboBox_queryTermEncoding;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox comboBox_defaultElementSetName;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBox_homepage;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button button_gotoHomepage;
        private System.Windows.Forms.Button button_deleteBindingItem;
        private System.Windows.Forms.Button button_newBindingItem;
        private System.Windows.Forms.Button button_modifyBindingItem;
        private System.Windows.Forms.ListView listView_recordSyntaxAndEncodingBinding;
        private System.Windows.Forms.ColumnHeader columnHeader_recordSyntax;
        private System.Windows.Forms.ColumnHeader columnHeader_encoding;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_charNegoUTF8;
        private System.Windows.Forms.CheckBox checkBox_charNegoRecordsInSelectedCharSets;
        private System.Windows.Forms.TabPage tabPage_unionCatalog;
        private System.Windows.Forms.TextBox textBox_unionCatalog_bindingDp2ServerName;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button button_unionCatalog_findDp2Server;
        private System.Windows.Forms.TextBox textBox_unionCatalog_bindingUcServerUrl;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.CheckBox checkBox_ignoreRerenceID;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textBox_notInAllDatabaseNames;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBox_isbn_removeHyphen;
        private System.Windows.Forms.CheckBox checkBox_isbn_addHyphen;
        private System.Windows.Forms.CheckBox checkBox_isbn_forceIsbn10;
        private System.Windows.Forms.CheckBox checkBox_isbn_forceIsbn13;
        private System.Windows.Forms.CheckBox checkBox_isbn_wild;
        private System.Windows.Forms.CheckBox checkBox_forceIssn8;
    }
}