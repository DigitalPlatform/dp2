namespace dp2Circulation
{
    partial class ChangeReaderActionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeReaderActionDialog));
            this.checkedComboBox_stateRemove = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkedComboBox_stateAdd = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_readerType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.comboBox_state = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_expireDate = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dateControl_expireDate = new DigitalPlatform.CommonControl.DateControl();
            this.label_expireDate = new System.Windows.Forms.Label();
            this.label_state = new System.Windows.Forms.Label();
            this.label_readerType = new System.Windows.Forms.Label();
            this.comboBox_fieldName = new System.Windows.Forms.ComboBox();
            this.textBox_fieldValue = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_inputTimeString = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_rfc1123Single = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.label_fieldName = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkedComboBox_stateRemove
            // 
            this.checkedComboBox_stateRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateRemove.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateRemove.Location = new System.Drawing.Point(188, 128);
            this.checkedComboBox_stateRemove.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateRemove.Name = "checkedComboBox_stateRemove";
            this.checkedComboBox_stateRemove.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateRemove.Size = new System.Drawing.Size(153, 22);
            this.checkedComboBox_stateRemove.TabIndex = 8;
            this.checkedComboBox_stateRemove.DropDown += new System.EventHandler(this.checkedComboBox_stateRemove_DropDown);
            this.checkedComboBox_stateRemove.TextChanged += new System.EventHandler(this.checkedComboBox_stateRemove_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(124, 130);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "减(&R):";
            // 
            // checkedComboBox_stateAdd
            // 
            this.checkedComboBox_stateAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateAdd.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateAdd.Location = new System.Drawing.Point(188, 106);
            this.checkedComboBox_stateAdd.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateAdd.Name = "checkedComboBox_stateAdd";
            this.checkedComboBox_stateAdd.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateAdd.Size = new System.Drawing.Size(153, 22);
            this.checkedComboBox_stateAdd.TabIndex = 6;
            this.checkedComboBox_stateAdd.DropDown += new System.EventHandler(this.checkedComboBox_stateAdd_DropDown);
            this.checkedComboBox_stateAdd.TextChanged += new System.EventHandler(this.checkedComboBox_stateAdd_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(124, 108);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "增(&A):";
            // 
            // comboBox_readerType
            // 
            this.comboBox_readerType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_readerType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_readerType.FormattingEnabled = true;
            this.comboBox_readerType.Location = new System.Drawing.Point(125, 179);
            this.comboBox_readerType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_readerType.Name = "comboBox_readerType";
            this.comboBox_readerType.Size = new System.Drawing.Size(217, 20);
            this.comboBox_readerType.TabIndex = 10;
            this.comboBox_readerType.Text = "<不改变>";
            this.comboBox_readerType.DropDown += new System.EventHandler(this.comboBox_readerType_DropDown);
            this.comboBox_readerType.SelectedIndexChanged += new System.EventHandler(this.comboBox_readerType_SelectedIndexChanged);
            this.comboBox_readerType.SizeChanged += new System.EventHandler(this.comboBox_readerType_SizeChanged);
            this.comboBox_readerType.TextChanged += new System.EventHandler(this.comboBox_readerType_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 182);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "读者类别(&T):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(285, 247);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 12;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(224, 247);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // comboBox_state
            // 
            this.comboBox_state.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_state.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_state.FormattingEnabled = true;
            this.comboBox_state.Items.AddRange(new object[] {
            "<不改变>",
            "<增、减>"});
            this.comboBox_state.Location = new System.Drawing.Point(125, 82);
            this.comboBox_state.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_state.Name = "comboBox_state";
            this.comboBox_state.Size = new System.Drawing.Size(217, 20);
            this.comboBox_state.TabIndex = 4;
            this.comboBox_state.Text = "<不改变>";
            this.comboBox_state.SizeChanged += new System.EventHandler(this.comboBox_state_SizeChanged);
            this.comboBox_state.TextChanged += new System.EventHandler(this.comboBox_state_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 84);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "状态(&S):";
            // 
            // comboBox_expireDate
            // 
            this.comboBox_expireDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_expireDate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_expireDate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_expireDate.FormattingEnabled = true;
            this.comboBox_expireDate.Items.AddRange(new object[] {
            "<不改变>",
            "<当前时间>",
            "<指定时间>",
            "<清除>"});
            this.comboBox_expireDate.Location = new System.Drawing.Point(125, 10);
            this.comboBox_expireDate.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_expireDate.Name = "comboBox_expireDate";
            this.comboBox_expireDate.Size = new System.Drawing.Size(217, 20);
            this.comboBox_expireDate.TabIndex = 1;
            this.comboBox_expireDate.SizeChanged += new System.EventHandler(this.comboBox_expireDate_SizeChanged);
            this.comboBox_expireDate.TextChanged += new System.EventHandler(this.comboBox_expireDate_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "失效日期(&E):";
            // 
            // dateControl_expireDate
            // 
            this.dateControl_expireDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateControl_expireDate.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_expireDate.Caption = "失效日期";
            this.dateControl_expireDate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_expireDate.Location = new System.Drawing.Point(125, 34);
            this.dateControl_expireDate.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_expireDate.MinimumSize = new System.Drawing.Size(100, 0);
            this.dateControl_expireDate.Name = "dateControl_expireDate";
            this.dateControl_expireDate.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_expireDate.Size = new System.Drawing.Size(214, 22);
            this.dateControl_expireDate.TabIndex = 2;
            this.dateControl_expireDate.Value = new System.DateTime(((long)(0)));
            // 
            // label_expireDate
            // 
            this.label_expireDate.Location = new System.Drawing.Point(112, 10);
            this.label_expireDate.Margin = new System.Windows.Forms.Padding(0);
            this.label_expireDate.Name = "label_expireDate";
            this.label_expireDate.Size = new System.Drawing.Size(10, 46);
            this.label_expireDate.TabIndex = 13;
            // 
            // label_state
            // 
            this.label_state.Location = new System.Drawing.Point(112, 82);
            this.label_state.Margin = new System.Windows.Forms.Padding(0);
            this.label_state.Name = "label_state";
            this.label_state.Size = new System.Drawing.Size(10, 68);
            this.label_state.TabIndex = 14;
            // 
            // label_readerType
            // 
            this.label_readerType.Location = new System.Drawing.Point(112, 179);
            this.label_readerType.Margin = new System.Windows.Forms.Padding(0);
            this.label_readerType.Name = "label_readerType";
            this.label_readerType.Size = new System.Drawing.Size(10, 20);
            this.label_readerType.TabIndex = 15;
            // 
            // comboBox_fieldName
            // 
            this.comboBox_fieldName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_fieldName.FormattingEnabled = true;
            this.comboBox_fieldName.Items.AddRange(new object[] {
            "<不使用>",
            "证条码号",
            "证号",
            "发证日期",
            "失效日期",
            "注释",
            "租金周期",
            "租金失效期",
            "押金余额",
            "姓名",
            "性别",
            "出生日期",
            "身份证号",
            "单位",
            "职务",
            "地址",
            "电话",
            "Email地址"});
            this.comboBox_fieldName.Location = new System.Drawing.Point(11, 214);
            this.comboBox_fieldName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_fieldName.Name = "comboBox_fieldName";
            this.comboBox_fieldName.Size = new System.Drawing.Size(89, 20);
            this.comboBox_fieldName.TabIndex = 16;
            this.comboBox_fieldName.Text = "<不使用>";
            this.comboBox_fieldName.TextChanged += new System.EventHandler(this.comboBox_fieldName_TextChanged);
            // 
            // textBox_fieldValue
            // 
            this.textBox_fieldValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_fieldValue.Enabled = false;
            this.textBox_fieldValue.Location = new System.Drawing.Point(125, 213);
            this.textBox_fieldValue.Name = "textBox_fieldValue";
            this.textBox_fieldValue.Size = new System.Drawing.Size(182, 21);
            this.textBox_fieldValue.TabIndex = 17;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_inputTimeString});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip1.Location = new System.Drawing.Point(312, 213);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStrip1.Size = new System.Drawing.Size(30, 23);
            this.toolStrip1.TabIndex = 20;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_inputTimeString
            // 
            this.toolStripDropDownButton_inputTimeString.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_inputTimeString.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rfc1123Single,
            this.toolStripSeparator1});
            this.toolStripDropDownButton_inputTimeString.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_inputTimeString.Image")));
            this.toolStripDropDownButton_inputTimeString.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_inputTimeString.Name = "toolStripDropDownButton_inputTimeString";
            this.toolStripDropDownButton_inputTimeString.Size = new System.Drawing.Size(29, 20);
            this.toolStripDropDownButton_inputTimeString.Text = "和时间有关的功能";
            // 
            // ToolStripMenuItem_rfc1123Single
            // 
            this.ToolStripMenuItem_rfc1123Single.Name = "ToolStripMenuItem_rfc1123Single";
            this.ToolStripMenuItem_rfc1123Single.Size = new System.Drawing.Size(171, 22);
            this.ToolStripMenuItem_rfc1123Single.Text = "RFC1123时间值...";
            this.ToolStripMenuItem_rfc1123Single.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Single_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // label_fieldName
            // 
            this.label_fieldName.Location = new System.Drawing.Point(112, 213);
            this.label_fieldName.Margin = new System.Windows.Forms.Padding(0);
            this.label_fieldName.Name = "label_fieldName";
            this.label_fieldName.Size = new System.Drawing.Size(10, 20);
            this.label_fieldName.TabIndex = 21;
            // 
            // ChangeReaderActionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 280);
            this.Controls.Add(this.label_fieldName);
            this.Controls.Add(this.textBox_fieldValue);
            this.Controls.Add(this.comboBox_fieldName);
            this.Controls.Add(this.label_readerType);
            this.Controls.Add(this.label_state);
            this.Controls.Add(this.label_expireDate);
            this.Controls.Add(this.dateControl_expireDate);
            this.Controls.Add(this.checkedComboBox_stateRemove);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkedComboBox_stateAdd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_readerType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_state);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_expireDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangeReaderActionDialog";
            this.ShowInTaskbar = false;
            this.Text = "动作参数";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChangeReaderActionDialog_FormClosed);
            this.Load += new System.EventHandler(this.ChangeReaderActionDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateRemove;
        private System.Windows.Forms.Label label4;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateAdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_readerType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ComboBox comboBox_state;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_expireDate;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.DateControl dateControl_expireDate;
        private System.Windows.Forms.Label label_expireDate;
        private System.Windows.Forms.Label label_state;
        private System.Windows.Forms.Label label_readerType;
        private System.Windows.Forms.ComboBox comboBox_fieldName;
        private System.Windows.Forms.TextBox textBox_fieldValue;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_inputTimeString;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfc1123Single;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label label_fieldName;
    }
}