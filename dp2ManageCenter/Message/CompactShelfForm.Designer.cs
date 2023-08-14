namespace dp2ManageCenter.Message
{
    partial class CompactShelfForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompactShelfForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_selectAccount = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton_utility = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_clearConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_setControlParameters = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox_codeExpireLength = new System.Windows.Forms.ToolStripTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label_code = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_selectAccount,
            this.toolStripSeparator1,
            this.toolStripDropDownButton_utility,
            this.toolStripButton_setControlParameters,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.toolStripTextBox_codeExpireLength});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 44);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_selectAccount
            // 
            this.toolStripButton_selectAccount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectAccount.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectAccount.Image")));
            this.toolStripButton_selectAccount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectAccount.Name = "toolStripButton_selectAccount";
            this.toolStripButton_selectAccount.Size = new System.Drawing.Size(142, 38);
            this.toolStripButton_selectAccount.Text = "选择书架账户";
            this.toolStripButton_selectAccount.Click += new System.EventHandler(this.toolStripButton_selectAccount_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripDropDownButton_utility
            // 
            this.toolStripDropDownButton_utility.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_utility.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_clearConnection});
            this.toolStripDropDownButton_utility.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_utility.Image")));
            this.toolStripDropDownButton_utility.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_utility.Name = "toolStripDropDownButton_utility";
            this.toolStripDropDownButton_utility.Size = new System.Drawing.Size(48, 38);
            this.toolStripDropDownButton_utility.Text = "...";
            // 
            // ToolStripMenuItem_clearConnection
            // 
            this.ToolStripMenuItem_clearConnection.Name = "ToolStripMenuItem_clearConnection";
            this.ToolStripMenuItem_clearConnection.Size = new System.Drawing.Size(297, 40);
            this.ToolStripMenuItem_clearConnection.Text = "ClearConnection";
            // 
            // toolStripButton_setControlParameters
            // 
            this.toolStripButton_setControlParameters.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_setControlParameters.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_setControlParameters.Image")));
            this.toolStripButton_setControlParameters.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_setControlParameters.Name = "toolStripButton_setControlParameters";
            this.toolStripButton_setControlParameters.Size = new System.Drawing.Size(142, 38);
            this.toolStripButton_setControlParameters.Text = "设置控制参数";
            this.toolStripButton_setControlParameters.Click += new System.EventHandler(this.toolStripButton_setControlParameters_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(117, 38);
            this.toolStripLabel1.Text = "现场码失效";
            // 
            // toolStripTextBox_codeExpireLength
            // 
            this.toolStripTextBox_codeExpireLength.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.toolStripTextBox_codeExpireLength.Name = "toolStripTextBox_codeExpireLength";
            this.toolStripTextBox_codeExpireLength.Size = new System.Drawing.Size(100, 44);
            this.toolStripTextBox_codeExpireLength.ToolTipText = "现场码自动失效时间长度";
            this.toolStripTextBox_codeExpireLength.Validating += new System.ComponentModel.CancelEventHandler(this.toolStripTextBox_codeExpireLength_Validating);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 413);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 37);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(27, 28);
            this.toolStripStatusLabel1.Text = "...";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_code
            // 
            this.label_code.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_code.Font = new System.Drawing.Font("微软雅黑", 42F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_code.Location = new System.Drawing.Point(0, 44);
            this.label_code.Name = "label_code";
            this.label_code.Size = new System.Drawing.Size(800, 369);
            this.label_code.TabIndex = 4;
            this.label_code.Text = "现场码";
            this.label_code.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CompactShelfForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label_code);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "CompactShelfForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "密集书架";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CompactShelfForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CompactShelfForm_FormClosed);
            this.Load += new System.EventHandler(this.CompactShelfForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectAccount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_utility;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_clearConnection;
        private System.Windows.Forms.ToolStripButton toolStripButton_setControlParameters;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Label label_code;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_codeExpireLength;
    }
}