namespace dp2Circulation
{
    partial class UserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserForm));
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.listView_users = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_rights = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_changed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_access = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_binding = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel_userEdit = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_userType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_userRights = new DigitalPlatform.CommonControl.AutoHeightTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.button_editUserRights = new System.Windows.Forms.Button();
            this.textBox_confirmPassword = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_changePassword = new System.Windows.Forms.CheckBox();
            this.button_resetPassword = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_access = new DigitalPlatform.CommonControl.AutoHeightTextBox();
            this.checkedComboBox_libraryCode = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_comment = new DigitalPlatform.CommonControl.AutoHeightTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_binding = new DigitalPlatform.CommonControl.AutoHeightTextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_listAllUsers = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_create = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton_privateUserName = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripButton_setPivateUserName = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clearPrivateUserName = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_freeAllChannels = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_userEdit.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(17, 18);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.listView_users);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_userEdit);
            this.splitContainer_main.Size = new System.Drawing.Size(829, 494);
            this.splitContainer_main.SplitterDistance = 211;
            this.splitContainer_main.SplitterWidth = 14;
            this.splitContainer_main.TabIndex = 0;
            // 
            // listView_users
            // 
            this.listView_users.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_libraryCode,
            this.columnHeader_userName,
            this.columnHeader_type,
            this.columnHeader_rights,
            this.columnHeader_changed,
            this.columnHeader_access,
            this.columnHeader_binding,
            this.columnHeader_comment});
            this.listView_users.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_users.FullRowSelect = true;
            this.listView_users.HideSelection = false;
            this.listView_users.Location = new System.Drawing.Point(0, 0);
            this.listView_users.Margin = new System.Windows.Forms.Padding(4);
            this.listView_users.Name = "listView_users";
            this.listView_users.Size = new System.Drawing.Size(829, 211);
            this.listView_users.TabIndex = 0;
            this.listView_users.UseCompatibleStateImageBehavior = false;
            this.listView_users.View = System.Windows.Forms.View.Details;
            this.listView_users.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_users_ColumnClick);
            this.listView_users.SelectedIndexChanged += new System.EventHandler(this.listView_users_SelectedIndexChanged);
            // 
            // columnHeader_libraryCode
            // 
            this.columnHeader_libraryCode.Text = "图书馆代码";
            this.columnHeader_libraryCode.Width = 150;
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 100;
            // 
            // columnHeader_type
            // 
            this.columnHeader_type.Text = "类型";
            this.columnHeader_type.Width = 100;
            // 
            // columnHeader_rights
            // 
            this.columnHeader_rights.Text = "权限";
            this.columnHeader_rights.Width = 300;
            // 
            // columnHeader_changed
            // 
            this.columnHeader_changed.Text = "是否修改过";
            this.columnHeader_changed.Width = 100;
            // 
            // columnHeader_access
            // 
            this.columnHeader_access.Text = "存取定义";
            this.columnHeader_access.Width = 200;
            // 
            // columnHeader_binding
            // 
            this.columnHeader_binding.Text = "绑定";
            this.columnHeader_binding.Width = 80;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 200;
            // 
            // tableLayoutPanel_userEdit
            // 
            this.tableLayoutPanel_userEdit.AutoScroll = true;
            this.tableLayoutPanel_userEdit.AutoSize = true;
            this.tableLayoutPanel_userEdit.BackColor = System.Drawing.Color.AliceBlue;
            this.tableLayoutPanel_userEdit.ColumnCount = 3;
            this.tableLayoutPanel_userEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_userEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_userEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_userEdit.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_userName, 1, 0);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_userType, 1, 1);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_userRights, 1, 3);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label4, 0, 8);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_password, 1, 8);
            this.tableLayoutPanel_userEdit.Controls.Add(this.button_editUserRights, 2, 3);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_confirmPassword, 1, 9);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label5, 0, 9);
            this.tableLayoutPanel_userEdit.Controls.Add(this.checkBox_changePassword, 0, 7);
            this.tableLayoutPanel_userEdit.Controls.Add(this.button_resetPassword, 2, 9);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label6, 0, 4);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label7, 0, 5);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_access, 1, 5);
            this.tableLayoutPanel_userEdit.Controls.Add(this.checkedComboBox_libraryCode, 1, 4);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label8, 0, 2);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_comment, 1, 2);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label9, 0, 6);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_binding, 1, 6);
            this.tableLayoutPanel_userEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_userEdit.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_userEdit.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_userEdit.Name = "tableLayoutPanel_userEdit";
            this.tableLayoutPanel_userEdit.RowCount = 11;
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.Size = new System.Drawing.Size(829, 269);
            this.tableLayoutPanel_userEdit.TabIndex = 0;
            this.tableLayoutPanel_userEdit.SizeChanged += new System.EventHandler(this.tableLayoutPanel_userEdit_SizeChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名(&N):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_userName
            // 
            this.textBox_userName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_userName.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_userName.Location = new System.Drawing.Point(192, 4);
            this.textBox_userName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_userName.MinimumSize = new System.Drawing.Size(136, 4);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(267, 31);
            this.textBox_userName.TabIndex = 1;
            this.textBox_userName.TextChanged += new System.EventHandler(this.textBox_userName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(4, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 39);
            this.label2.TabIndex = 2;
            this.label2.Text = "类型(&T):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_userType
            // 
            this.textBox_userType.Location = new System.Drawing.Point(192, 43);
            this.textBox_userType.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_userType.Name = "textBox_userType";
            this.textBox_userType.Size = new System.Drawing.Size(145, 31);
            this.textBox_userType.TabIndex = 3;
            this.textBox_userType.TextChanged += new System.EventHandler(this.textBox_userType_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(4, 113);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 44);
            this.label3.TabIndex = 4;
            this.label3.Text = "权限(&R):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_userRights
            // 
            this.textBox_userRights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_userRights.Location = new System.Drawing.Point(192, 117);
            this.textBox_userRights.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_userRights.MinimumSize = new System.Drawing.Size(66, 4);
            this.textBox_userRights.Multiline = true;
            this.textBox_userRights.Name = "textBox_userRights";
            this.textBox_userRights.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_userRights.Size = new System.Drawing.Size(267, 36);
            this.textBox_userRights.TabIndex = 5;
            this.textBox_userRights.TextChanged += new System.EventHandler(this.textBox_userRights_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(4, 300);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 39);
            this.label4.TabIndex = 7;
            this.label4.Text = "密码(&P):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_password
            // 
            this.textBox_password.Enabled = false;
            this.textBox_password.Location = new System.Drawing.Point(192, 304);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(145, 31);
            this.textBox_password.TabIndex = 8;
            this.textBox_password.TextChanged += new System.EventHandler(this.textBox_password_TextChanged);
            // 
            // button_editUserRights
            // 
            this.button_editUserRights.AutoSize = true;
            this.button_editUserRights.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_editUserRights.Font = new System.Drawing.Font("宋体", 5F);
            this.button_editUserRights.Location = new System.Drawing.Point(467, 117);
            this.button_editUserRights.Margin = new System.Windows.Forms.Padding(4);
            this.button_editUserRights.Name = "button_editUserRights";
            this.button_editUserRights.Size = new System.Drawing.Size(45, 22);
            this.button_editUserRights.TabIndex = 6;
            this.button_editUserRights.Text = ". . .";
            this.button_editUserRights.UseVisualStyleBackColor = true;
            this.button_editUserRights.Click += new System.EventHandler(this.button_editUserRights_Click);
            // 
            // textBox_confirmPassword
            // 
            this.textBox_confirmPassword.Enabled = false;
            this.textBox_confirmPassword.Location = new System.Drawing.Point(192, 343);
            this.textBox_confirmPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_confirmPassword.Name = "textBox_confirmPassword";
            this.textBox_confirmPassword.PasswordChar = '*';
            this.textBox_confirmPassword.Size = new System.Drawing.Size(145, 31);
            this.textBox_confirmPassword.TabIndex = 11;
            this.textBox_confirmPassword.TextChanged += new System.EventHandler(this.textBox_confirmPassword_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(4, 339);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(180, 62);
            this.label5.TabIndex = 10;
            this.label5.Text = "再次输入密码(&C):";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBox_changePassword
            // 
            this.checkBox_changePassword.AutoSize = true;
            this.checkBox_changePassword.Location = new System.Drawing.Point(4, 271);
            this.checkBox_changePassword.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_changePassword.Name = "checkBox_changePassword";
            this.checkBox_changePassword.Size = new System.Drawing.Size(120, 25);
            this.checkBox_changePassword.TabIndex = 12;
            this.checkBox_changePassword.Text = "修改密码";
            this.checkBox_changePassword.UseVisualStyleBackColor = true;
            this.checkBox_changePassword.CheckedChanged += new System.EventHandler(this.checkBox_changePassword_CheckedChanged);
            // 
            // button_resetPassword
            // 
            this.button_resetPassword.AutoSize = true;
            this.button_resetPassword.Enabled = false;
            this.button_resetPassword.Location = new System.Drawing.Point(467, 343);
            this.button_resetPassword.Margin = new System.Windows.Forms.Padding(4);
            this.button_resetPassword.Name = "button_resetPassword";
            this.button_resetPassword.Size = new System.Drawing.Size(328, 54);
            this.button_resetPassword.TabIndex = 9;
            this.button_resetPassword.Text = "立即重设密码(&R)";
            this.button_resetPassword.UseVisualStyleBackColor = true;
            this.button_resetPassword.Click += new System.EventHandler(this.button_resetPassword_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(4, 157);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(180, 40);
            this.label6.TabIndex = 13;
            this.label6.Text = "图书馆代码(&C):";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(4, 197);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(180, 35);
            this.label7.TabIndex = 15;
            this.label7.Text = "存取定义(&A):";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_access
            // 
            this.textBox_access.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_access.Location = new System.Drawing.Point(192, 201);
            this.textBox_access.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_access.MinimumSize = new System.Drawing.Size(66, 4);
            this.textBox_access.Multiline = true;
            this.textBox_access.Name = "textBox_access";
            this.textBox_access.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_access.Size = new System.Drawing.Size(267, 27);
            this.textBox_access.TabIndex = 16;
            this.textBox_access.TextChanged += new System.EventHandler(this.textBox_access_TextChanged);
            // 
            // checkedComboBox_libraryCode
            // 
            this.checkedComboBox_libraryCode.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_libraryCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedComboBox_libraryCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_libraryCode.Location = new System.Drawing.Point(192, 161);
            this.checkedComboBox_libraryCode.Margin = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_libraryCode.Name = "checkedComboBox_libraryCode";
            this.checkedComboBox_libraryCode.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_libraryCode.ReadOnly = false;
            this.checkedComboBox_libraryCode.Size = new System.Drawing.Size(267, 32);
            this.checkedComboBox_libraryCode.TabIndex = 17;
            this.checkedComboBox_libraryCode.DropDown += new System.EventHandler(this.checkedComboBox_libraryCode_DropDown);
            this.checkedComboBox_libraryCode.TextChanged += new System.EventHandler(this.checkedComboBox_libraryCode_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(4, 78);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(180, 35);
            this.label8.TabIndex = 18;
            this.label8.Text = "注释(&C):";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_comment
            // 
            this.textBox_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_comment.Location = new System.Drawing.Point(192, 82);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_comment.MinimumSize = new System.Drawing.Size(66, 4);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(267, 27);
            this.textBox_comment.TabIndex = 19;
            this.textBox_comment.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(4, 232);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(180, 35);
            this.label9.TabIndex = 20;
            this.label9.Text = "绑定(&B):";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_binding
            // 
            this.textBox_binding.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_binding.Location = new System.Drawing.Point(192, 236);
            this.textBox_binding.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_binding.MinimumSize = new System.Drawing.Size(66, 4);
            this.textBox_binding.Multiline = true;
            this.textBox_binding.Name = "textBox_binding";
            this.textBox_binding.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_binding.Size = new System.Drawing.Size(267, 27);
            this.textBox_binding.TabIndex = 21;
            this.textBox_binding.TextChanged += new System.EventHandler(this.textBox_binding_TextChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_listAllUsers,
            this.toolStripButton_save,
            this.toolStripButton_create,
            this.toolStripButton_delete,
            this.toolStripSeparator1,
            this.toolStripDropDownButton_privateUserName,
            this.toolStripButton_freeAllChannels});
            this.toolStrip1.Location = new System.Drawing.Point(17, 516);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(829, 45);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_listAllUsers
            // 
            this.toolStripButton_listAllUsers.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_listAllUsers.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_listAllUsers.Image")));
            this.toolStripButton_listAllUsers.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_listAllUsers.Name = "toolStripButton_listAllUsers";
            this.toolStripButton_listAllUsers.Size = new System.Drawing.Size(142, 39);
            this.toolStripButton_listAllUsers.Text = "列出全部用户";
            this.toolStripButton_listAllUsers.Click += new System.EventHandler(this.toolStripButton_listAllUsers_Click);
            // 
            // toolStripButton_save
            // 
            this.toolStripButton_save.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_save.Image")));
            this.toolStripButton_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_save.Name = "toolStripButton_save";
            this.toolStripButton_save.Size = new System.Drawing.Size(58, 39);
            this.toolStripButton_save.Text = "保存";
            this.toolStripButton_save.Click += new System.EventHandler(this.toolStripButton_save_Click);
            // 
            // toolStripButton_create
            // 
            this.toolStripButton_create.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_create.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_create.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_create.Image")));
            this.toolStripButton_create.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_create.Name = "toolStripButton_create";
            this.toolStripButton_create.Size = new System.Drawing.Size(121, 39);
            this.toolStripButton_create.Text = "创建新用户";
            this.toolStripButton_create.Click += new System.EventHandler(this.toolStripButton_create_Click);
            // 
            // toolStripButton_delete
            // 
            this.toolStripButton_delete.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_delete.Image")));
            this.toolStripButton_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_delete.Name = "toolStripButton_delete";
            this.toolStripButton_delete.Size = new System.Drawing.Size(58, 39);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 45);
            // 
            // toolStripDropDownButton_privateUserName
            // 
            this.toolStripDropDownButton_privateUserName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_privateUserName.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_setPivateUserName,
            this.toolStripButton_clearPrivateUserName});
            this.toolStripDropDownButton_privateUserName.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_privateUserName.Image")));
            this.toolStripDropDownButton_privateUserName.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_privateUserName.Name = "toolStripDropDownButton_privateUserName";
            this.toolStripDropDownButton_privateUserName.Size = new System.Drawing.Size(117, 39);
            this.toolStripDropDownButton_privateUserName.Text = "登录账户";
            // 
            // toolStripButton_setPivateUserName
            // 
            this.toolStripButton_setPivateUserName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_setPivateUserName.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_setPivateUserName.Image")));
            this.toolStripButton_setPivateUserName.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_setPivateUserName.Name = "toolStripButton_setPivateUserName";
            this.toolStripButton_setPivateUserName.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_setPivateUserName.Text = "指定登录账户";
            this.toolStripButton_setPivateUserName.Click += new System.EventHandler(this.toolStripButton_setPivateUserName_Click);
            // 
            // toolStripButton_clearPrivateUserName
            // 
            this.toolStripButton_clearPrivateUserName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clearPrivateUserName.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearPrivateUserName.Image")));
            this.toolStripButton_clearPrivateUserName.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearPrivateUserName.Name = "toolStripButton_clearPrivateUserName";
            this.toolStripButton_clearPrivateUserName.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_clearPrivateUserName.Text = "清除登录账户";
            this.toolStripButton_clearPrivateUserName.Click += new System.EventHandler(this.toolStripButton_clearPrivateUserName_Click);
            // 
            // toolStripButton_freeAllChannels
            // 
            this.toolStripButton_freeAllChannels.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_freeAllChannels.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_freeAllChannels.Image")));
            this.toolStripButton_freeAllChannels.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_freeAllChannels.Name = "toolStripButton_freeAllChannels";
            this.toolStripButton_freeAllChannels.Size = new System.Drawing.Size(142, 39);
            this.toolStripButton_freeAllChannels.Text = "释放所有通道";
            this.toolStripButton_freeAllChannels.Click += new System.EventHandler(this.toolStripButton_freeAllChannels_Click);
            // 
            // UserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 570);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UserForm";
            this.ShowInTaskbar = false;
            this.Text = "用户窗";
            this.Activated += new System.EventHandler(this.UserForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UserForm_FormClosed);
            this.Load += new System.EventHandler(this.UserForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_userEdit.ResumeLayout(false);
            this.tableLayoutPanel_userEdit.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private DigitalPlatform.GUI.ListViewNF listView_users;
        private System.Windows.Forms.ColumnHeader columnHeader_userName;
        private System.Windows.Forms.ColumnHeader columnHeader_type;
        private System.Windows.Forms.ColumnHeader columnHeader_rights;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_userEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_userType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Button button_editUserRights;
        private System.Windows.Forms.Button button_resetPassword;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_confirmPassword;
        private System.Windows.Forms.ColumnHeader columnHeader_changed;
        private System.Windows.Forms.CheckBox checkBox_changePassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryCode;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ColumnHeader columnHeader_access;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_libraryCode;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private DigitalPlatform.CommonControl.AutoHeightTextBox textBox_comment;
        private DigitalPlatform.CommonControl.AutoHeightTextBox textBox_userRights;
        private DigitalPlatform.CommonControl.AutoHeightTextBox textBox_access;
        private System.Windows.Forms.Label label9;
        private DigitalPlatform.CommonControl.AutoHeightTextBox textBox_binding;
        private System.Windows.Forms.ColumnHeader columnHeader_binding;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_listAllUsers;
        private System.Windows.Forms.ToolStripButton toolStripButton_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_create;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_privateUserName;
        private System.Windows.Forms.ToolStripButton toolStripButton_setPivateUserName;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearPrivateUserName;
        private System.Windows.Forms.ToolStripButton toolStripButton_freeAllChannels;
    }
}