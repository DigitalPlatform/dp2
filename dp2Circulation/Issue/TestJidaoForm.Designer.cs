namespace dp2Circulation
{
    partial class TestJidaoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestJidaoForm));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBox_dataSource = new System.Windows.Forms.ToolStripComboBox();
            this.jidaoControl1 = new UpgradeUtil.JidaoControl();
            this.toolStripButton_upgrade = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_check = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(408, 265);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(327, 265);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripComboBox_dataSource,
            this.toolStripSeparator1,
            this.toolStripButton_upgrade,
            this.toolStripSeparator2,
            this.toolStripButton_check});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(495, 26);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(43, 23);
            this.toolStripLabel1.Text = "来源:";
            // 
            // toolStripComboBox_dataSource
            // 
            this.toolStripComboBox_dataSource.Name = "toolStripComboBox_dataSource";
            this.toolStripComboBox_dataSource.Size = new System.Drawing.Size(121, 26);
            this.toolStripComboBox_dataSource.DropDown += new System.EventHandler(this.toolStripComboBox_dataSource_DropDown);
            this.toolStripComboBox_dataSource.DropDownClosed += new System.EventHandler(this.toolStripComboBox_dataSource_DropDownClosed);
            this.toolStripComboBox_dataSource.TextChanged += new System.EventHandler(this.toolStripComboBox_dataSource_TextChanged);
            // 
            // jidaoControl1
            // 
            this.jidaoControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.jidaoControl1.AutoScroll = true;
            this.jidaoControl1.BackColor = System.Drawing.SystemColors.Window;
            this.jidaoControl1.Location = new System.Drawing.Point(13, 28);
            this.jidaoControl1.Name = "jidaoControl1";
            this.jidaoControl1.Size = new System.Drawing.Size(470, 231);
            this.jidaoControl1.TabIndex = 7;
            this.jidaoControl1.Text = "jidaoControl1";
            // 
            // toolStripButton_upgrade
            // 
            this.toolStripButton_upgrade.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_upgrade.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_upgrade.Image")));
            this.toolStripButton_upgrade.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_upgrade.Name = "toolStripButton_upgrade";
            this.toolStripButton_upgrade.Size = new System.Drawing.Size(42, 23);
            this.toolStripButton_upgrade.Text = "升级";
            this.toolStripButton_upgrade.Click += new System.EventHandler(this.toolStripButton_upgrade_Click);
            // 
            // toolStripButton_check
            // 
            this.toolStripButton_check.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_check.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_check.Image")));
            this.toolStripButton_check.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_check.Name = "toolStripButton_check";
            this.toolStripButton_check.Size = new System.Drawing.Size(42, 23);
            this.toolStripButton_check.Text = "检查";
            this.toolStripButton_check.Click += new System.EventHandler(this.toolStripButton_check_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 26);
            // 
            // TestJidaoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 305);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.jidaoControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "TestJidaoForm";
            this.Text = "TestJidaoForm";
            this.Load += new System.EventHandler(this.TestJidaoForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private UpgradeUtil.JidaoControl jidaoControl1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_dataSource;
        private System.Windows.Forms.ToolStripButton toolStripButton_upgrade;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_check;
    }
}