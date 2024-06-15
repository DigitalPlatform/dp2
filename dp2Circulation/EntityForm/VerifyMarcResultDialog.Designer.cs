namespace dp2Circulation
{
    partial class VerifyMarcResultDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerifyMarcResultDialog));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_acceptChangedMarc = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_copyLeftToClipboard = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_copyRightToClipboard = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_records = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_source = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_targetOld = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_targetNew = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.toolStrip_records.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(15, 48);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(891, 361);
            this.webBrowser1.TabIndex = 1;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(768, 428);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_acceptChangedMarc
            // 
            this.button_acceptChangedMarc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_acceptChangedMarc.Location = new System.Drawing.Point(450, 428);
            this.button_acceptChangedMarc.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_acceptChangedMarc.Name = "button_acceptChangedMarc";
            this.button_acceptChangedMarc.Size = new System.Drawing.Size(306, 40);
            this.button_acceptChangedMarc.TabIndex = 5;
            this.button_acceptChangedMarc.Text = "接受修改后的记录";
            this.button_acceptChangedMarc.UseVisualStyleBackColor = true;
            this.button_acceptChangedMarc.Click += new System.EventHandler(this.button_acceptChangedMarc_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_copyLeftToClipboard,
            this.toolStripButton_copyRightToClipboard});
            this.toolStrip1.Location = new System.Drawing.Point(9, 409);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(305, 38);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_copyLeftToClipboard
            // 
            this.toolStripButton_copyLeftToClipboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_copyLeftToClipboard.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_copyLeftToClipboard.Image")));
            this.toolStripButton_copyLeftToClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_copyLeftToClipboard.Name = "toolStripButton_copyLeftToClipboard";
            this.toolStripButton_copyLeftToClipboard.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_copyLeftToClipboard.Text = "复制左侧记录";
            this.toolStripButton_copyLeftToClipboard.ToolTipText = "复制左侧记录到剪贴板";
            this.toolStripButton_copyLeftToClipboard.Click += new System.EventHandler(this.toolStripButton_copyLeftToClipboard_Click);
            // 
            // toolStripButton_copyRightToClipboard
            // 
            this.toolStripButton_copyRightToClipboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_copyRightToClipboard.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_copyRightToClipboard.Image")));
            this.toolStripButton_copyRightToClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_copyRightToClipboard.Name = "toolStripButton_copyRightToClipboard";
            this.toolStripButton_copyRightToClipboard.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_copyRightToClipboard.Text = "复制右侧记录";
            this.toolStripButton_copyRightToClipboard.ToolTipText = "复制右侧记录到剪贴板";
            this.toolStripButton_copyRightToClipboard.Click += new System.EventHandler(this.toolStripButton_copyRightToClipboard_Click);
            // 
            // toolStrip_records
            // 
            this.toolStrip_records.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip_records.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_source,
            this.toolStripSeparator1,
            this.toolStripButton_targetOld,
            this.toolStripSeparator2,
            this.toolStripButton_targetNew});
            this.toolStrip_records.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_records.Name = "toolStrip_records";
            this.toolStrip_records.Size = new System.Drawing.Size(921, 38);
            this.toolStrip_records.TabIndex = 8;
            this.toolStrip_records.Text = "toolStrip2";
            // 
            // toolStripButton_source
            // 
            this.toolStripButton_source.Checked = true;
            this.toolStripButton_source.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton_source.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_source.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_source.Image")));
            this.toolStripButton_source.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_source.Name = "toolStripButton_source";
            this.toolStripButton_source.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_source.Text = "源";
            this.toolStripButton_source.Click += new System.EventHandler(this.toolStripButton_source_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_targetOld
            // 
            this.toolStripButton_targetOld.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_targetOld.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_targetOld.Image")));
            this.toolStripButton_targetOld.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_targetOld.Name = "toolStripButton_targetOld";
            this.toolStripButton_targetOld.Size = new System.Drawing.Size(135, 32);
            this.toolStripButton_targetOld.Text = "目标[已存在]";
            this.toolStripButton_targetOld.Click += new System.EventHandler(this.toolStripButton_source_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_targetNew
            // 
            this.toolStripButton_targetNew.Checked = true;
            this.toolStripButton_targetNew.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton_targetNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_targetNew.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_targetNew.Image")));
            this.toolStripButton_targetNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_targetNew.Name = "toolStripButton_targetNew";
            this.toolStripButton_targetNew.Size = new System.Drawing.Size(93, 32);
            this.toolStripButton_targetNew.Text = "目标[新]";
            this.toolStripButton_targetNew.Click += new System.EventHandler(this.toolStripButton_source_Click);
            // 
            // VerifyMarcResultDialog
            // 
            this.AcceptButton = this.button_acceptChangedMarc;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(921, 482);
            this.Controls.Add(this.toolStrip_records);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_acceptChangedMarc);
            this.Controls.Add(this.webBrowser1);
            this.Name = "VerifyMarcResultDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "MARC 记录被修改";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VerifyMarcResultDialog_FormClosed);
            this.Load += new System.EventHandler(this.VerifyMarcResultDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip_records.ResumeLayout(false);
            this.toolStrip_records.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        internal System.Windows.Forms.Button button_Cancel;
        internal System.Windows.Forms.Button button_acceptChangedMarc;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_copyLeftToClipboard;
        private System.Windows.Forms.ToolStripButton toolStripButton_copyRightToClipboard;
        private System.Windows.Forms.ToolStrip toolStrip_records;
        private System.Windows.Forms.ToolStripButton toolStripButton_source;
        private System.Windows.Forms.ToolStripButton toolStripButton_targetOld;
        private System.Windows.Forms.ToolStripButton toolStripButton_targetNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}