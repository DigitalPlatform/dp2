namespace dp2Circulation
{
    partial class RfidPatronCardDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RfidPatronCardDialog));
            this.toolStrip_rfid = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_saveRfid = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_loadRfid = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_rfidArea = new System.Windows.Forms.SplitContainer();
            this.chipEditor_editing = new DigitalPlatform.RFID.UI.ChipEditor();
            this.chipEditor_existing = new DigitalPlatform.RFID.UI.ChipEditor();
            this.toolStrip_rfid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rfidArea)).BeginInit();
            this.splitContainer_rfidArea.Panel1.SuspendLayout();
            this.splitContainer_rfidArea.Panel2.SuspendLayout();
            this.splitContainer_rfidArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip_rfid
            // 
            this.toolStrip_rfid.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip_rfid.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_rfid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_saveRfid,
            this.toolStripButton_loadRfid});
            this.toolStrip_rfid.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_rfid.Name = "toolStrip_rfid";
            this.toolStrip_rfid.Size = new System.Drawing.Size(56, 579);
            this.toolStrip_rfid.TabIndex = 0;
            this.toolStrip_rfid.Text = "toolStrip2";
            // 
            // toolStripButton_saveRfid
            // 
            this.toolStripButton_saveRfid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_saveRfid.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_saveRfid.Image")));
            this.toolStripButton_saveRfid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_saveRfid.Name = "toolStripButton_saveRfid";
            this.toolStripButton_saveRfid.Size = new System.Drawing.Size(51, 28);
            this.toolStripButton_saveRfid.Text = "写入标签";
            this.toolStripButton_saveRfid.Click += new System.EventHandler(this.toolStripButton_saveRfid_Click);
            // 
            // toolStripButton_loadRfid
            // 
            this.toolStripButton_loadRfid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_loadRfid.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loadRfid.Image")));
            this.toolStripButton_loadRfid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loadRfid.Name = "toolStripButton_loadRfid";
            this.toolStripButton_loadRfid.Size = new System.Drawing.Size(37, 28);
            this.toolStripButton_loadRfid.Text = "装载标签";
            this.toolStripButton_loadRfid.Click += new System.EventHandler(this.toolStripButton_loadRfid_Click);
            // 
            // splitContainer_rfidArea
            // 
            this.splitContainer_rfidArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_rfidArea.Location = new System.Drawing.Point(56, 0);
            this.splitContainer_rfidArea.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_rfidArea.Name = "splitContainer_rfidArea";
            // 
            // splitContainer_rfidArea.Panel1
            // 
            this.splitContainer_rfidArea.Panel1.Controls.Add(this.chipEditor_existing);
            // 
            // splitContainer_rfidArea.Panel2
            // 
            this.splitContainer_rfidArea.Panel2.Controls.Add(this.chipEditor_editing);
            this.splitContainer_rfidArea.Size = new System.Drawing.Size(779, 579);
            this.splitContainer_rfidArea.SplitterDistance = 372;
            this.splitContainer_rfidArea.SplitterWidth = 8;
            this.splitContainer_rfidArea.TabIndex = 0;
            // 
            // chipEditor_editing
            // 
            this.chipEditor_editing.AutoSize = true;
            this.chipEditor_editing.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.chipEditor_editing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chipEditor_editing.Location = new System.Drawing.Point(0, 0);
            this.chipEditor_editing.LogicChipItem = null;
            this.chipEditor_editing.Margin = new System.Windows.Forms.Padding(5);
            this.chipEditor_editing.Name = "chipEditor_editing";
            this.chipEditor_editing.Size = new System.Drawing.Size(399, 579);
            this.chipEditor_editing.TabIndex = 1;
            this.chipEditor_editing.TitleVisible = true;
            // 
            // chipEditor_existing
            // 
            this.chipEditor_existing.AutoSize = true;
            this.chipEditor_existing.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.chipEditor_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chipEditor_existing.Location = new System.Drawing.Point(0, 0);
            this.chipEditor_existing.LogicChipItem = null;
            this.chipEditor_existing.Margin = new System.Windows.Forms.Padding(5);
            this.chipEditor_existing.Name = "chipEditor_existing";
            this.chipEditor_existing.Size = new System.Drawing.Size(372, 579);
            this.chipEditor_existing.TabIndex = 0;
            this.chipEditor_existing.TitleVisible = true;
            // 
            // RfidPatronCardDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(835, 579);
            this.Controls.Add(this.splitContainer_rfidArea);
            this.Controls.Add(this.toolStrip_rfid);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "RfidPatronCardDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Rfid 读者卡";
            this.toolStrip_rfid.ResumeLayout(false);
            this.toolStrip_rfid.PerformLayout();
            this.splitContainer_rfidArea.Panel1.ResumeLayout(false);
            this.splitContainer_rfidArea.Panel1.PerformLayout();
            this.splitContainer_rfidArea.Panel2.ResumeLayout(false);
            this.splitContainer_rfidArea.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rfidArea)).EndInit();
            this.splitContainer_rfidArea.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip_rfid;
        private System.Windows.Forms.ToolStripButton toolStripButton_saveRfid;
        private System.Windows.Forms.ToolStripButton toolStripButton_loadRfid;
        private System.Windows.Forms.SplitContainer splitContainer_rfidArea;
        private DigitalPlatform.RFID.UI.ChipEditor chipEditor_existing;
        private DigitalPlatform.RFID.UI.ChipEditor chipEditor_editing;
    }
}