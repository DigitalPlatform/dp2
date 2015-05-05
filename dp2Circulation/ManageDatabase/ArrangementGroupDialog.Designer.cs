namespace dp2Circulation
{
    partial class ArrangementGroupDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArrangementGroupDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_comment = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.button_getZhongcihaoDbName = new System.Windows.Forms.Button();
            this.textBox_zhongcihaoDbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_groupName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_classType = new System.Windows.Forms.ComboBox();
            this.comboBox_qufenhaoType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_callNumberType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkedComboBox_qufenhaoType = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(256, 230);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 14;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(195, 230);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 13;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_comment.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_comment.HideSelection = false;
            this.textBox_comment.Location = new System.Drawing.Point(9, 140);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(304, 82);
            this.textBox_comment.TabIndex = 12;
            // 
            // button_getZhongcihaoDbName
            // 
            this.button_getZhongcihaoDbName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getZhongcihaoDbName.Enabled = false;
            this.button_getZhongcihaoDbName.Location = new System.Drawing.Point(271, 80);
            this.button_getZhongcihaoDbName.Margin = new System.Windows.Forms.Padding(2);
            this.button_getZhongcihaoDbName.Name = "button_getZhongcihaoDbName";
            this.button_getZhongcihaoDbName.Size = new System.Drawing.Size(41, 22);
            this.button_getZhongcihaoDbName.TabIndex = 9;
            this.button_getZhongcihaoDbName.Text = "...";
            this.button_getZhongcihaoDbName.UseVisualStyleBackColor = true;
            this.button_getZhongcihaoDbName.Click += new System.EventHandler(this.button_getZhongcihaoDbName_Click);
            // 
            // textBox_zhongcihaoDbName
            // 
            this.textBox_zhongcihaoDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_zhongcihaoDbName.Enabled = false;
            this.textBox_zhongcihaoDbName.Location = new System.Drawing.Point(113, 81);
            this.textBox_zhongcihaoDbName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_zhongcihaoDbName.Name = "textBox_zhongcihaoDbName";
            this.textBox_zhongcihaoDbName.Size = new System.Drawing.Size(154, 21);
            this.textBox_zhongcihaoDbName.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 83);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "种次号库名(&Z):";
            // 
            // textBox_groupName
            // 
            this.textBox_groupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_groupName.Location = new System.Drawing.Point(113, 10);
            this.textBox_groupName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_groupName.Name = "textBox_groupName";
            this.textBox_groupName.Size = new System.Drawing.Size(154, 21);
            this.textBox_groupName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "排架体系名(&A):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 37);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "类号类型(&C):";
            // 
            // comboBox_classType
            // 
            this.comboBox_classType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_classType.FormattingEnabled = true;
            this.comboBox_classType.Items.AddRange(new object[] {
            "中图法",
            "科图法",
            "人大法",
            "石头汤分类法",
            "DDC",
            "UDC",
            "LCC",
            "其它"});
            this.comboBox_classType.Location = new System.Drawing.Point(113, 34);
            this.comboBox_classType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_classType.Name = "comboBox_classType";
            this.comboBox_classType.Size = new System.Drawing.Size(154, 20);
            this.comboBox_classType.TabIndex = 3;
            // 
            // comboBox_qufenhaoType
            // 
            this.comboBox_qufenhaoType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_qufenhaoType.FormattingEnabled = true;
            this.comboBox_qufenhaoType.Items.AddRange(new object[] {
            "GCAT",
            "种次号",
            "四角号码",
            "石头汤著者号",
            "手动",
            "Cutter-Sanborn Three-Figure",
            "<无>"});
            this.comboBox_qufenhaoType.Location = new System.Drawing.Point(271, 56);
            this.comboBox_qufenhaoType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_qufenhaoType.Name = "comboBox_qufenhaoType";
            this.comboBox_qufenhaoType.Size = new System.Drawing.Size(154, 20);
            this.comboBox_qufenhaoType.TabIndex = 6;
            this.comboBox_qufenhaoType.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 60);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "区分号类型(&Q):";
            // 
            // comboBox_callNumberType
            // 
            this.comboBox_callNumberType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_callNumberType.FormattingEnabled = true;
            this.comboBox_callNumberType.Items.AddRange(new object[] {
            "索取类号+区分号",
            "馆藏代码+索取类号+区分号"});
            this.comboBox_callNumberType.Location = new System.Drawing.Point(113, 106);
            this.comboBox_callNumberType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_callNumberType.Name = "comboBox_callNumberType";
            this.comboBox_callNumberType.Size = new System.Drawing.Size(154, 20);
            this.comboBox_callNumberType.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 108);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "索取号形态(&T):";
            // 
            // checkedComboBox_qufenhaoType
            // 
            this.checkedComboBox_qufenhaoType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_qufenhaoType.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_qufenhaoType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_qufenhaoType.Location = new System.Drawing.Point(113, 56);
            this.checkedComboBox_qufenhaoType.Margin = new System.Windows.Forms.Padding(2);
            this.checkedComboBox_qufenhaoType.MinimumSize = new System.Drawing.Size(90, 0);
            this.checkedComboBox_qufenhaoType.Name = "checkedComboBox_qufenhaoType";
            this.checkedComboBox_qufenhaoType.Padding = new System.Windows.Forms.Padding(6);
            this.checkedComboBox_qufenhaoType.Size = new System.Drawing.Size(154, 26);
            this.checkedComboBox_qufenhaoType.TabIndex = 5;
            this.checkedComboBox_qufenhaoType.DropDown += new System.EventHandler(this.checkedComboBox_qufenhaoType_DropDown);
            this.checkedComboBox_qufenhaoType.TextChanged += new System.EventHandler(this.checkedComboBox_qufenhaoType_TextChanged);
            // 
            // ArrangementGroupDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(321, 263);
            this.Controls.Add(this.checkedComboBox_qufenhaoType);
            this.Controls.Add(this.comboBox_callNumberType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox_qufenhaoType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox_classType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.button_getZhongcihaoDbName);
            this.Controls.Add(this.textBox_zhongcihaoDbName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_groupName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ArrangementGroupDialog";
            this.ShowInTaskbar = false;
            this.Text = "排架体系";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ArrangementGroupDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ArrangementGroupDialog_FormClosed);
            this.Load += new System.EventHandler(this.ArrangementGroupDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private DigitalPlatform.GUI.NoHasSelTextBox textBox_comment;
        private System.Windows.Forms.Button button_getZhongcihaoDbName;
        private System.Windows.Forms.TextBox textBox_zhongcihaoDbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_groupName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_classType;
        private System.Windows.Forms.ComboBox comboBox_qufenhaoType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_callNumberType;
        private System.Windows.Forms.Label label5;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_qufenhaoType;
    }
}