namespace dp2Catalog
{
    partial class SaveRecordDlg
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveRecordDlg));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_DTLP = new System.Windows.Forms.TabPage();
            this.button_dtlp_append = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dtlpResDirControl1 = new DigitalPlatform.DTLP.DtlpResDirControl();
            this.textBox_dtlpRecPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_dp2 = new System.Windows.Forms.TabPage();
            this.button_dp2_append = new System.Windows.Forms.Button();
            this.dp2ResTree1 = new DigitalPlatform.CirculationClient.dp2ResTree();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_dp2RecPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_unionCatalog = new System.Windows.Forms.TabPage();
            this.button_unionCatalog_append = new System.Windows.Forms.Button();
            this.textBox_unionCatalogRecPath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_DTLP.SuspendLayout();
            this.tabPage_dp2.SuspendLayout();
            this.tabPage_unionCatalog.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(284, 238);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(344, 238);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_DTLP);
            this.tabControl_main.Controls.Add(this.tabPage_dp2);
            this.tabControl_main.Controls.Add(this.tabPage_unionCatalog);
            this.tabControl_main.Location = new System.Drawing.Point(10, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(391, 223);
            this.tabControl_main.TabIndex = 2;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_DTLP
            // 
            this.tabPage_DTLP.Controls.Add(this.button_dtlp_append);
            this.tabPage_DTLP.Controls.Add(this.label2);
            this.tabPage_DTLP.Controls.Add(this.dtlpResDirControl1);
            this.tabPage_DTLP.Controls.Add(this.textBox_dtlpRecPath);
            this.tabPage_DTLP.Controls.Add(this.label1);
            this.tabPage_DTLP.Location = new System.Drawing.Point(4, 22);
            this.tabPage_DTLP.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_DTLP.Name = "tabPage_DTLP";
            this.tabPage_DTLP.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_DTLP.Size = new System.Drawing.Size(383, 197);
            this.tabPage_DTLP.TabIndex = 0;
            this.tabPage_DTLP.Text = "DTLP";
            this.tabPage_DTLP.UseVisualStyleBackColor = true;
            // 
            // button_dtlp_append
            // 
            this.button_dtlp_append.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_dtlp_append.Location = new System.Drawing.Point(313, 174);
            this.button_dtlp_append.Margin = new System.Windows.Forms.Padding(2);
            this.button_dtlp_append.Name = "button_dtlp_append";
            this.button_dtlp_append.Size = new System.Drawing.Size(68, 22);
            this.button_dtlp_append.TabIndex = 10;
            this.button_dtlp_append.Text = "追加(&A)";
            this.button_dtlp_append.UseVisualStyleBackColor = true;
            this.button_dtlp_append.Click += new System.EventHandler(this.button_dtlp_append_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "保存到:";
            // 
            // dtlpResDirControl1
            // 
            this.dtlpResDirControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dtlpResDirControl1.BackColor = System.Drawing.SystemColors.Window;
            this.dtlpResDirControl1.HideSelection = false;
            this.dtlpResDirControl1.ImageIndex = 0;
            this.dtlpResDirControl1.Location = new System.Drawing.Point(4, 26);
            this.dtlpResDirControl1.Margin = new System.Windows.Forms.Padding(2);
            this.dtlpResDirControl1.Name = "dtlpResDirControl1";
            this.dtlpResDirControl1.SelectedImageIndex = 0;
            this.dtlpResDirControl1.SelectedPath1 = null;
            this.dtlpResDirControl1.Size = new System.Drawing.Size(377, 144);
            this.dtlpResDirControl1.TabIndex = 2;
            this.dtlpResDirControl1.ItemSelected += new DigitalPlatform.DTLP.ItemSelectedEventHandle(this.dtlpResDirControl1_ItemSelected);
            this.dtlpResDirControl1.GetItemTextStyle += new DigitalPlatform.DTLP.GetItemTextStyleEventHandle(this.dtlpResDirControl1_GetItemTextStyle);
            // 
            // textBox_dtlpRecPath
            // 
            this.textBox_dtlpRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dtlpRecPath.Location = new System.Drawing.Point(86, 174);
            this.textBox_dtlpRecPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dtlpRecPath.Name = "textBox_dtlpRecPath";
            this.textBox_dtlpRecPath.Size = new System.Drawing.Size(224, 21);
            this.textBox_dtlpRecPath.TabIndex = 1;
            this.textBox_dtlpRecPath.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_dtlpRecPath_Validating);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 177);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "记录路径(&P):";
            // 
            // tabPage_dp2
            // 
            this.tabPage_dp2.Controls.Add(this.button_dp2_append);
            this.tabPage_dp2.Controls.Add(this.dp2ResTree1);
            this.tabPage_dp2.Controls.Add(this.label3);
            this.tabPage_dp2.Controls.Add(this.textBox_dp2RecPath);
            this.tabPage_dp2.Controls.Add(this.label4);
            this.tabPage_dp2.Location = new System.Drawing.Point(4, 22);
            this.tabPage_dp2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_dp2.Name = "tabPage_dp2";
            this.tabPage_dp2.Size = new System.Drawing.Size(383, 197);
            this.tabPage_dp2.TabIndex = 1;
            this.tabPage_dp2.Text = "dp2library";
            this.tabPage_dp2.UseVisualStyleBackColor = true;
            // 
            // button_dp2_append
            // 
            this.button_dp2_append.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_dp2_append.Location = new System.Drawing.Point(313, 174);
            this.button_dp2_append.Margin = new System.Windows.Forms.Padding(2);
            this.button_dp2_append.Name = "button_dp2_append";
            this.button_dp2_append.Size = new System.Drawing.Size(68, 22);
            this.button_dp2_append.TabIndex = 9;
            this.button_dp2_append.Text = "追加(&A)";
            this.button_dp2_append.UseVisualStyleBackColor = true;
            this.button_dp2_append.Click += new System.EventHandler(this.button_dp2_append_Click);
            // 
            // dp2ResTree1
            // 
            this.dp2ResTree1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dp2ResTree1.HideSelection = false;
            this.dp2ResTree1.ImageIndex = 0;
            this.dp2ResTree1.Location = new System.Drawing.Point(5, 24);
            this.dp2ResTree1.Margin = new System.Windows.Forms.Padding(2);
            this.dp2ResTree1.Name = "dp2ResTree1";
            this.dp2ResTree1.SelectedImageIndex = 0;
            this.dp2ResTree1.Size = new System.Drawing.Size(377, 144);
            this.dp2ResTree1.TabIndex = 8;
            this.dp2ResTree1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dp2ResTree1_AfterSelect);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "保存到:";
            // 
            // textBox_dp2RecPath
            // 
            this.textBox_dp2RecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2RecPath.Location = new System.Drawing.Point(86, 174);
            this.textBox_dp2RecPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dp2RecPath.Name = "textBox_dp2RecPath";
            this.textBox_dp2RecPath.Size = new System.Drawing.Size(224, 21);
            this.textBox_dp2RecPath.TabIndex = 5;
            this.textBox_dp2RecPath.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_dp2RecPath_Validating);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 177);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "记录路径(&P):";
            // 
            // tabPage_unionCatalog
            // 
            this.tabPage_unionCatalog.Controls.Add(this.button_unionCatalog_append);
            this.tabPage_unionCatalog.Controls.Add(this.textBox_unionCatalogRecPath);
            this.tabPage_unionCatalog.Controls.Add(this.label5);
            this.tabPage_unionCatalog.Location = new System.Drawing.Point(4, 22);
            this.tabPage_unionCatalog.Name = "tabPage_unionCatalog";
            this.tabPage_unionCatalog.Size = new System.Drawing.Size(383, 197);
            this.tabPage_unionCatalog.TabIndex = 2;
            this.tabPage_unionCatalog.Text = "Union Catalog";
            this.tabPage_unionCatalog.UseVisualStyleBackColor = true;
            // 
            // button_unionCatalog_append
            // 
            this.button_unionCatalog_append.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_unionCatalog_append.Location = new System.Drawing.Point(313, 174);
            this.button_unionCatalog_append.Margin = new System.Windows.Forms.Padding(2);
            this.button_unionCatalog_append.Name = "button_unionCatalog_append";
            this.button_unionCatalog_append.Size = new System.Drawing.Size(68, 22);
            this.button_unionCatalog_append.TabIndex = 12;
            this.button_unionCatalog_append.Text = "追加(&A)";
            this.button_unionCatalog_append.UseVisualStyleBackColor = true;
            this.button_unionCatalog_append.Click += new System.EventHandler(this.button_unionCatalog_append_Click);
            // 
            // textBox_unionCatalogRecPath
            // 
            this.textBox_unionCatalogRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unionCatalogRecPath.Location = new System.Drawing.Point(86, 174);
            this.textBox_unionCatalogRecPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_unionCatalogRecPath.Name = "textBox_unionCatalogRecPath";
            this.textBox_unionCatalogRecPath.Size = new System.Drawing.Size(224, 21);
            this.textBox_unionCatalogRecPath.TabIndex = 11;
            this.textBox_unionCatalogRecPath.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_unionCatalogRecPath_Validating);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 177);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "记录路径(&P):";
            // 
            // SaveRecordDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 270);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SaveRecordDlg";
            this.ShowInTaskbar = false;
            this.Text = "保存记录";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SaveRecordDlg_FormClosed);
            this.Load += new System.EventHandler(this.SaveRecordDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_DTLP.ResumeLayout(false);
            this.tabPage_DTLP.PerformLayout();
            this.tabPage_dp2.ResumeLayout(false);
            this.tabPage_dp2.PerformLayout();
            this.tabPage_unionCatalog.ResumeLayout(false);
            this.tabPage_unionCatalog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_DTLP;
        private System.Windows.Forms.TextBox textBox_dtlpRecPath;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.DTLP.DtlpResDirControl dtlpResDirControl1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage_dp2;
        private DigitalPlatform.CirculationClient.dp2ResTree dp2ResTree1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_dp2RecPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_dp2_append;
        private System.Windows.Forms.Button button_dtlp_append;
        private System.Windows.Forms.TabPage tabPage_unionCatalog;
        private System.Windows.Forms.Button button_unionCatalog_append;
        private System.Windows.Forms.TextBox textBox_unionCatalogRecPath;
        private System.Windows.Forms.Label label5;
    }
}