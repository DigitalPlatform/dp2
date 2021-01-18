
namespace RfidTool
{
    partial class BeginModifyDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_action = new System.Windows.Forms.TabPage();
            this.checkBox_writeUidLog = new System.Windows.Forms.CheckBox();
            this.checkBox_oi = new System.Windows.Forms.CheckBox();
            this.textBox_rfid_oi = new System.Windows.Forms.TextBox();
            this.label_eas = new System.Windows.Forms.Label();
            this.comboBox_eas = new System.Windows.Forms.ComboBox();
            this.checkBox_uidPiiMap = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_filter_tu = new System.Windows.Forms.ComboBox();
            this.checkBox_aoi = new System.Windows.Forms.CheckBox();
            this.linkLabel_oiHelp = new System.Windows.Forms.LinkLabel();
            this.textBox_rfid_aoi = new System.Windows.Forms.TextBox();
            this.tabPage_other = new System.Windows.Forms.TabPage();
            this.checkBox_verifyPii = new System.Windows.Forms.CheckBox();
            this.button_modifyVerifyRule = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_action.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(677, 523);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(111, 40);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(561, 523);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(111, 40);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_action);
            this.tabControl1.Controls.Add(this.tabPage_other);
            this.tabControl1.Location = new System.Drawing.Point(12, 11);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(776, 508);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage_action
            // 
            this.tabPage_action.AutoScroll = true;
            this.tabPage_action.Controls.Add(this.button_modifyVerifyRule);
            this.tabPage_action.Controls.Add(this.checkBox_verifyPii);
            this.tabPage_action.Controls.Add(this.checkBox_writeUidLog);
            this.tabPage_action.Controls.Add(this.checkBox_oi);
            this.tabPage_action.Controls.Add(this.textBox_rfid_oi);
            this.tabPage_action.Controls.Add(this.label_eas);
            this.tabPage_action.Controls.Add(this.comboBox_eas);
            this.tabPage_action.Controls.Add(this.checkBox_uidPiiMap);
            this.tabPage_action.Controls.Add(this.label1);
            this.tabPage_action.Controls.Add(this.comboBox_filter_tu);
            this.tabPage_action.Controls.Add(this.checkBox_aoi);
            this.tabPage_action.Controls.Add(this.linkLabel_oiHelp);
            this.tabPage_action.Controls.Add(this.textBox_rfid_aoi);
            this.tabPage_action.Location = new System.Drawing.Point(4, 31);
            this.tabPage_action.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_action.Name = "tabPage_action";
            this.tabPage_action.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_action.Size = new System.Drawing.Size(768, 473);
            this.tabPage_action.TabIndex = 0;
            this.tabPage_action.Text = "动作参数";
            this.tabPage_action.UseVisualStyleBackColor = true;
            // 
            // checkBox_writeUidLog
            // 
            this.checkBox_writeUidLog.AutoSize = true;
            this.checkBox_writeUidLog.Checked = true;
            this.checkBox_writeUidLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_writeUidLog.Enabled = false;
            this.checkBox_writeUidLog.Location = new System.Drawing.Point(10, 343);
            this.checkBox_writeUidLog.Name = "checkBox_writeUidLog";
            this.checkBox_writeUidLog.Size = new System.Drawing.Size(294, 25);
            this.checkBox_writeUidLog.TabIndex = 12;
            this.checkBox_writeUidLog.Text = "创建 UID PII 对照日志(&M)";
            this.checkBox_writeUidLog.UseVisualStyleBackColor = true;
            this.checkBox_writeUidLog.CheckedChanged += new System.EventHandler(this.checkBox_writeUidLog_CheckedChanged);
            // 
            // checkBox_oi
            // 
            this.checkBox_oi.Location = new System.Drawing.Point(10, 96);
            this.checkBox_oi.Name = "checkBox_oi";
            this.checkBox_oi.Size = new System.Drawing.Size(228, 25);
            this.checkBox_oi.TabIndex = 5;
            this.checkBox_oi.Text = "机构代码(&O)";
            this.checkBox_oi.UseVisualStyleBackColor = true;
            this.checkBox_oi.CheckedChanged += new System.EventHandler(this.checkBox_oi_CheckedChanged);
            // 
            // textBox_rfid_oi
            // 
            this.textBox_rfid_oi.Enabled = false;
            this.textBox_rfid_oi.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_rfid_oi.Location = new System.Drawing.Point(244, 94);
            this.textBox_rfid_oi.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_rfid_oi.Name = "textBox_rfid_oi";
            this.textBox_rfid_oi.Size = new System.Drawing.Size(294, 31);
            this.textBox_rfid_oi.TabIndex = 1;
            // 
            // label_eas
            // 
            this.label_eas.Location = new System.Drawing.Point(6, 264);
            this.label_eas.Name = "label_eas";
            this.label_eas.Size = new System.Drawing.Size(232, 21);
            this.label_eas.TabIndex = 11;
            this.label_eas.Text = "EAS(&E):";
            // 
            // comboBox_eas
            // 
            this.comboBox_eas.FormattingEnabled = true;
            this.comboBox_eas.Items.AddRange(new object[] {
            "不修改",
            "On",
            "Off"});
            this.comboBox_eas.Location = new System.Drawing.Point(244, 261);
            this.comboBox_eas.Name = "comboBox_eas";
            this.comboBox_eas.Size = new System.Drawing.Size(294, 29);
            this.comboBox_eas.TabIndex = 10;
            this.comboBox_eas.TextChanged += new System.EventHandler(this.comboBox_eas_TextChanged);
            // 
            // checkBox_uidPiiMap
            // 
            this.checkBox_uidPiiMap.AutoSize = true;
            this.checkBox_uidPiiMap.Location = new System.Drawing.Point(455, 343);
            this.checkBox_uidPiiMap.Name = "checkBox_uidPiiMap";
            this.checkBox_uidPiiMap.Size = new System.Drawing.Size(294, 25);
            this.checkBox_uidPiiMap.TabIndex = 9;
            this.checkBox_uidPiiMap.Text = "建立 UID PII 对照关系(&M)";
            this.checkBox_uidPiiMap.UseVisualStyleBackColor = true;
            this.checkBox_uidPiiMap.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 21);
            this.label1.TabIndex = 8;
            this.label1.Text = "筛选应用类别(&T):";
            // 
            // comboBox_filter_tu
            // 
            this.comboBox_filter_tu.FormattingEnabled = true;
            this.comboBox_filter_tu.Items.AddRange(new object[] {
            "图书",
            "读者证",
            "层架标",
            "所有类别"});
            this.comboBox_filter_tu.Location = new System.Drawing.Point(244, 28);
            this.comboBox_filter_tu.Name = "comboBox_filter_tu";
            this.comboBox_filter_tu.Size = new System.Drawing.Size(294, 29);
            this.comboBox_filter_tu.TabIndex = 7;
            // 
            // checkBox_aoi
            // 
            this.checkBox_aoi.Location = new System.Drawing.Point(10, 143);
            this.checkBox_aoi.Name = "checkBox_aoi";
            this.checkBox_aoi.Size = new System.Drawing.Size(228, 25);
            this.checkBox_aoi.TabIndex = 6;
            this.checkBox_aoi.Text = "非标准机构代码(&A)";
            this.checkBox_aoi.UseVisualStyleBackColor = true;
            this.checkBox_aoi.CheckedChanged += new System.EventHandler(this.checkBox_aoi_CheckedChanged);
            // 
            // linkLabel_oiHelp
            // 
            this.linkLabel_oiHelp.AutoSize = true;
            this.linkLabel_oiHelp.Location = new System.Drawing.Point(6, 193);
            this.linkLabel_oiHelp.Name = "linkLabel_oiHelp";
            this.linkLabel_oiHelp.Size = new System.Drawing.Size(262, 21);
            this.linkLabel_oiHelp.TabIndex = 4;
            this.linkLabel_oiHelp.TabStop = true;
            this.linkLabel_oiHelp.Text = "帮助：如何设置机构代码？";
            // 
            // textBox_rfid_aoi
            // 
            this.textBox_rfid_aoi.Enabled = false;
            this.textBox_rfid_aoi.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_rfid_aoi.Location = new System.Drawing.Point(244, 141);
            this.textBox_rfid_aoi.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_rfid_aoi.Name = "textBox_rfid_aoi";
            this.textBox_rfid_aoi.Size = new System.Drawing.Size(294, 31);
            this.textBox_rfid_aoi.TabIndex = 3;
            // 
            // tabPage_other
            // 
            this.tabPage_other.Location = new System.Drawing.Point(4, 31);
            this.tabPage_other.Name = "tabPage_other";
            this.tabPage_other.Size = new System.Drawing.Size(768, 473);
            this.tabPage_other.TabIndex = 1;
            this.tabPage_other.Text = "其它";
            this.tabPage_other.UseVisualStyleBackColor = true;
            // 
            // checkBox_verifyPii
            // 
            this.checkBox_verifyPii.AutoSize = true;
            this.checkBox_verifyPii.Location = new System.Drawing.Point(10, 392);
            this.checkBox_verifyPii.Name = "checkBox_verifyPii";
            this.checkBox_verifyPii.Size = new System.Drawing.Size(166, 25);
            this.checkBox_verifyPii.TabIndex = 13;
            this.checkBox_verifyPii.Text = "校验 PII (&V)";
            this.checkBox_verifyPii.UseVisualStyleBackColor = true;
            // 
            // button_modifyVerifyRule
            // 
            this.button_modifyVerifyRule.Location = new System.Drawing.Point(244, 384);
            this.button_modifyVerifyRule.Name = "button_modifyVerifyRule";
            this.button_modifyVerifyRule.Size = new System.Drawing.Size(294, 39);
            this.button_modifyVerifyRule.TabIndex = 14;
            this.button_modifyVerifyRule.Text = "配置校验规则 ...";
            this.button_modifyVerifyRule.UseVisualStyleBackColor = true;
            this.button_modifyVerifyRule.Click += new System.EventHandler(this.button_modifyVerifyRule_Click);
            // 
            // BeginModifyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 574);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "BeginModifyDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "开始修改";
            this.Load += new System.EventHandler(this.BeginModifyDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_action.ResumeLayout(false);
            this.tabPage_action.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_action;
        private System.Windows.Forms.LinkLabel linkLabel_oiHelp;
        private System.Windows.Forms.TextBox textBox_rfid_aoi;
        private System.Windows.Forms.TextBox textBox_rfid_oi;
        private System.Windows.Forms.TabPage tabPage_other;
        private System.Windows.Forms.CheckBox checkBox_aoi;
        private System.Windows.Forms.CheckBox checkBox_oi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_filter_tu;
        private System.Windows.Forms.CheckBox checkBox_uidPiiMap;
        private System.Windows.Forms.Label label_eas;
        private System.Windows.Forms.ComboBox comboBox_eas;
        private System.Windows.Forms.CheckBox checkBox_writeUidLog;
        private System.Windows.Forms.CheckBox checkBox_verifyPii;
        private System.Windows.Forms.Button button_modifyVerifyRule;
    }
}