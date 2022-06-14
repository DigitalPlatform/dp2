namespace dp2ManageCenter
{
    partial class ServerFileSystemForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerFileSystemForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.kernelResTree1 = new DigitalPlatform.CirculationClient.KernelResTree();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_refresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 38);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_refresh.Text = "刷新";
            this.toolStripButton_refresh.Click += new System.EventHandler(this.toolStripButton_refresh_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_message});
            this.statusStrip1.Location = new System.Drawing.Point(0, 413);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 37);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // kernelResTree1
            // 
            this.kernelResTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kernelResTree1.ImageIndex = 0;
            this.kernelResTree1.Lang = null;
            this.kernelResTree1.Location = new System.Drawing.Point(0, 38);
            this.kernelResTree1.Name = "kernelResTree1";
            this.kernelResTree1.SelectedImageIndex = 0;
            this.kernelResTree1.Size = new System.Drawing.Size(800, 375);
            this.kernelResTree1.TabIndex = 1;
            this.kernelResTree1.UploadFiles += new DigitalPlatform.CirculationClient.UploadFilesEventHandler(this.kernelResTree1_UploadFiles);
            // this.kernelResTree1.DownloadFiles += new DigitalPlatform.CirculationClient.DownloadFilesEventHandler(this.kernelResTree1_DownloadFiles);
            this.kernelResTree1.GetChannel += new DigitalPlatform.LibraryClient.GetChannelEventHandler(this.kernelResTree1_GetChannel);
            this.kernelResTree1.ReturnChannel += new DigitalPlatform.LibraryClient.ReturnChannelEventHandler(this.kernelResTree1_ReturnChannel);
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(228, 28);
            this.toolStripStatusLabel_message.Text = "toolStripStatusLabel1";
            // 
            // ServerFileSystemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.kernelResTree1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "ServerFileSystemForm";
            this.ShowIcon = false;
            this.Text = "ServerFileSystemForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerFileSystemForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ServerFileSystemForm_FormClosed);
            this.Load += new System.EventHandler(this.ServerFileSystemForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private DigitalPlatform.CirculationClient.KernelResTree kernelResTree1;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
    }
}