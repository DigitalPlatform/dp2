namespace dp2Circulation
{
    partial class BatchOrderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BatchOrderForm));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_newOrder = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_deleteOrder = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_save = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 25);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(512, 280);
            this.webBrowser1.TabIndex = 2;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_refresh,
            this.toolStripButton_newOrder,
            this.toolStripButton_deleteOrder,
            this.toolStripButton_save});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(512, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_refresh.Text = "刷新";
            // 
            // toolStripButton_newOrder
            // 
            this.toolStripButton_newOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_newOrder.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newOrder.Image")));
            this.toolStripButton_newOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newOrder.Name = "toolStripButton_newOrder";
            this.toolStripButton_newOrder.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton_newOrder.Text = "新订购";
            this.toolStripButton_newOrder.Click += new System.EventHandler(this.toolStripButton_newOrder_Click);
            // 
            // toolStripButton_deleteOrder
            // 
            this.toolStripButton_deleteOrder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_deleteOrder.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_deleteOrder.Image")));
            this.toolStripButton_deleteOrder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_deleteOrder.Name = "toolStripButton_deleteOrder";
            this.toolStripButton_deleteOrder.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_deleteOrder.Text = "删除";
            this.toolStripButton_deleteOrder.Click += new System.EventHandler(this.toolStripButton_deleteOrder_Click);
            // 
            // toolStripButton_save
            // 
            this.toolStripButton_save.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_save.Enabled = false;
            this.toolStripButton_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_save.Image")));
            this.toolStripButton_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_save.Name = "toolStripButton_save";
            this.toolStripButton_save.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_save.Text = "保存";
            this.toolStripButton_save.Click += new System.EventHandler(this.toolStripButton_save_Click);
            // 
            // BatchOrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 305);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "BatchOrderForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "批订购";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BatchOrderForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BatchOrderForm_FormClosed);
            this.Load += new System.EventHandler(this.BatchOrderForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.ToolStripButton toolStripButton_newOrder;
        private System.Windows.Forms.ToolStripButton toolStripButton_deleteOrder;
        private System.Windows.Forms.ToolStripButton toolStripButton_save;
    }
}