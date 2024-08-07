namespace dp2Circulation
{
    partial class BiblioSaveToDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiblioSaveToDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_biblioDbName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_recordID = new System.Windows.Forms.TextBox();
            this.label_message = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_buildLink = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_buildLinkMessage = new System.Windows.Forms.Label();
            this.checkBox_copyChildRecords = new System.Windows.Forms.CheckBox();
            this.checkBox_compressTailNo = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 173);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "书目库名(&B):";
            // 
            // comboBox_biblioDbName
            // 
            this.comboBox_biblioDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_biblioDbName.FormattingEnabled = true;
            this.comboBox_biblioDbName.Location = new System.Drawing.Point(174, 168);
            this.comboBox_biblioDbName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_biblioDbName.Name = "comboBox_biblioDbName";
            this.comboBox_biblioDbName.Size = new System.Drawing.Size(316, 29);
            this.comboBox_biblioDbName.TabIndex = 1;
            this.comboBox_biblioDbName.DropDown += new System.EventHandler(this.comboBox_biblioDbName_DropDown);
            this.comboBox_biblioDbName.SelectedIndexChanged += new System.EventHandler(this.comboBox_biblioDbName_SelectedIndexChanged);
            this.comboBox_biblioDbName.TextChanged += new System.EventHandler(this.comboBox_biblioDbName_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 219);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "记录ID(&I):";
            // 
            // textBox_recordID
            // 
            this.textBox_recordID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recordID.Location = new System.Drawing.Point(174, 215);
            this.textBox_recordID.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_recordID.Name = "textBox_recordID";
            this.textBox_recordID.Size = new System.Drawing.Size(217, 31);
            this.textBox_recordID.TabIndex = 3;
            this.textBox_recordID.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_recordID_Validating);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.BackColor = System.Drawing.SystemColors.Info;
            this.label_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label_message.Location = new System.Drawing.Point(16, 18);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(596, 128);
            this.label_message.TabIndex = 8;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(510, 215);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(510, 168);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_buildLink
            // 
            this.checkBox_buildLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_buildLink.AutoSize = true;
            this.checkBox_buildLink.Location = new System.Drawing.Point(20, 37);
            this.checkBox_buildLink.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_buildLink.Name = "checkBox_buildLink";
            this.checkBox_buildLink.Size = new System.Drawing.Size(364, 25);
            this.checkBox_buildLink.TabIndex = 0;
            this.checkBox_buildLink.Text = "将当前记录设为新记录的 目标 (&T)";
            this.checkBox_buildLink.UseVisualStyleBackColor = true;
            this.checkBox_buildLink.CheckedChanged += new System.EventHandler(this.checkBox_buildLink_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label_buildLinkMessage);
            this.groupBox1.Controls.Add(this.checkBox_buildLink);
            this.groupBox1.Location = new System.Drawing.Point(18, 317);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(595, 186);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // label_buildLinkMessage
            // 
            this.label_buildLinkMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_buildLinkMessage.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label_buildLinkMessage.Location = new System.Drawing.Point(20, 65);
            this.label_buildLinkMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_buildLinkMessage.Name = "label_buildLinkMessage";
            this.label_buildLinkMessage.Size = new System.Drawing.Size(551, 102);
            this.label_buildLinkMessage.TabIndex = 1;
            this.label_buildLinkMessage.Text = "label3";
            // 
            // checkBox_copyChildRecords
            // 
            this.checkBox_copyChildRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_copyChildRecords.AutoSize = true;
            this.checkBox_copyChildRecords.Location = new System.Drawing.Point(18, 285);
            this.checkBox_copyChildRecords.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_copyChildRecords.Name = "checkBox_copyChildRecords";
            this.checkBox_copyChildRecords.Size = new System.Drawing.Size(501, 25);
            this.checkBox_copyChildRecords.TabIndex = 4;
            this.checkBox_copyChildRecords.Text = "同时复制子记录(册、订购、期、评注和对象) (&C)";
            this.checkBox_copyChildRecords.UseVisualStyleBackColor = true;
            // 
            // checkBox_compressTailNo
            // 
            this.checkBox_compressTailNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_compressTailNo.AutoSize = true;
            this.checkBox_compressTailNo.Location = new System.Drawing.Point(20, 511);
            this.checkBox_compressTailNo.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_compressTailNo.Name = "checkBox_compressTailNo";
            this.checkBox_compressTailNo.Size = new System.Drawing.Size(227, 25);
            this.checkBox_compressTailNo.TabIndex = 9;
            this.checkBox_compressTailNo.Text = "压缩数据库尾号 (&P)";
            this.checkBox_compressTailNo.UseVisualStyleBackColor = true;
            // 
            // BiblioSaveToDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(630, 569);
            this.Controls.Add(this.checkBox_compressTailNo);
            this.Controls.Add(this.checkBox_copyChildRecords);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.textBox_recordID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_biblioDbName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "BiblioSaveToDlg";
            this.ShowInTaskbar = false;
            this.Text = "复制书目记录到...";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BiblioSaveToDlg_FormClosed);
            this.Load += new System.EventHandler(this.BiblioSaveToDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_biblioDbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_recordID;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_buildLink;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_buildLinkMessage;
        private System.Windows.Forms.CheckBox checkBox_copyChildRecords;
        private System.Windows.Forms.CheckBox checkBox_compressTailNo;
    }
}