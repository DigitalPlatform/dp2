namespace dp2Circulation
{
    partial class ChangePasswordForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangePasswordForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_reader_barcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_reader_oldPassword = new System.Windows.Forms.TextBox();
            this.textBox_reader_newPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_reader_confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_reader_changePassword = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_reader = new System.Windows.Forms.TabPage();
            this.textBox_reader_comment = new System.Windows.Forms.TextBox();
            this.tabPage_worker = new System.Windows.Forms.TabPage();
            this.textBox_worker_comment = new System.Windows.Forms.TextBox();
            this.checkBox_worker_force = new System.Windows.Forms.CheckBox();
            this.button_worker_changePassword = new System.Windows.Forms.Button();
            this.textBox_worker_confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_worker_oldPassword = new System.Windows.Forms.TextBox();
            this.textBox_worker_newPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_worker_userName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage_resetPatronPassword = new System.Windows.Forms.TabPage();
            this.label1_resetPatronPassword_queryword = new System.Windows.Forms.Label();
            this.textBox_resetPatronPassword_queryWord = new System.Windows.Forms.TextBox();
            this.button_resetPatronPassword_displayTempPassword = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button_resetPatronPassword = new System.Windows.Forms.Button();
            this.textBox_resetPatronPassword_barcode = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_resetPatronPassword_name = new System.Windows.Forms.TextBox();
            this.textBox_resetPatronPassword_phoneNumber = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_reader.SuspendLayout();
            this.tabPage_worker.SuspendLayout();
            this.tabPage_resetPatronPassword.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 147);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号(&B):";
            // 
            // textBox_reader_barcode
            // 
            this.textBox_reader_barcode.Location = new System.Drawing.Point(207, 142);
            this.textBox_reader_barcode.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_reader_barcode.Name = "textBox_reader_barcode";
            this.textBox_reader_barcode.Size = new System.Drawing.Size(290, 31);
            this.textBox_reader_barcode.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 203);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "旧密码(&O):";
            // 
            // textBox_reader_oldPassword
            // 
            this.textBox_reader_oldPassword.Location = new System.Drawing.Point(207, 198);
            this.textBox_reader_oldPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_reader_oldPassword.Name = "textBox_reader_oldPassword";
            this.textBox_reader_oldPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_reader_oldPassword.TabIndex = 3;
            this.textBox_reader_oldPassword.UseSystemPasswordChar = true;
            // 
            // textBox_reader_newPassword
            // 
            this.textBox_reader_newPassword.Location = new System.Drawing.Point(207, 254);
            this.textBox_reader_newPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_reader_newPassword.Name = "textBox_reader_newPassword";
            this.textBox_reader_newPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_reader_newPassword.TabIndex = 5;
            this.textBox_reader_newPassword.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 259);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "新密码(&N):";
            // 
            // textBox_reader_confirmNewPassword
            // 
            this.textBox_reader_confirmNewPassword.Location = new System.Drawing.Point(207, 301);
            this.textBox_reader_confirmNewPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_reader_confirmNewPassword.Name = "textBox_reader_confirmNewPassword";
            this.textBox_reader_confirmNewPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_reader_confirmNewPassword.TabIndex = 7;
            this.textBox_reader_confirmNewPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 306);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(159, 21);
            this.label4.TabIndex = 6;
            this.label4.Text = "确认新密码(&C):";
            // 
            // button_reader_changePassword
            // 
            this.button_reader_changePassword.Location = new System.Drawing.Point(207, 348);
            this.button_reader_changePassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_reader_changePassword.Name = "button_reader_changePassword";
            this.button_reader_changePassword.Size = new System.Drawing.Size(176, 40);
            this.button_reader_changePassword.TabIndex = 8;
            this.button_reader_changePassword.Text = "修改密码(&C)";
            this.button_reader_changePassword.UseVisualStyleBackColor = true;
            this.button_reader_changePassword.Click += new System.EventHandler(this.button_reader_changePassword_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_reader);
            this.tabControl_main.Controls.Add(this.tabPage_worker);
            this.tabControl_main.Controls.Add(this.tabPage_resetPatronPassword);
            this.tabControl_main.Location = new System.Drawing.Point(0, 18);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(684, 495);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_reader
            // 
            this.tabPage_reader.AutoScroll = true;
            this.tabPage_reader.Controls.Add(this.textBox_reader_comment);
            this.tabPage_reader.Controls.Add(this.label1);
            this.tabPage_reader.Controls.Add(this.button_reader_changePassword);
            this.tabPage_reader.Controls.Add(this.textBox_reader_barcode);
            this.tabPage_reader.Controls.Add(this.textBox_reader_confirmNewPassword);
            this.tabPage_reader.Controls.Add(this.label2);
            this.tabPage_reader.Controls.Add(this.label4);
            this.tabPage_reader.Controls.Add(this.textBox_reader_oldPassword);
            this.tabPage_reader.Controls.Add(this.textBox_reader_newPassword);
            this.tabPage_reader.Controls.Add(this.label3);
            this.tabPage_reader.Location = new System.Drawing.Point(4, 31);
            this.tabPage_reader.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_reader.Name = "tabPage_reader";
            this.tabPage_reader.Padding = new System.Windows.Forms.Padding(22, 21, 22, 21);
            this.tabPage_reader.Size = new System.Drawing.Size(676, 460);
            this.tabPage_reader.TabIndex = 0;
            this.tabPage_reader.Text = "读者";
            this.tabPage_reader.UseVisualStyleBackColor = true;
            // 
            // textBox_reader_comment
            // 
            this.textBox_reader_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_reader_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_reader_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_reader_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_reader_comment.Location = new System.Drawing.Point(33, 26);
            this.textBox_reader_comment.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_reader_comment.Multiline = true;
            this.textBox_reader_comment.Name = "textBox_reader_comment";
            this.textBox_reader_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_reader_comment.Size = new System.Drawing.Size(609, 100);
            this.textBox_reader_comment.TabIndex = 9;
            this.textBox_reader_comment.Text = "这是工作人员为读者强制修改密码。\r\n\r\n使用本功能前，请务必仔细核实读者身份。";
            // 
            // tabPage_worker
            // 
            this.tabPage_worker.AutoScroll = true;
            this.tabPage_worker.Controls.Add(this.textBox_worker_comment);
            this.tabPage_worker.Controls.Add(this.checkBox_worker_force);
            this.tabPage_worker.Controls.Add(this.button_worker_changePassword);
            this.tabPage_worker.Controls.Add(this.textBox_worker_confirmNewPassword);
            this.tabPage_worker.Controls.Add(this.label6);
            this.tabPage_worker.Controls.Add(this.label7);
            this.tabPage_worker.Controls.Add(this.textBox_worker_oldPassword);
            this.tabPage_worker.Controls.Add(this.textBox_worker_newPassword);
            this.tabPage_worker.Controls.Add(this.label8);
            this.tabPage_worker.Controls.Add(this.textBox_worker_userName);
            this.tabPage_worker.Controls.Add(this.label5);
            this.tabPage_worker.Location = new System.Drawing.Point(4, 31);
            this.tabPage_worker.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_worker.Name = "tabPage_worker";
            this.tabPage_worker.Padding = new System.Windows.Forms.Padding(22, 21, 22, 21);
            this.tabPage_worker.Size = new System.Drawing.Size(676, 460);
            this.tabPage_worker.TabIndex = 1;
            this.tabPage_worker.Text = "工作人员";
            this.tabPage_worker.UseVisualStyleBackColor = true;
            // 
            // textBox_worker_comment
            // 
            this.textBox_worker_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_worker_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_worker_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_worker_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_worker_comment.Location = new System.Drawing.Point(33, 24);
            this.textBox_worker_comment.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_comment.Multiline = true;
            this.textBox_worker_comment.Name = "textBox_worker_comment";
            this.textBox_worker_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_worker_comment.Size = new System.Drawing.Size(609, 100);
            this.textBox_worker_comment.TabIndex = 10;
            this.textBox_worker_comment.Text = "这是工作人员为自己或者其他工作人员修改密码。\r\n";
            // 
            // checkBox_worker_force
            // 
            this.checkBox_worker_force.AutoSize = true;
            this.checkBox_worker_force.Location = new System.Drawing.Point(215, 340);
            this.checkBox_worker_force.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_worker_force.Name = "checkBox_worker_force";
            this.checkBox_worker_force.Size = new System.Drawing.Size(153, 25);
            this.checkBox_worker_force.TabIndex = 8;
            this.checkBox_worker_force.Text = "强制修改(&F)";
            this.checkBox_worker_force.UseVisualStyleBackColor = true;
            this.checkBox_worker_force.CheckedChanged += new System.EventHandler(this.checkBox_worker_force_CheckedChanged);
            // 
            // button_worker_changePassword
            // 
            this.button_worker_changePassword.Location = new System.Drawing.Point(215, 374);
            this.button_worker_changePassword.Margin = new System.Windows.Forms.Padding(4);
            this.button_worker_changePassword.Name = "button_worker_changePassword";
            this.button_worker_changePassword.Size = new System.Drawing.Size(176, 38);
            this.button_worker_changePassword.TabIndex = 9;
            this.button_worker_changePassword.Text = "修改密码(&C)";
            this.button_worker_changePassword.UseVisualStyleBackColor = true;
            this.button_worker_changePassword.Click += new System.EventHandler(this.button_worker_changePassword_Click);
            // 
            // textBox_worker_confirmNewPassword
            // 
            this.textBox_worker_confirmNewPassword.Location = new System.Drawing.Point(215, 294);
            this.textBox_worker_confirmNewPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_confirmNewPassword.Name = "textBox_worker_confirmNewPassword";
            this.textBox_worker_confirmNewPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_confirmNewPassword.TabIndex = 7;
            this.textBox_worker_confirmNewPassword.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(35, 196);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(117, 21);
            this.label6.TabIndex = 2;
            this.label6.Text = "旧密码(&O):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(35, 299);
            this.label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(159, 21);
            this.label7.TabIndex = 6;
            this.label7.Text = "确认新密码(&C):";
            // 
            // textBox_worker_oldPassword
            // 
            this.textBox_worker_oldPassword.Location = new System.Drawing.Point(215, 191);
            this.textBox_worker_oldPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_oldPassword.Name = "textBox_worker_oldPassword";
            this.textBox_worker_oldPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_oldPassword.TabIndex = 3;
            this.textBox_worker_oldPassword.UseSystemPasswordChar = true;
            // 
            // textBox_worker_newPassword
            // 
            this.textBox_worker_newPassword.Location = new System.Drawing.Point(215, 247);
            this.textBox_worker_newPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_newPassword.Name = "textBox_worker_newPassword";
            this.textBox_worker_newPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_newPassword.TabIndex = 5;
            this.textBox_worker_newPassword.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(35, 252);
            this.label8.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 21);
            this.label8.TabIndex = 4;
            this.label8.Text = "新密码(&N):";
            // 
            // textBox_worker_userName
            // 
            this.textBox_worker_userName.Location = new System.Drawing.Point(215, 133);
            this.textBox_worker_userName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_worker_userName.Name = "textBox_worker_userName";
            this.textBox_worker_userName.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_userName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 138);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 21);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&U):";
            // 
            // tabPage_resetPatronPassword
            // 
            this.tabPage_resetPatronPassword.AutoScroll = true;
            this.tabPage_resetPatronPassword.Controls.Add(this.label1_resetPatronPassword_queryword);
            this.tabPage_resetPatronPassword.Controls.Add(this.textBox_resetPatronPassword_queryWord);
            this.tabPage_resetPatronPassword.Controls.Add(this.button_resetPatronPassword_displayTempPassword);
            this.tabPage_resetPatronPassword.Controls.Add(this.textBox1);
            this.tabPage_resetPatronPassword.Controls.Add(this.label9);
            this.tabPage_resetPatronPassword.Controls.Add(this.button_resetPatronPassword);
            this.tabPage_resetPatronPassword.Controls.Add(this.textBox_resetPatronPassword_barcode);
            this.tabPage_resetPatronPassword.Controls.Add(this.label10);
            this.tabPage_resetPatronPassword.Controls.Add(this.textBox_resetPatronPassword_name);
            this.tabPage_resetPatronPassword.Controls.Add(this.textBox_resetPatronPassword_phoneNumber);
            this.tabPage_resetPatronPassword.Controls.Add(this.label12);
            this.tabPage_resetPatronPassword.Location = new System.Drawing.Point(4, 31);
            this.tabPage_resetPatronPassword.Name = "tabPage_resetPatronPassword";
            this.tabPage_resetPatronPassword.Size = new System.Drawing.Size(676, 460);
            this.tabPage_resetPatronPassword.TabIndex = 2;
            this.tabPage_resetPatronPassword.Text = "读者找回密码";
            this.tabPage_resetPatronPassword.UseVisualStyleBackColor = true;
            // 
            // label1_resetPatronPassword_queryword
            // 
            this.label1_resetPatronPassword_queryword.AutoSize = true;
            this.label1_resetPatronPassword_queryword.Location = new System.Drawing.Point(27, 423);
            this.label1_resetPatronPassword_queryword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1_resetPatronPassword_queryword.Name = "label1_resetPatronPassword_queryword";
            this.label1_resetPatronPassword_queryword.Size = new System.Drawing.Size(120, 21);
            this.label1_resetPatronPassword_queryword.TabIndex = 21;
            this.label1_resetPatronPassword_queryword.Text = "queryword=";
            this.label1_resetPatronPassword_queryword.Visible = false;
            // 
            // textBox_resetPatronPassword_queryWord
            // 
            this.textBox_resetPatronPassword_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_resetPatronPassword_queryWord.Location = new System.Drawing.Point(219, 420);
            this.textBox_resetPatronPassword_queryWord.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_resetPatronPassword_queryWord.Name = "textBox_resetPatronPassword_queryWord";
            this.textBox_resetPatronPassword_queryWord.Size = new System.Drawing.Size(428, 31);
            this.textBox_resetPatronPassword_queryWord.TabIndex = 22;
            this.textBox_resetPatronPassword_queryWord.Visible = false;
            // 
            // button_resetPatronPassword_displayTempPassword
            // 
            this.button_resetPatronPassword_displayTempPassword.Location = new System.Drawing.Point(219, 370);
            this.button_resetPatronPassword_displayTempPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_resetPatronPassword_displayTempPassword.Name = "button_resetPatronPassword_displayTempPassword";
            this.button_resetPatronPassword_displayTempPassword.Size = new System.Drawing.Size(428, 40);
            this.button_resetPatronPassword_displayTempPassword.TabIndex = 20;
            this.button_resetPatronPassword_displayTempPassword.Text = "获得临时密码[在本界面显示](&D)";
            this.button_resetPatronPassword_displayTempPassword.UseVisualStyleBackColor = true;
            this.button_resetPatronPassword_displayTempPassword.Click += new System.EventHandler(this.button_resetPatronPassword_displayTempPassword_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.SystemColors.Info;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox1.Location = new System.Drawing.Point(32, 33);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(609, 100);
            this.textBox1.TabIndex = 19;
            this.textBox1.Text = "这是为读者找回密码。\r\n\r\n临时密码可选择以手机短信发送到读者手机，或者直接从界面看到。";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(27, 154);
            this.label9.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(180, 21);
            this.label9.TabIndex = 10;
            this.label9.Text = "读者证条码号(&B):";
            // 
            // button_resetPatronPassword
            // 
            this.button_resetPatronPassword.Location = new System.Drawing.Point(219, 320);
            this.button_resetPatronPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_resetPatronPassword.Name = "button_resetPatronPassword";
            this.button_resetPatronPassword.Size = new System.Drawing.Size(428, 40);
            this.button_resetPatronPassword.TabIndex = 18;
            this.button_resetPatronPassword.Text = "获得临时密码[直接发送到读者手机](&R)";
            this.button_resetPatronPassword.UseVisualStyleBackColor = true;
            this.button_resetPatronPassword.Click += new System.EventHandler(this.button_resetPatronPassword_Click);
            // 
            // textBox_resetPatronPassword_barcode
            // 
            this.textBox_resetPatronPassword_barcode.Location = new System.Drawing.Point(219, 151);
            this.textBox_resetPatronPassword_barcode.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_resetPatronPassword_barcode.Name = "textBox_resetPatronPassword_barcode";
            this.textBox_resetPatronPassword_barcode.Size = new System.Drawing.Size(290, 31);
            this.textBox_resetPatronPassword_barcode.TabIndex = 11;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(27, 210);
            this.label10.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(96, 21);
            this.label10.TabIndex = 12;
            this.label10.Text = "姓名(&N):";
            // 
            // textBox_resetPatronPassword_name
            // 
            this.textBox_resetPatronPassword_name.Location = new System.Drawing.Point(219, 207);
            this.textBox_resetPatronPassword_name.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_resetPatronPassword_name.Name = "textBox_resetPatronPassword_name";
            this.textBox_resetPatronPassword_name.Size = new System.Drawing.Size(290, 31);
            this.textBox_resetPatronPassword_name.TabIndex = 13;
            // 
            // textBox_resetPatronPassword_phoneNumber
            // 
            this.textBox_resetPatronPassword_phoneNumber.Location = new System.Drawing.Point(219, 263);
            this.textBox_resetPatronPassword_phoneNumber.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_resetPatronPassword_phoneNumber.Name = "textBox_resetPatronPassword_phoneNumber";
            this.textBox_resetPatronPassword_phoneNumber.Size = new System.Drawing.Size(290, 31);
            this.textBox_resetPatronPassword_phoneNumber.TabIndex = 15;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(27, 266);
            this.label12.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(138, 21);
            this.label12.TabIndex = 14;
            this.label12.Text = "手机号码(&T):";
            // 
            // ChangePasswordForm
            // 
            this.AcceptButton = this.button_reader_changePassword;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 528);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "ChangePasswordForm";
            this.ShowInTaskbar = false;
            this.Text = "修改密码";
            this.Activated += new System.EventHandler(this.ChangePasswordForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChangePasswordForm_FormClosed);
            this.Load += new System.EventHandler(this.ChangePasswordForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_reader.ResumeLayout(false);
            this.tabPage_reader.PerformLayout();
            this.tabPage_worker.ResumeLayout(false);
            this.tabPage_worker.PerformLayout();
            this.tabPage_resetPatronPassword.ResumeLayout(false);
            this.tabPage_resetPatronPassword.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_reader_barcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_reader_oldPassword;
        private System.Windows.Forms.TextBox textBox_reader_newPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_reader_confirmNewPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_reader_changePassword;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_reader;
        private System.Windows.Forms.TabPage tabPage_worker;
        private System.Windows.Forms.TextBox textBox_worker_userName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_worker_confirmNewPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_worker_oldPassword;
        private System.Windows.Forms.TextBox textBox_worker_newPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_worker_changePassword;
        private System.Windows.Forms.CheckBox checkBox_worker_force;
        private System.Windows.Forms.TextBox textBox_reader_comment;
        private System.Windows.Forms.TextBox textBox_worker_comment;
        private System.Windows.Forms.TabPage tabPage_resetPatronPassword;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button_resetPatronPassword;
        private System.Windows.Forms.TextBox textBox_resetPatronPassword_barcode;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_resetPatronPassword_name;
        private System.Windows.Forms.TextBox textBox_resetPatronPassword_phoneNumber;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button_resetPatronPassword_displayTempPassword;
        private System.Windows.Forms.Label label1_resetPatronPassword_queryword;
        private System.Windows.Forms.TextBox textBox_resetPatronPassword_queryWord;
    }
}