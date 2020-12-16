namespace CallRfidCenterSample
{
    partial class TagListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TagListForm));
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_protocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_antenna = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_writeUhf = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_writeUhf_gaoxiao = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_writeUhf_gb_noUserBank = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_writeUhf_gb = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_tags
            // 
            this.listView_tags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_pii,
            this.columnHeader_tu,
            this.columnHeader_oi,
            this.columnHeader_protocol,
            this.columnHeader_antenna,
            this.columnHeader_readerName});
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(16, 15);
            this.listView_tags.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(946, 437);
            this.listView_tags.TabIndex = 0;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            this.listView_tags.SelectedIndexChanged += new System.EventHandler(this.listView_tags_SelectedIndexChanged);
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 212;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 175;
            // 
            // columnHeader_tu
            // 
            this.columnHeader_tu.Text = "标签用途";
            this.columnHeader_tu.Width = 120;
            // 
            // columnHeader_oi
            // 
            this.columnHeader_oi.Text = "所属机构";
            this.columnHeader_oi.Width = 120;
            // 
            // columnHeader_protocol
            // 
            this.columnHeader_protocol.Text = "协议";
            this.columnHeader_protocol.Width = 178;
            // 
            // columnHeader_antenna
            // 
            this.columnHeader_antenna.Text = "天线编号";
            this.columnHeader_antenna.Width = 100;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读卡器名";
            this.columnHeader_readerName.Width = 120;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_writeUhf});
            this.toolStrip1.Location = new System.Drawing.Point(16, 461);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(289, 44);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_writeUhf
            // 
            this.toolStripDropDownButton_writeUhf.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_writeUhf.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank,
            this.ToolStripMenuItem_writeUhf_gaoxiao,
            this.ToolStripMenuItem_writeUhf_gb_noUserBank,
            this.ToolStripMenuItem_writeUhf_gb});
            this.toolStripDropDownButton_writeUhf.Enabled = false;
            this.toolStripDropDownButton_writeUhf.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_writeUhf.Image")));
            this.toolStripDropDownButton_writeUhf.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_writeUhf.Name = "toolStripDropDownButton_writeUhf";
            this.toolStripDropDownButton_writeUhf.Size = new System.Drawing.Size(214, 38);
            this.toolStripDropDownButton_writeUhf.Text = "写入 UHF 图书标签";
            // 
            // ToolStripMenuItem_writeUhf_gaoxiao_noUserBank
            // 
            this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank.Name = "ToolStripMenuItem_writeUhf_gaoxiao_noUserBank";
            this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank.Size = new System.Drawing.Size(404, 40);
            this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank.Text = "高校联盟格式，无 User Bank";
            this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank.Click += new System.EventHandler(this.ToolStripMenuItem_writeUhf_gaoxiao_noUserBank_Click);
            // 
            // ToolStripMenuItem_writeUhf_gaoxiao
            // 
            this.ToolStripMenuItem_writeUhf_gaoxiao.Name = "ToolStripMenuItem_writeUhf_gaoxiao";
            this.ToolStripMenuItem_writeUhf_gaoxiao.Size = new System.Drawing.Size(404, 40);
            this.ToolStripMenuItem_writeUhf_gaoxiao.Text = "高校联盟格式，有 User Bank";
            this.ToolStripMenuItem_writeUhf_gaoxiao.Click += new System.EventHandler(this.ToolStripMenuItem_writeUhf_gaoxiao_Click);
            // 
            // ToolStripMenuItem_writeUhf_gb_noUserBank
            // 
            this.ToolStripMenuItem_writeUhf_gb_noUserBank.Name = "ToolStripMenuItem_writeUhf_gb_noUserBank";
            this.ToolStripMenuItem_writeUhf_gb_noUserBank.Size = new System.Drawing.Size(404, 40);
            this.ToolStripMenuItem_writeUhf_gb_noUserBank.Text = "国标格式，无 User Bank";
            this.ToolStripMenuItem_writeUhf_gb_noUserBank.Click += new System.EventHandler(this.ToolStripMenuItem_writeUhf_gb_noUserBank_Click);
            // 
            // ToolStripMenuItem_writeUhf_gb
            // 
            this.ToolStripMenuItem_writeUhf_gb.Name = "ToolStripMenuItem_writeUhf_gb";
            this.ToolStripMenuItem_writeUhf_gb.Size = new System.Drawing.Size(404, 40);
            this.ToolStripMenuItem_writeUhf_gb.Text = "国标格式，有 UserBank";
            this.ToolStripMenuItem_writeUhf_gb.Click += new System.EventHandler(this.ToolStripMenuItem_writeUhf_gb_Click);
            // 
            // TagListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 525);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView_tags);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "TagListForm";
            this.Text = "TagListForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TagListForm_FormClosed);
            this.Load += new System.EventHandler(this.TagListForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView_tags;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_protocol;
        private System.Windows.Forms.ColumnHeader columnHeader_antenna;
        private System.Windows.Forms.ColumnHeader columnHeader_tu;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_writeUhf;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_writeUhf_gaoxiao_noUserBank;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_writeUhf_gaoxiao;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_writeUhf_gb_noUserBank;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_writeUhf_gb;
    }
}