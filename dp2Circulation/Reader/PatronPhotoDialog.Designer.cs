namespace dp2Circulation
{
    partial class PatronPhotoDialog
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

            /*
            if (this.qrRecognitionControl1 != null)
                this.qrRecognitionControl1.Dispose();
            */

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatronPhotoDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_getAndClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_freeze = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_ratate = new System.Windows.Forms.ToolStripButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(0, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(788, 458);
            this.panel1.TabIndex = 0;
            this.panel1.SizeChanged += new System.EventHandler(this.panel1_SizeChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_getAndClose,
            this.toolStripButton_freeze,
            this.toolStripButton_ratate});
            this.toolStrip1.Location = new System.Drawing.Point(0, 461);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(788, 47);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_getAndClose
            // 
            this.toolStripButton_getAndClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_getAndClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_getAndClose.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_getAndClose.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_getAndClose.Image")));
            this.toolStripButton_getAndClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_getAndClose.Name = "toolStripButton_getAndClose";
            this.toolStripButton_getAndClose.Size = new System.Drawing.Size(133, 41);
            this.toolStripButton_getAndClose.Text = "获取图像";
            this.toolStripButton_getAndClose.ToolTipText = "获取图像，自动关闭对话框";
            this.toolStripButton_getAndClose.Click += new System.EventHandler(this.toolStripButton_getAndClose_Click);
            // 
            // toolStripButton_freeze
            // 
            this.toolStripButton_freeze.CheckOnClick = true;
            this.toolStripButton_freeze.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_freeze.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_freeze.Image")));
            this.toolStripButton_freeze.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_freeze.Name = "toolStripButton_freeze";
            this.toolStripButton_freeze.Size = new System.Drawing.Size(58, 41);
            this.toolStripButton_freeze.Text = "冻结";
            this.toolStripButton_freeze.ToolTipText = "冻结 / 继续捕获";
            this.toolStripButton_freeze.CheckedChanged += new System.EventHandler(this.toolStripButton_freeze_CheckedChanged);
            // 
            // toolStripButton_ratate
            // 
            this.toolStripButton_ratate.Enabled = false;
            this.toolStripButton_ratate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_ratate.Image")));
            this.toolStripButton_ratate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ratate.Name = "toolStripButton_ratate";
            this.toolStripButton_ratate.Size = new System.Drawing.Size(86, 41);
            this.toolStripButton_ratate.Text = "旋转";
            this.toolStripButton_ratate.ToolTipText = "顺指针旋转 90 度 (冻结时可用)";
            this.toolStripButton_ratate.Click += new System.EventHandler(this.toolStripButton_ratate_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(788, 458);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // PatronPhotoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(788, 508);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "PatronPhotoDialog";
            this.Text = "从摄像头获取图像";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraPhotoDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CameraPhotoDialog_FormClosed);
            this.Load += new System.EventHandler(this.CameraPhotoDialog_Load);
            this.panel1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_getAndClose;
        private System.Windows.Forms.ToolStripButton toolStripButton_freeze;
        private System.Windows.Forms.ToolStripButton toolStripButton_ratate;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}