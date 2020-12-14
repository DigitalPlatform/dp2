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
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_protocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_antenna = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_writeGaoxiao = new System.Windows.Forms.Button();
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
            // button_writeGaoxiao
            // 
            this.button_writeGaoxiao.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_writeGaoxiao.Enabled = false;
            this.button_writeGaoxiao.Location = new System.Drawing.Point(16, 458);
            this.button_writeGaoxiao.Name = "button_writeGaoxiao";
            this.button_writeGaoxiao.Size = new System.Drawing.Size(277, 54);
            this.button_writeGaoxiao.TabIndex = 1;
            this.button_writeGaoxiao.Text = "测试写入 高校联盟 格式";
            this.button_writeGaoxiao.UseVisualStyleBackColor = true;
            this.button_writeGaoxiao.Click += new System.EventHandler(this.button_writeGaoxiao_Click);
            // 
            // TagListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 525);
            this.Controls.Add(this.button_writeGaoxiao);
            this.Controls.Add(this.listView_tags);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "TagListForm";
            this.Text = "TagListForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TagListForm_FormClosed);
            this.Load += new System.EventHandler(this.TagListForm_Load);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Button button_writeGaoxiao;
    }
}