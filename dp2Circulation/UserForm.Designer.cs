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
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel_userEdit = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_userType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_userRights = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.button_editUserRights = new System.Windows.Forms.Button();
            this.textBox_confirmPassword = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_changePassword = new System.Windows.Forms.CheckBox();
            this.button_resetPassword = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_access = new System.Windows.Forms.TextBox();
            this.checkedComboBox_libraryCode = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.button_save = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.button_create = new System.Windows.Forms.Button();
            this.button_listAllUsers = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_userEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(9, 10);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2);
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
            this.splitContainer_main.Size = new System.Drawing.Size(452, 270);
            this.splitContainer_main.SplitterDistance = 116;
            this.splitContainer_main.SplitterWidth = 8;
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
            this.columnHeader_comment});
            this.listView_users.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_users.FullRowSelect = true;
            this.listView_users.HideSelection = false;
            this.listView_users.Location = new System.Drawing.Point(0, 0);
            this.listView_users.Margin = new System.Windows.Forms.Padding(2);
            this.listView_users.Name = "listView_users";
            this.listView_users.Size = new System.Drawing.Size(452, 116);
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
            this.tableLayoutPanel_userEdit.Controls.Add(this.label4, 0, 7);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_password, 1, 7);
            this.tableLayoutPanel_userEdit.Controls.Add(this.button_editUserRights, 2, 3);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_confirmPassword, 1, 8);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label5, 0, 8);
            this.tableLayoutPanel_userEdit.Controls.Add(this.checkBox_changePassword, 0, 6);
            this.tableLayoutPanel_userEdit.Controls.Add(this.button_resetPassword, 2, 8);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label6, 0, 4);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label7, 0, 5);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_access, 1, 5);
            this.tableLayoutPanel_userEdit.Controls.Add(this.checkedComboBox_libraryCode, 1, 4);
            this.tableLayoutPanel_userEdit.Controls.Add(this.label8, 0, 2);
            this.tableLayoutPanel_userEdit.Controls.Add(this.textBox_comment, 1, 2);
            this.tableLayoutPanel_userEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_userEdit.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_userEdit.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_userEdit.Name = "tableLayoutPanel_userEdit";
            this.tableLayoutPanel_userEdit.RowCount = 10;
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_userEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel_userEdit.Size = new System.Drawing.Size(452, 146);
            this.tableLayoutPanel_userEdit.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名(&N):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_userName
            // 
            this.textBox_userName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_userName.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_userName.Location = new System.Drawing.Point(107, 2);
            this.textBox_userName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_userName.MinimumSize = new System.Drawing.Size(76, 4);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(217, 21);
            this.textBox_userName.TabIndex = 1;
            this.textBox_userName.TextChanged += new System.EventHandler(this.textBox_userName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(2, 25);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "类型(&T):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_userType
            // 
            this.textBox_userType.Location = new System.Drawing.Point(107, 27);
            this.textBox_userType.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_userType.Name = "textBox_userType";
            this.textBox_userType.Size = new System.Drawing.Size(146, 21);
            this.textBox_userType.TabIndex = 3;
            this.textBox_userType.TextChanged += new System.EventHandler(this.textBox_userType_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 100);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 76);
            this.label3.TabIndex = 4;
            this.label3.Text = "权限(&R):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_userRights
            // 
            this.textBox_userRights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_userRights.Location = new System.Drawing.Point(107, 102);
            this.textBox_userRights.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_userRights.MinimumSize = new System.Drawing.Size(38, 41);
            this.textBox_userRights.Multiline = true;
            this.textBox_userRights.Name = "textBox_userRights";
            this.textBox_userRights.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_userRights.Size = new System.Drawing.Size(217, 72);
            this.textBox_userRights.TabIndex = 5;
            this.textBox_userRights.TextChanged += new System.EventHandler(this.textBox_userRights_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(2, 302);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 25);
            this.label4.TabIndex = 7;
            this.label4.Text = "密码(&P):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_password
            // 
            this.textBox_password.Enabled = false;
            this.textBox_password.Location = new System.Drawing.Point(107, 304);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(146, 21);
            this.textBox_password.TabIndex = 8;
            this.textBox_password.TextChanged += new System.EventHandler(this.textBox_password_TextChanged);
            // 
            // button_editUserRights
            // 
            this.button_editUserRights.Location = new System.Drawing.Point(328, 102);
            this.button_editUserRights.Margin = new System.Windows.Forms.Padding(2);
            this.button_editUserRights.Name = "button_editUserRights";
            this.button_editUserRights.Size = new System.Drawing.Size(33, 22);
            this.button_editUserRights.TabIndex = 6;
            this.button_editUserRights.Text = "...";
            this.button_editUserRights.UseVisualStyleBackColor = true;
            this.button_editUserRights.Click += new System.EventHandler(this.button_editUserRights_Click);
            // 
            // textBox_confirmPassword
            // 
            this.textBox_confirmPassword.Enabled = false;
            this.textBox_confirmPassword.Location = new System.Drawing.Point(107, 329);
            this.textBox_confirmPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_confirmPassword.Name = "textBox_confirmPassword";
            this.textBox_confirmPassword.PasswordChar = '*';
            this.textBox_confirmPassword.Size = new System.Drawing.Size(146, 21);
            this.textBox_confirmPassword.TabIndex = 11;
            this.textBox_confirmPassword.TextChanged += new System.EventHandler(this.textBox_confirmPassword_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(2, 327);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 26);
            this.label5.TabIndex = 10;
            this.label5.Text = "再次输入密码(&C):";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBox_changePassword
            // 
            this.checkBox_changePassword.AutoSize = true;
            this.checkBox_changePassword.Location = new System.Drawing.Point(2, 284);
            this.checkBox_changePassword.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_changePassword.Name = "checkBox_changePassword";
            this.checkBox_changePassword.Size = new System.Drawing.Size(72, 16);
            this.checkBox_changePassword.TabIndex = 12;
            this.checkBox_changePassword.Text = "修改密码";
            this.checkBox_changePassword.UseVisualStyleBackColor = true;
            this.checkBox_changePassword.CheckedChanged += new System.EventHandler(this.checkBox_changePassword_CheckedChanged);
            // 
            // button_resetPassword
            // 
            this.button_resetPassword.AutoSize = true;
            this.button_resetPassword.Enabled = false;
            this.button_resetPassword.Location = new System.Drawing.Point(328, 329);
            this.button_resetPassword.Margin = new System.Windows.Forms.Padding(2);
            this.button_resetPassword.Name = "button_resetPassword";
            this.button_resetPassword.Size = new System.Drawing.Size(105, 22);
            this.button_resetPassword.TabIndex = 9;
            this.button_resetPassword.Text = "立即重设密码(&R)";
            this.button_resetPassword.UseVisualStyleBackColor = true;
            this.button_resetPassword.Click += new System.EventHandler(this.button_resetPassword_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(2, 176);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 30);
            this.label6.TabIndex = 13;
            this.label6.Text = "图书馆代码(&C):";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(2, 206);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 76);
            this.label7.TabIndex = 15;
            this.label7.Text = "存取定义(&A):";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_access
            // 
            this.textBox_access.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_access.Location = new System.Drawing.Point(107, 208);
            this.textBox_access.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_access.MinimumSize = new System.Drawing.Size(38, 41);
            this.textBox_access.Multiline = true;
            this.textBox_access.Name = "textBox_access";
            this.textBox_access.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_access.Size = new System.Drawing.Size(217, 72);
            this.textBox_access.TabIndex = 16;
            this.textBox_access.TextChanged += new System.EventHandler(this.textBox_access_TextChanged);
            // 
            // checkedComboBox_libraryCode
            // 
            this.checkedComboBox_libraryCode.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_libraryCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedComboBox_libraryCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_libraryCode.Location = new System.Drawing.Point(109, 180);
            this.checkedComboBox_libraryCode.Margin = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_libraryCode.Name = "checkedComboBox_libraryCode";
            this.checkedComboBox_libraryCode.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_libraryCode.Size = new System.Drawing.Size(213, 22);
            this.checkedComboBox_libraryCode.TabIndex = 17;
            this.checkedComboBox_libraryCode.DropDown += new System.EventHandler(this.checkedComboBox_libraryCode_DropDown);
            this.checkedComboBox_libraryCode.TextChanged += new System.EventHandler(this.checkedComboBox_libraryCode_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(2, 50);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(101, 50);
            this.label8.TabIndex = 18;
            this.label8.Text = "注释(&C):";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_comment
            // 
            this.textBox_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_comment.Location = new System.Drawing.Point(107, 52);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_comment.MinimumSize = new System.Drawing.Size(38, 41);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(217, 46);
            this.textBox_comment.TabIndex = 19;
            this.textBox_comment.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // button_save
            // 
            this.button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_save.Location = new System.Drawing.Point(405, 294);
            this.button_save.Margin = new System.Windows.Forms.Padding(2);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(56, 22);
            this.button_save.TabIndex = 4;
            this.button_save.Text = "保存(&S)";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_delete.Location = new System.Drawing.Point(228, 294);
            this.button_delete.Margin = new System.Windows.Forms.Padding(2);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(56, 22);
            this.button_delete.TabIndex = 2;
            this.button_delete.Text = "删除(&D)";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // button_create
            // 
            this.button_create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_create.Location = new System.Drawing.Point(289, 294);
            this.button_create.Margin = new System.Windows.Forms.Padding(2);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(98, 22);
            this.button_create.TabIndex = 3;
            this.button_create.Text = "创建新用户(&C)";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // button_listAllUsers
            // 
            this.button_listAllUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_listAllUsers.Location = new System.Drawing.Point(9, 294);
            this.button_listAllUsers.Margin = new System.Windows.Forms.Padding(2);
            this.button_listAllUsers.Name = "button_listAllUsers";
            this.button_listAllUsers.Size = new System.Drawing.Size(116, 22);
            this.button_listAllUsers.TabIndex = 1;
            this.button_listAllUsers.Text = "列出全部用户(&L)";
            this.button_listAllUsers.UseVisualStyleBackColor = true;
            this.button_listAllUsers.Click += new System.EventHandler(this.button_listAllUsers_Click);
            // 
            // UserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 326);
            this.Controls.Add(this.button_listAllUsers);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Button button_create;
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
        private System.Windows.Forms.TextBox textBox_userRights;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Button button_editUserRights;
        private System.Windows.Forms.Button button_listAllUsers;
        private System.Windows.Forms.Button button_resetPassword;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_confirmPassword;
        private System.Windows.Forms.ColumnHeader columnHeader_changed;
        private System.Windows.Forms.CheckBox checkBox_changePassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryCode;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_access;
        private System.Windows.Forms.ColumnHeader columnHeader_access;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_libraryCode;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
    }
}