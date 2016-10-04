namespace DigitalPlatform.Drawing
{
    partial class CameraPhotoDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CameraPhotoDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_getAndClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_freeze = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_ratate = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Location = new System.Drawing.Point(0, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(430, 262);
            this.panel1.TabIndex = 0;
            this.panel1.SizeChanged += new System.EventHandler(this.panel1_SizeChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_getAndClose,
            this.toolStripButton_freeze,
            this.toolStripButton_ratate});
            this.toolStrip1.Location = new System.Drawing.Point(0, 261);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(430, 29);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_getAndClose
            // 
            this.toolStripButton_getAndClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_getAndClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_getAndClose.Enabled = false;
            this.toolStripButton_getAndClose.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_getAndClose.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_getAndClose.Image")));
            this.toolStripButton_getAndClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_getAndClose.Name = "toolStripButton_getAndClose";
            this.toolStripButton_getAndClose.Size = new System.Drawing.Size(78, 26);
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
            this.toolStripButton_freeze.Size = new System.Drawing.Size(36, 26);
            this.toolStripButton_freeze.Text = "冻结";
            this.toolStripButton_freeze.ToolTipText = "冻结 / 继续捕获";
            this.toolStripButton_freeze.CheckedChanged += new System.EventHandler(this.toolStripButton_freeze_CheckedChanged);
            this.toolStripButton_freeze.Click += new System.EventHandler(this.toolStripButton_freeze_Click);
            // 
            // toolStripButton_ratate
            // 
            this.toolStripButton_ratate.Enabled = false;
            this.toolStripButton_ratate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_ratate.Image")));
            this.toolStripButton_ratate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ratate.Name = "toolStripButton_ratate";
            this.toolStripButton_ratate.Size = new System.Drawing.Size(52, 26);
            this.toolStripButton_ratate.Text = "旋转";
            this.toolStripButton_ratate.ToolTipText = "顺指针旋转 90 度 (冻结时可用)";
            this.toolStripButton_ratate.Click += new System.EventHandler(this.toolStripButton_ratate_Click);
            // 
            // CameraPhotoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(430, 290);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CameraPhotoDialog";
            this.Text = "从摄像头获取图像";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraPhotoDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CameraPhotoDialog_FormClosed);
            this.Load += new System.EventHandler(this.CameraPhotoDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_getAndClose;
        private System.Windows.Forms.ToolStripButton toolStripButton_freeze;
        private System.Windows.Forms.ToolStripButton toolStripButton_ratate;
    }
}