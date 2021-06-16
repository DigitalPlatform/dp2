
namespace RfidTool
{
    partial class SettingDialog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_writeTag = new System.Windows.Forms.TabPage();
            this.checkBox_writeTag_verifyPii = new System.Windows.Forms.CheckBox();
            this.checkBox_changeAOI = new System.Windows.Forms.CheckBox();
            this.groupBox_uhf = new System.Windows.Forms.GroupBox();
            this.checkBox_writeUserBank = new System.Windows.Forms.CheckBox();
            this.checkBox_warningWhenUhfFormatMismatch = new System.Windows.Forms.CheckBox();
            this.comboBox_uhfDataFormat = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel_oiHelp = new System.Windows.Forms.LinkLabel();
            this.textBox_rfid_aoi = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_rfid_oi = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_modifyTag = new System.Windows.Forms.TabPage();
            this.numericUpDown_seconds = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_other = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_verifyRule = new System.Windows.Forms.TextBox();
            this.checkBox_enableTagCache = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_writeTag_useLocalStoreage = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage_writeTag.SuspendLayout();
            this.groupBox_uhf.SuspendLayout();
            this.tabPage_modifyTag.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_seconds)).BeginInit();
            this.tabPage_other.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_writeTag);
            this.tabControl1.Controls.Add(this.tabPage_modifyTag);
            this.tabControl1.Controls.Add(this.tabPage_other);
            this.tabControl1.Location = new System.Drawing.Point(11, 10);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(726, 526);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_writeTag
            // 
            this.tabPage_writeTag.AutoScroll = true;
            this.tabPage_writeTag.Controls.Add(this.checkBox_writeTag_useLocalStoreage);
            this.tabPage_writeTag.Controls.Add(this.checkBox_writeTag_verifyPii);
            this.tabPage_writeTag.Controls.Add(this.checkBox_changeAOI);
            this.tabPage_writeTag.Controls.Add(this.groupBox_uhf);
            this.tabPage_writeTag.Controls.Add(this.linkLabel_oiHelp);
            this.tabPage_writeTag.Controls.Add(this.textBox_rfid_aoi);
            this.tabPage_writeTag.Controls.Add(this.label2);
            this.tabPage_writeTag.Controls.Add(this.textBox_rfid_oi);
            this.tabPage_writeTag.Controls.Add(this.label1);
            this.tabPage_writeTag.Location = new System.Drawing.Point(4, 31);
            this.tabPage_writeTag.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_writeTag.Name = "tabPage_writeTag";
            this.tabPage_writeTag.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_writeTag.Size = new System.Drawing.Size(718, 491);
            this.tabPage_writeTag.TabIndex = 0;
            this.tabPage_writeTag.Text = "写入标签";
            this.tabPage_writeTag.UseVisualStyleBackColor = true;
            // 
            // checkBox_writeTag_verifyPii
            // 
            this.checkBox_writeTag_verifyPii.AutoSize = true;
            this.checkBox_writeTag_verifyPii.Location = new System.Drawing.Point(10, 410);
            this.checkBox_writeTag_verifyPii.Name = "checkBox_writeTag_verifyPii";
            this.checkBox_writeTag_verifyPii.Size = new System.Drawing.Size(174, 25);
            this.checkBox_writeTag_verifyPii.TabIndex = 7;
            this.checkBox_writeTag_verifyPii.Text = "校验条码号(&V)";
            this.checkBox_writeTag_verifyPii.UseVisualStyleBackColor = true;
            // 
            // checkBox_changeAOI
            // 
            this.checkBox_changeAOI.AutoSize = true;
            this.checkBox_changeAOI.Location = new System.Drawing.Point(535, 65);
            this.checkBox_changeAOI.Name = "checkBox_changeAOI";
            this.checkBox_changeAOI.Size = new System.Drawing.Size(78, 25);
            this.checkBox_changeAOI.TabIndex = 6;
            this.checkBox_changeAOI.Text = "修改";
            this.checkBox_changeAOI.UseVisualStyleBackColor = true;
            this.checkBox_changeAOI.CheckedChanged += new System.EventHandler(this.checkBox_changeAOI_CheckedChanged);
            // 
            // groupBox_uhf
            // 
            this.groupBox_uhf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_uhf.Controls.Add(this.checkBox_writeUserBank);
            this.groupBox_uhf.Controls.Add(this.checkBox_warningWhenUhfFormatMismatch);
            this.groupBox_uhf.Controls.Add(this.comboBox_uhfDataFormat);
            this.groupBox_uhf.Controls.Add(this.label3);
            this.groupBox_uhf.Location = new System.Drawing.Point(10, 198);
            this.groupBox_uhf.Name = "groupBox_uhf";
            this.groupBox_uhf.Size = new System.Drawing.Size(702, 184);
            this.groupBox_uhf.TabIndex = 5;
            this.groupBox_uhf.TabStop = false;
            this.groupBox_uhf.Text = " UHF(超高频)标签 ";
            // 
            // checkBox_writeUserBank
            // 
            this.checkBox_writeUserBank.AutoSize = true;
            this.checkBox_writeUserBank.Location = new System.Drawing.Point(20, 93);
            this.checkBox_writeUserBank.Name = "checkBox_writeUserBank";
            this.checkBox_writeUserBank.Size = new System.Drawing.Size(242, 25);
            this.checkBox_writeUserBank.TabIndex = 2;
            this.checkBox_writeUserBank.Text = "要写入 User Bank(&U)";
            this.checkBox_writeUserBank.UseVisualStyleBackColor = true;
            // 
            // checkBox_warningWhenUhfFormatMismatch
            // 
            this.checkBox_warningWhenUhfFormatMismatch.AutoSize = true;
            this.checkBox_warningWhenUhfFormatMismatch.Location = new System.Drawing.Point(20, 124);
            this.checkBox_warningWhenUhfFormatMismatch.Name = "checkBox_warningWhenUhfFormatMismatch";
            this.checkBox_warningWhenUhfFormatMismatch.Size = new System.Drawing.Size(384, 25);
            this.checkBox_warningWhenUhfFormatMismatch.TabIndex = 3;
            this.checkBox_warningWhenUhfFormatMismatch.Text = "覆盖格式不同的标签原内容前警告(&W)";
            this.checkBox_warningWhenUhfFormatMismatch.UseVisualStyleBackColor = true;
            // 
            // comboBox_uhfDataFormat
            // 
            this.comboBox_uhfDataFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_uhfDataFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_uhfDataFormat.FormattingEnabled = true;
            this.comboBox_uhfDataFormat.Items.AddRange(new object[] {
            "高校联盟格式",
            "国标格式"});
            this.comboBox_uhfDataFormat.Location = new System.Drawing.Point(215, 38);
            this.comboBox_uhfDataFormat.Name = "comboBox_uhfDataFormat";
            this.comboBox_uhfDataFormat.Size = new System.Drawing.Size(481, 29);
            this.comboBox_uhfDataFormat.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "写入格式(&S):";
            // 
            // linkLabel_oiHelp
            // 
            this.linkLabel_oiHelp.AutoSize = true;
            this.linkLabel_oiHelp.Location = new System.Drawing.Point(6, 140);
            this.linkLabel_oiHelp.Name = "linkLabel_oiHelp";
            this.linkLabel_oiHelp.Size = new System.Drawing.Size(262, 21);
            this.linkLabel_oiHelp.TabIndex = 4;
            this.linkLabel_oiHelp.TabStop = true;
            this.linkLabel_oiHelp.Text = "帮助：如何设置机构代码？";
            this.linkLabel_oiHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_oiHelp_LinkClicked);
            // 
            // textBox_rfid_aoi
            // 
            this.textBox_rfid_aoi.Enabled = false;
            this.textBox_rfid_aoi.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_rfid_aoi.Location = new System.Drawing.Point(225, 62);
            this.textBox_rfid_aoi.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_rfid_aoi.Name = "textBox_rfid_aoi";
            this.textBox_rfid_aoi.Size = new System.Drawing.Size(294, 31);
            this.textBox_rfid_aoi.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(201, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "非标准机构代码(&A):";
            // 
            // textBox_rfid_oi
            // 
            this.textBox_rfid_oi.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_rfid_oi.Location = new System.Drawing.Point(225, 18);
            this.textBox_rfid_oi.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_rfid_oi.Name = "textBox_rfid_oi";
            this.textBox_rfid_oi.Size = new System.Drawing.Size(294, 31);
            this.textBox_rfid_oi.TabIndex = 1;
            this.textBox_rfid_oi.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_rfid_oi_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "机构代码(&O):";
            // 
            // tabPage_modifyTag
            // 
            this.tabPage_modifyTag.AutoScroll = true;
            this.tabPage_modifyTag.Controls.Add(this.numericUpDown_seconds);
            this.tabPage_modifyTag.Controls.Add(this.label4);
            this.tabPage_modifyTag.Location = new System.Drawing.Point(4, 31);
            this.tabPage_modifyTag.Name = "tabPage_modifyTag";
            this.tabPage_modifyTag.Size = new System.Drawing.Size(718, 491);
            this.tabPage_modifyTag.TabIndex = 2;
            this.tabPage_modifyTag.Text = "修改标签";
            this.tabPage_modifyTag.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_seconds
            // 
            this.numericUpDown_seconds.Location = new System.Drawing.Point(256, 39);
            this.numericUpDown_seconds.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_seconds.Name = "numericUpDown_seconds";
            this.numericUpDown_seconds.Size = new System.Drawing.Size(120, 31);
            this.numericUpDown_seconds.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(222, 21);
            this.label4.TabIndex = 0;
            this.label4.Text = "扫描前倒计时秒数(&S):";
            // 
            // tabPage_other
            // 
            this.tabPage_other.AutoScroll = true;
            this.tabPage_other.Controls.Add(this.groupBox1);
            this.tabPage_other.Controls.Add(this.checkBox_enableTagCache);
            this.tabPage_other.Location = new System.Drawing.Point(4, 31);
            this.tabPage_other.Name = "tabPage_other";
            this.tabPage_other.Size = new System.Drawing.Size(718, 491);
            this.tabPage_other.TabIndex = 1;
            this.tabPage_other.Text = "其它";
            this.tabPage_other.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_verifyRule);
            this.groupBox1.Location = new System.Drawing.Point(15, 99);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(689, 363);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 条码号校验规则 ";
            // 
            // textBox_verifyRule
            // 
            this.textBox_verifyRule.AcceptsReturn = true;
            this.textBox_verifyRule.AcceptsTab = true;
            this.textBox_verifyRule.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_verifyRule.Location = new System.Drawing.Point(20, 30);
            this.textBox_verifyRule.Multiline = true;
            this.textBox_verifyRule.Name = "textBox_verifyRule";
            this.textBox_verifyRule.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_verifyRule.Size = new System.Drawing.Size(644, 313);
            this.textBox_verifyRule.TabIndex = 2;
            // 
            // checkBox_enableTagCache
            // 
            this.checkBox_enableTagCache.AutoSize = true;
            this.checkBox_enableTagCache.Location = new System.Drawing.Point(15, 30);
            this.checkBox_enableTagCache.Name = "checkBox_enableTagCache";
            this.checkBox_enableTagCache.Size = new System.Drawing.Size(237, 25);
            this.checkBox_enableTagCache.TabIndex = 0;
            this.checkBox_enableTagCache.Text = "启用标签信息缓存(&C)";
            this.checkBox_enableTagCache.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(506, 540);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(111, 40);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(622, 540);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(111, 40);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_writeTag_useLocalStoreage
            // 
            this.checkBox_writeTag_useLocalStoreage.AutoSize = true;
            this.checkBox_writeTag_useLocalStoreage.Location = new System.Drawing.Point(225, 410);
            this.checkBox_writeTag_useLocalStoreage.Name = "checkBox_writeTag_useLocalStoreage";
            this.checkBox_writeTag_useLocalStoreage.Size = new System.Drawing.Size(195, 25);
            this.checkBox_writeTag_useLocalStoreage.TabIndex = 8;
            this.checkBox_writeTag_useLocalStoreage.Text = "查询本地存储(&L)";
            this.checkBox_writeTag_useLocalStoreage.UseVisualStyleBackColor = true;
            // 
            // SettingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(747, 589);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "SettingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_writeTag.ResumeLayout(false);
            this.tabPage_writeTag.PerformLayout();
            this.groupBox_uhf.ResumeLayout(false);
            this.groupBox_uhf.PerformLayout();
            this.tabPage_modifyTag.ResumeLayout(false);
            this.tabPage_modifyTag.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_seconds)).EndInit();
            this.tabPage_other.ResumeLayout(false);
            this.tabPage_other.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_writeTag;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_rfid_oi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_rfid_aoi;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel_oiHelp;
        private System.Windows.Forms.GroupBox groupBox_uhf;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_uhfDataFormat;
        private System.Windows.Forms.CheckBox checkBox_warningWhenUhfFormatMismatch;
        private System.Windows.Forms.CheckBox checkBox_writeUserBank;
        private System.Windows.Forms.TabPage tabPage_other;
        private System.Windows.Forms.CheckBox checkBox_enableTagCache;
        private System.Windows.Forms.TabPage tabPage_modifyTag;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown_seconds;
        private System.Windows.Forms.CheckBox checkBox_changeAOI;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_verifyRule;
        private System.Windows.Forms.CheckBox checkBox_writeTag_verifyPii;
        private System.Windows.Forms.CheckBox checkBox_writeTag_useLocalStoreage;
    }
}