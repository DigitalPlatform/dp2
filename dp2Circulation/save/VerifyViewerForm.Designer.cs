namespace dp2Circulation
{
    partial class VerifyViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerifyViewerForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dock = new System.Windows.Forms.ToolStripButton();
            this.textBox_verifyResult = new System.Windows.Forms.TextBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dock});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(284, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_dock
            // 
            this.toolStripButton_dock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_dock.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dock.Image")));
            this.toolStripButton_dock.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_dock.Name = "toolStripButton_dock";
            this.toolStripButton_dock.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_dock.Text = "停靠到固定面板";
            this.toolStripButton_dock.Click += new System.EventHandler(this.toolStripButton_dock_Click);
            // 
            // textBox_verifyResult
            // 
            this.textBox_verifyResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_verifyResult.Location = new System.Drawing.Point(0, 25);
            this.textBox_verifyResult.Multiline = true;
            this.textBox_verifyResult.Name = "textBox_verifyResult";
            this.textBox_verifyResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_verifyResult.Size = new System.Drawing.Size(284, 239);
            this.textBox_verifyResult.TabIndex = 2;
            this.textBox_verifyResult.WordWrap = false;
            this.textBox_verifyResult.DoubleClick += new System.EventHandler(this.textBox_verifyResult_DoubleClick);
            // 
            // VerifyViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.Controls.Add(this.textBox_verifyResult);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VerifyViewerForm";
            this.ShowInTaskbar = false;
            this.Text = "VerifyViewerForm";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_dock;
        private System.Windows.Forms.TextBox textBox_verifyResult;
    }
}