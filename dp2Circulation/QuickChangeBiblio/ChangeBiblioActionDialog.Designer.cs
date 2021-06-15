namespace dp2Circulation
{
    partial class ChangeBiblioActionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeBiblioActionDialog));
            this.comboBox_batchNo = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.comboBox_state = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_opertime = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkedComboBox_stateAdd = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.checkedComboBox_stateRemove = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label_operTime = new System.Windows.Forms.Label();
            this.label_state = new System.Windows.Forms.Label();
            this.label_batchNo = new System.Windows.Forms.Label();
            this.checkBox_add102 = new System.Windows.Forms.CheckBox();
            this.checkBox_addPublisher = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // comboBox_batchNo
            // 
            this.comboBox_batchNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_batchNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_batchNo.FormattingEnabled = true;
            this.comboBox_batchNo.Items.AddRange(new object[] {
            "<不改变>"});
            this.comboBox_batchNo.Location = new System.Drawing.Point(191, 313);
            this.comboBox_batchNo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_batchNo.Name = "comboBox_batchNo";
            this.comboBox_batchNo.Size = new System.Drawing.Size(414, 29);
            this.comboBox_batchNo.TabIndex = 13;
            this.comboBox_batchNo.Text = "<不改变>";
            this.comboBox_batchNo.SizeChanged += new System.EventHandler(this.comboBox_batchNo_SizeChanged);
            this.comboBox_batchNo.TextChanged += new System.EventHandler(this.comboBox_batchNo_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 318);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 21);
            this.label5.TabIndex = 11;
            this.label5.Text = "批次号(&N):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(504, 507);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 15;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(392, 507);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 14;
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
            this.comboBox_state.Location = new System.Drawing.Point(191, 144);
            this.comboBox_state.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_state.Name = "comboBox_state";
            this.comboBox_state.Size = new System.Drawing.Size(414, 29);
            this.comboBox_state.TabIndex = 6;
            this.comboBox_state.Text = "<不改变>";
            this.comboBox_state.SizeChanged += new System.EventHandler(this.comboBox_state_SizeChanged);
            this.comboBox_state.TextChanged += new System.EventHandler(this.comboBox_state_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 147);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "状态(&S):";
            // 
            // comboBox_opertime
            // 
            this.comboBox_opertime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_opertime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_opertime.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_opertime.FormattingEnabled = true;
            this.comboBox_opertime.Items.AddRange(new object[] {
            "<不改变>",
            "<当前时间>",
            "<指定时间>",
            "<清除>"});
            this.comboBox_opertime.Location = new System.Drawing.Point(191, 18);
            this.comboBox_opertime.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_opertime.Name = "comboBox_opertime";
            this.comboBox_opertime.Size = new System.Drawing.Size(414, 29);
            this.comboBox_opertime.TabIndex = 2;
            this.comboBox_opertime.SizeChanged += new System.EventHandler(this.comboBox_opertime_SizeChanged);
            this.comboBox_opertime.TextChanged += new System.EventHandler(this.comboBox_opertime_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "操作时间(&L):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(185, 186);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 21);
            this.label2.TabIndex = 7;
            this.label2.Text = "增(&A):";
            // 
            // checkedComboBox_stateAdd
            // 
            this.checkedComboBox_stateAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateAdd.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateAdd.Location = new System.Drawing.Point(270, 186);
            this.checkedComboBox_stateAdd.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateAdd.Name = "checkedComboBox_stateAdd";
            this.checkedComboBox_stateAdd.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateAdd.ReadOnly = false;
            this.checkedComboBox_stateAdd.Size = new System.Drawing.Size(337, 32);
            this.checkedComboBox_stateAdd.TabIndex = 8;
            this.checkedComboBox_stateAdd.DropDown += new System.EventHandler(this.checkedComboBox_stateAdd_DropDown);
            this.checkedComboBox_stateAdd.TextChanged += new System.EventHandler(this.checkedComboBox_stateAdd_TextChanged);
            // 
            // checkedComboBox_stateRemove
            // 
            this.checkedComboBox_stateRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateRemove.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateRemove.Location = new System.Drawing.Point(270, 224);
            this.checkedComboBox_stateRemove.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateRemove.Name = "checkedComboBox_stateRemove";
            this.checkedComboBox_stateRemove.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateRemove.ReadOnly = false;
            this.checkedComboBox_stateRemove.Size = new System.Drawing.Size(337, 32);
            this.checkedComboBox_stateRemove.TabIndex = 10;
            this.checkedComboBox_stateRemove.DropDown += new System.EventHandler(this.checkedComboBox_stateRemove_DropDown);
            this.checkedComboBox_stateRemove.TextChanged += new System.EventHandler(this.checkedComboBox_stateRemove_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(185, 224);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 21);
            this.label4.TabIndex = 9;
            this.label4.Text = "减(&R):";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(191, 58);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(414, 31);
            this.dateTimePicker1.TabIndex = 3;
            // 
            // label_operTime
            // 
            this.label_operTime.Location = new System.Drawing.Point(167, 18);
            this.label_operTime.Margin = new System.Windows.Forms.Padding(0);
            this.label_operTime.Name = "label_operTime";
            this.label_operTime.Size = new System.Drawing.Size(18, 77);
            this.label_operTime.TabIndex = 1;
            // 
            // label_state
            // 
            this.label_state.Location = new System.Drawing.Point(167, 144);
            this.label_state.Margin = new System.Windows.Forms.Padding(0);
            this.label_state.Name = "label_state";
            this.label_state.Size = new System.Drawing.Size(18, 119);
            this.label_state.TabIndex = 5;
            // 
            // label_batchNo
            // 
            this.label_batchNo.Location = new System.Drawing.Point(167, 313);
            this.label_batchNo.Margin = new System.Windows.Forms.Padding(0);
            this.label_batchNo.Name = "label_batchNo";
            this.label_batchNo.Size = new System.Drawing.Size(18, 35);
            this.label_batchNo.TabIndex = 12;
            // 
            // checkBox_add102
            // 
            this.checkBox_add102.AutoSize = true;
            this.checkBox_add102.Location = new System.Drawing.Point(21, 389);
            this.checkBox_add102.Name = "checkBox_add102";
            this.checkBox_add102.Size = new System.Drawing.Size(208, 25);
            this.checkBox_add102.TabIndex = 16;
            this.checkBox_add102.Text = "添加 102 字段(&A)";
            this.checkBox_add102.UseVisualStyleBackColor = true;
            // 
            // checkBox_addPublisher
            // 
            this.checkBox_addPublisher.AutoSize = true;
            this.checkBox_addPublisher.Location = new System.Drawing.Point(255, 389);
            this.checkBox_addPublisher.Name = "checkBox_addPublisher";
            this.checkBox_addPublisher.Size = new System.Drawing.Size(237, 25);
            this.checkBox_addPublisher.TabIndex = 17;
            this.checkBox_addPublisher.Text = "添加出版社子字段(&P)";
            this.checkBox_addPublisher.UseVisualStyleBackColor = true;
            // 
            // ChangeBiblioActionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(623, 565);
            this.Controls.Add(this.checkBox_addPublisher);
            this.Controls.Add(this.checkBox_add102);
            this.Controls.Add(this.label_batchNo);
            this.Controls.Add(this.label_state);
            this.Controls.Add(this.label_operTime);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.checkedComboBox_stateRemove);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkedComboBox_stateAdd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_batchNo);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_state);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_opertime);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ChangeBiblioActionDialog";
            this.ShowInTaskbar = false;
            this.Text = "动作参数";
            this.Load += new System.EventHandler(this.ChangeBiblioActionDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_batchNo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ComboBox comboBox_state;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_opertime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateAdd;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateRemove;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label_operTime;
        private System.Windows.Forms.Label label_state;
        private System.Windows.Forms.Label label_batchNo;
        private System.Windows.Forms.CheckBox checkBox_add102;
        private System.Windows.Forms.CheckBox checkBox_addPublisher;
    }
}