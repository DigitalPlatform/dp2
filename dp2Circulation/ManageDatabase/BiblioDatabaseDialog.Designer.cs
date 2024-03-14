namespace dp2Circulation
{
    partial class BiblioDatabaseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiblioDatabaseDialog));
            this.label_biblioDbName = new System.Windows.Forms.Label();
            this.textBox_biblioDbName = new System.Windows.Forms.TextBox();
            this.textBox_entityDbName = new System.Windows.Forms.TextBox();
            this.label_entityDbName = new System.Windows.Forms.Label();
            this.textBox_orderDbName = new System.Windows.Forms.TextBox();
            this.label_orderDbName = new System.Windows.Forms.Label();
            this.textBox_issueDbName = new System.Windows.Forms.TextBox();
            this.label_issueDbName = new System.Windows.Forms.Label();
            this.label_marcSyntax = new System.Windows.Forms.Label();
            this.comboBox_syntax = new System.Windows.Forms.ComboBox();
            this.comboBox_documentType = new System.Windows.Forms.ComboBox();
            this.label_usage = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_inCirculation = new System.Windows.Forms.CheckBox();
            this.label_role = new System.Windows.Forms.Label();
            this.checkedComboBox_role = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.textBox_commentDbName = new System.Windows.Forms.TextBox();
            this.label_commentDbName = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_normal = new System.Windows.Forms.TabPage();
            this.checkBox_lockDbNames = new System.Windows.Forms.CheckBox();
            this.tabPage_replicate = new System.Windows.Forms.TabPage();
            this.comboBox_replication_centerServer = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBox_replication_dbName = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage_normal.SuspendLayout();
            this.tabPage_replicate.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_biblioDbName
            // 
            this.label_biblioDbName.AutoSize = true;
            this.label_biblioDbName.Location = new System.Drawing.Point(22, 19);
            this.label_biblioDbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_biblioDbName.Name = "label_biblioDbName";
            this.label_biblioDbName.Size = new System.Drawing.Size(138, 21);
            this.label_biblioDbName.TabIndex = 0;
            this.label_biblioDbName.Text = "书目库名(&B):";
            // 
            // textBox_biblioDbName
            // 
            this.textBox_biblioDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblioDbName.Location = new System.Drawing.Point(188, 16);
            this.textBox_biblioDbName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_biblioDbName.Name = "textBox_biblioDbName";
            this.textBox_biblioDbName.Size = new System.Drawing.Size(316, 31);
            this.textBox_biblioDbName.TabIndex = 1;
            this.textBox_biblioDbName.TextChanged += new System.EventHandler(this.textBox_biblioDbName_TextChanged);
            // 
            // textBox_entityDbName
            // 
            this.textBox_entityDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_entityDbName.Location = new System.Drawing.Point(188, 227);
            this.textBox_entityDbName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_entityDbName.Name = "textBox_entityDbName";
            this.textBox_entityDbName.Size = new System.Drawing.Size(316, 31);
            this.textBox_entityDbName.TabIndex = 9;
            // 
            // label_entityDbName
            // 
            this.label_entityDbName.AutoSize = true;
            this.label_entityDbName.Location = new System.Drawing.Point(22, 233);
            this.label_entityDbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_entityDbName.Name = "label_entityDbName";
            this.label_entityDbName.Size = new System.Drawing.Size(138, 21);
            this.label_entityDbName.TabIndex = 8;
            this.label_entityDbName.Text = "实体库名(&E):";
            // 
            // textBox_orderDbName
            // 
            this.textBox_orderDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_orderDbName.Location = new System.Drawing.Point(188, 271);
            this.textBox_orderDbName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_orderDbName.Name = "textBox_orderDbName";
            this.textBox_orderDbName.Size = new System.Drawing.Size(316, 31);
            this.textBox_orderDbName.TabIndex = 11;
            // 
            // label_orderDbName
            // 
            this.label_orderDbName.AutoSize = true;
            this.label_orderDbName.Location = new System.Drawing.Point(22, 275);
            this.label_orderDbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_orderDbName.Name = "label_orderDbName";
            this.label_orderDbName.Size = new System.Drawing.Size(138, 21);
            this.label_orderDbName.TabIndex = 10;
            this.label_orderDbName.Text = "订购库名(&O):";
            // 
            // textBox_issueDbName
            // 
            this.textBox_issueDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_issueDbName.Location = new System.Drawing.Point(188, 315);
            this.textBox_issueDbName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_issueDbName.Name = "textBox_issueDbName";
            this.textBox_issueDbName.Size = new System.Drawing.Size(316, 31);
            this.textBox_issueDbName.TabIndex = 13;
            // 
            // label_issueDbName
            // 
            this.label_issueDbName.AutoSize = true;
            this.label_issueDbName.Location = new System.Drawing.Point(22, 318);
            this.label_issueDbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_issueDbName.Name = "label_issueDbName";
            this.label_issueDbName.Size = new System.Drawing.Size(117, 21);
            this.label_issueDbName.TabIndex = 12;
            this.label_issueDbName.Text = "期库名(&I):";
            // 
            // label_marcSyntax
            // 
            this.label_marcSyntax.AutoSize = true;
            this.label_marcSyntax.Location = new System.Drawing.Point(22, 63);
            this.label_marcSyntax.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_marcSyntax.Name = "label_marcSyntax";
            this.label_marcSyntax.Size = new System.Drawing.Size(138, 21);
            this.label_marcSyntax.TabIndex = 2;
            this.label_marcSyntax.Text = "数据格式(&S):";
            // 
            // comboBox_syntax
            // 
            this.comboBox_syntax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_syntax.FormattingEnabled = true;
            this.comboBox_syntax.Items.AddRange(new object[] {
            "unimarc",
            "usmarc",
            "dc"});
            this.comboBox_syntax.Location = new System.Drawing.Point(188, 58);
            this.comboBox_syntax.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_syntax.Name = "comboBox_syntax";
            this.comboBox_syntax.Size = new System.Drawing.Size(316, 29);
            this.comboBox_syntax.TabIndex = 3;
            this.comboBox_syntax.Text = "unimarc";
            // 
            // comboBox_documentType
            // 
            this.comboBox_documentType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_documentType.FormattingEnabled = true;
            this.comboBox_documentType.Items.AddRange(new object[] {
            "book -- 图书",
            "series -- 期刊"});
            this.comboBox_documentType.Location = new System.Drawing.Point(188, 100);
            this.comboBox_documentType.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_documentType.Name = "comboBox_documentType";
            this.comboBox_documentType.Size = new System.Drawing.Size(316, 29);
            this.comboBox_documentType.TabIndex = 5;
            this.comboBox_documentType.Text = "book -- 图书";
            this.comboBox_documentType.TextChanged += new System.EventHandler(this.comboBox_usage_TextChanged);
            // 
            // label_usage
            // 
            this.label_usage.AutoSize = true;
            this.label_usage.Location = new System.Drawing.Point(22, 103);
            this.label_usage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_usage.Name = "label_usage";
            this.label_usage.Size = new System.Drawing.Size(138, 21);
            this.label_usage.TabIndex = 4;
            this.label_usage.Text = "文献类型(&D):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(354, 555);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 17;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(466, 555);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 18;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_inCirculation
            // 
            this.checkBox_inCirculation.AutoSize = true;
            this.checkBox_inCirculation.Checked = true;
            this.checkBox_inCirculation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_inCirculation.Location = new System.Drawing.Point(26, 432);
            this.checkBox_inCirculation.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_inCirculation.Name = "checkBox_inCirculation";
            this.checkBox_inCirculation.Size = new System.Drawing.Size(153, 25);
            this.checkBox_inCirculation.TabIndex = 16;
            this.checkBox_inCirculation.Text = "参与流通(&C)";
            this.checkBox_inCirculation.UseVisualStyleBackColor = true;
            // 
            // label_role
            // 
            this.label_role.AutoSize = true;
            this.label_role.Location = new System.Drawing.Point(22, 144);
            this.label_role.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_role.Name = "label_role";
            this.label_role.Size = new System.Drawing.Size(96, 21);
            this.label_role.TabIndex = 6;
            this.label_role.Text = "角色(&R):";
            // 
            // checkedComboBox_role
            // 
            this.checkedComboBox_role.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_role.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_role.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedComboBox_role.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_role.Location = new System.Drawing.Point(188, 142);
            this.checkedComboBox_role.Margin = new System.Windows.Forms.Padding(2);
            this.checkedComboBox_role.Name = "checkedComboBox_role";
            this.checkedComboBox_role.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_role.ReadOnly = false;
            this.checkedComboBox_role.Size = new System.Drawing.Size(317, 34);
            this.checkedComboBox_role.TabIndex = 7;
            this.checkedComboBox_role.DropDown += new System.EventHandler(this.checkedComboBox_role_DropDown);
            // 
            // textBox_commentDbName
            // 
            this.textBox_commentDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_commentDbName.Location = new System.Drawing.Point(188, 359);
            this.textBox_commentDbName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_commentDbName.Name = "textBox_commentDbName";
            this.textBox_commentDbName.Size = new System.Drawing.Size(316, 31);
            this.textBox_commentDbName.TabIndex = 15;
            // 
            // label_commentDbName
            // 
            this.label_commentDbName.AutoSize = true;
            this.label_commentDbName.Location = new System.Drawing.Point(22, 362);
            this.label_commentDbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_commentDbName.Name = "label_commentDbName";
            this.label_commentDbName.Size = new System.Drawing.Size(138, 21);
            this.label_commentDbName.TabIndex = 14;
            this.label_commentDbName.Text = "评注库名(&C):";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_normal);
            this.tabControl1.Controls.Add(this.tabPage_replicate);
            this.tabControl1.Location = new System.Drawing.Point(22, 21);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(540, 525);
            this.tabControl1.TabIndex = 19;
            // 
            // tabPage_normal
            // 
            this.tabPage_normal.AutoScroll = true;
            this.tabPage_normal.Controls.Add(this.checkBox_lockDbNames);
            this.tabPage_normal.Controls.Add(this.checkedComboBox_role);
            this.tabPage_normal.Controls.Add(this.textBox_commentDbName);
            this.tabPage_normal.Controls.Add(this.label_biblioDbName);
            this.tabPage_normal.Controls.Add(this.label_commentDbName);
            this.tabPage_normal.Controls.Add(this.textBox_biblioDbName);
            this.tabPage_normal.Controls.Add(this.label_entityDbName);
            this.tabPage_normal.Controls.Add(this.label_role);
            this.tabPage_normal.Controls.Add(this.textBox_entityDbName);
            this.tabPage_normal.Controls.Add(this.checkBox_inCirculation);
            this.tabPage_normal.Controls.Add(this.label_orderDbName);
            this.tabPage_normal.Controls.Add(this.textBox_orderDbName);
            this.tabPage_normal.Controls.Add(this.label_issueDbName);
            this.tabPage_normal.Controls.Add(this.comboBox_documentType);
            this.tabPage_normal.Controls.Add(this.textBox_issueDbName);
            this.tabPage_normal.Controls.Add(this.label_usage);
            this.tabPage_normal.Controls.Add(this.label_marcSyntax);
            this.tabPage_normal.Controls.Add(this.comboBox_syntax);
            this.tabPage_normal.Location = new System.Drawing.Point(4, 31);
            this.tabPage_normal.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_normal.Name = "tabPage_normal";
            this.tabPage_normal.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_normal.Size = new System.Drawing.Size(532, 490);
            this.tabPage_normal.TabIndex = 0;
            this.tabPage_normal.Text = "一般定义";
            this.tabPage_normal.UseVisualStyleBackColor = true;
            // 
            // checkBox_lockDbNames
            // 
            this.checkBox_lockDbNames.AutoSize = true;
            this.checkBox_lockDbNames.Location = new System.Drawing.Point(26, 194);
            this.checkBox_lockDbNames.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_lockDbNames.Name = "checkBox_lockDbNames";
            this.checkBox_lockDbNames.Size = new System.Drawing.Size(237, 25);
            this.checkBox_lockDbNames.TabIndex = 17;
            this.checkBox_lockDbNames.Text = "锁定数据库名关系(&L)";
            this.checkBox_lockDbNames.UseVisualStyleBackColor = true;
            this.checkBox_lockDbNames.CheckedChanged += new System.EventHandler(this.checkBox_lockDbNames_CheckedChanged);
            // 
            // tabPage_replicate
            // 
            this.tabPage_replicate.AutoScroll = true;
            this.tabPage_replicate.Controls.Add(this.comboBox_replication_centerServer);
            this.tabPage_replicate.Controls.Add(this.label9);
            this.tabPage_replicate.Controls.Add(this.label10);
            this.tabPage_replicate.Controls.Add(this.comboBox_replication_dbName);
            this.tabPage_replicate.Location = new System.Drawing.Point(4, 31);
            this.tabPage_replicate.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_replicate.Name = "tabPage_replicate";
            this.tabPage_replicate.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_replicate.Size = new System.Drawing.Size(532, 490);
            this.tabPage_replicate.TabIndex = 1;
            this.tabPage_replicate.Text = "同步";
            this.tabPage_replicate.UseVisualStyleBackColor = true;
            // 
            // comboBox_replication_centerServer
            // 
            this.comboBox_replication_centerServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_replication_centerServer.FormattingEnabled = true;
            this.comboBox_replication_centerServer.Location = new System.Drawing.Point(180, 23);
            this.comboBox_replication_centerServer.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_replication_centerServer.Name = "comboBox_replication_centerServer";
            this.comboBox_replication_centerServer.Size = new System.Drawing.Size(316, 29);
            this.comboBox_replication_centerServer.TabIndex = 8;
            this.comboBox_replication_centerServer.DropDown += new System.EventHandler(this.comboBox_replication_centerServer_DropDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 28);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(159, 21);
            this.label9.TabIndex = 4;
            this.label9.Text = "中心服务器(&C):";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 72);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(138, 21);
            this.label10.TabIndex = 6;
            this.label10.Text = "书目库名(&D):";
            // 
            // comboBox_replication_dbName
            // 
            this.comboBox_replication_dbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_replication_dbName.FormattingEnabled = true;
            this.comboBox_replication_dbName.Location = new System.Drawing.Point(180, 66);
            this.comboBox_replication_dbName.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_replication_dbName.Name = "comboBox_replication_dbName";
            this.comboBox_replication_dbName.Size = new System.Drawing.Size(316, 29);
            this.comboBox_replication_dbName.TabIndex = 7;
            this.comboBox_replication_dbName.DropDown += new System.EventHandler(this.comboBox_replication_dbName_DropDown);
            // 
            // BiblioDatabaseDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(584, 611);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BiblioDatabaseDialog";
            this.ShowInTaskbar = false;
            this.Text = "书目库";
            this.Load += new System.EventHandler(this.BiblioDatabaseDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_normal.ResumeLayout(false);
            this.tabPage_normal.PerformLayout();
            this.tabPage_replicate.ResumeLayout(false);
            this.tabPage_replicate.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_biblioDbName;
        private System.Windows.Forms.TextBox textBox_biblioDbName;
        private System.Windows.Forms.TextBox textBox_entityDbName;
        private System.Windows.Forms.Label label_entityDbName;
        private System.Windows.Forms.TextBox textBox_orderDbName;
        private System.Windows.Forms.Label label_orderDbName;
        private System.Windows.Forms.TextBox textBox_issueDbName;
        private System.Windows.Forms.Label label_issueDbName;
        private System.Windows.Forms.Label label_marcSyntax;
        private System.Windows.Forms.ComboBox comboBox_syntax;
        private System.Windows.Forms.ComboBox comboBox_documentType;
        private System.Windows.Forms.Label label_usage;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_inCirculation;
        private System.Windows.Forms.Label label_role;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_role;
        private System.Windows.Forms.TextBox textBox_commentDbName;
        private System.Windows.Forms.Label label_commentDbName;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_normal;
        private System.Windows.Forms.TabPage tabPage_replicate;
        private System.Windows.Forms.ComboBox comboBox_replication_centerServer;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBox_replication_dbName;
        private System.Windows.Forms.CheckBox checkBox_lockDbNames;
    }
}