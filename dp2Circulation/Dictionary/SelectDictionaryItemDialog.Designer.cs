namespace dp2Circulation
{
    partial class SelectDictionaryItemDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDictionaryItemDialog));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.listView_list = new System.Windows.Forms.ListView();
            this.columnHeader_key = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_keyCaption = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_rel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_weight = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_relCaption = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_upLevel = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_downLevel = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_currentKey = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_wild = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listView_levels = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label_message = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(312, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(393, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // listView_list
            // 
            this.listView_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_key,
            this.columnHeader_keyCaption,
            this.columnHeader_rel,
            this.columnHeader_weight,
            this.columnHeader_relCaption});
            this.listView_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_list.FullRowSelect = true;
            this.listView_list.HideSelection = false;
            this.listView_list.Location = new System.Drawing.Point(0, 0);
            this.listView_list.Name = "listView_list";
            this.listView_list.Size = new System.Drawing.Size(316, 195);
            this.listView_list.TabIndex = 2;
            this.listView_list.UseCompatibleStateImageBehavior = false;
            this.listView_list.View = System.Windows.Forms.View.Details;
            this.listView_list.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader_key
            // 
            this.columnHeader_key.Text = "键";
            this.columnHeader_key.Width = 79;
            // 
            // columnHeader_keyCaption
            // 
            this.columnHeader_keyCaption.Text = "键标签";
            this.columnHeader_keyCaption.Width = 83;
            // 
            // columnHeader_rel
            // 
            this.columnHeader_rel.Text = "关联项";
            this.columnHeader_rel.Width = 103;
            // 
            // columnHeader_weight
            // 
            this.columnHeader_weight.Text = "权值";
            this.columnHeader_weight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader_relCaption
            // 
            this.columnHeader_relCaption.Text = "关联项标签";
            this.columnHeader_relCaption.Width = 194;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_stop,
            this.toolStripButton_upLevel,
            this.toolStripButton_downLevel,
            this.toolStripLabel_currentKey,
            this.toolStripButton_wild});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(480, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_stop
            // 
            this.toolStripButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stop.Image")));
            this.toolStripButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_stop.Name = "toolStripButton_stop";
            this.toolStripButton_stop.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_stop.Text = "停止";
            // 
            // toolStripButton_upLevel
            // 
            this.toolStripButton_upLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_upLevel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_upLevel.Image")));
            this.toolStripButton_upLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_upLevel.Name = "toolStripButton_upLevel";
            this.toolStripButton_upLevel.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_upLevel.Text = "上级";
            this.toolStripButton_upLevel.Click += new System.EventHandler(this.toolStripButton_upLevel_Click);
            // 
            // toolStripButton_downLevel
            // 
            this.toolStripButton_downLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_downLevel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_downLevel.Image")));
            this.toolStripButton_downLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_downLevel.Name = "toolStripButton_downLevel";
            this.toolStripButton_downLevel.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_downLevel.Text = "下级";
            this.toolStripButton_downLevel.Click += new System.EventHandler(this.toolStripButton_downLevel_Click);
            // 
            // toolStripLabel_currentKey
            // 
            this.toolStripLabel_currentKey.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_currentKey.Name = "toolStripLabel_currentKey";
            this.toolStripLabel_currentKey.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripButton_wild
            // 
            this.toolStripButton_wild.CheckOnClick = true;
            this.toolStripButton_wild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_wild.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_wild.Image")));
            this.toolStripButton_wild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_wild.Name = "toolStripButton_wild";
            this.toolStripButton_wild.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_wild.Text = "前方一致";
            this.toolStripButton_wild.CheckedChanged += new System.EventHandler(this.toolStripButton_wild_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView_levels);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView_list);
            this.splitContainer1.Size = new System.Drawing.Size(480, 195);
            this.splitContainer1.SplitterDistance = 160;
            this.splitContainer1.TabIndex = 4;
            // 
            // listView_levels
            // 
            this.listView_levels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView_levels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_levels.FullRowSelect = true;
            this.listView_levels.HideSelection = false;
            this.listView_levels.Location = new System.Drawing.Point(0, 0);
            this.listView_levels.MultiSelect = false;
            this.listView_levels.Name = "listView_levels";
            this.listView_levels.Size = new System.Drawing.Size(160, 195);
            this.listView_levels.TabIndex = 0;
            this.listView_levels.UseCompatibleStateImageBehavior = false;
            this.listView_levels.View = System.Windows.Forms.View.Details;
            this.listView_levels.SelectedIndexChanged += new System.EventHandler(this.listView_levels_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "键";
            this.columnHeader1.Width = 92;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "命中数";
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_message.AutoSize = true;
            this.label_message.Location = new System.Drawing.Point(-2, 234);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(0, 12);
            this.label_message.TabIndex = 5;
            // 
            // SelectDictionaryItemDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(480, 264);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SelectDictionaryItemDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请选择事项";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectDictionaryItemDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SelectDictionaryItemDialog_FormClosed);
            this.Load += new System.EventHandler(this.SelectDictionaryItemDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ListView listView_list;
        private System.Windows.Forms.ColumnHeader columnHeader_key;
        private System.Windows.Forms.ColumnHeader columnHeader_keyCaption;
        private System.Windows.Forms.ColumnHeader columnHeader_rel;
        private System.Windows.Forms.ColumnHeader columnHeader_relCaption;
        private System.Windows.Forms.ColumnHeader columnHeader_weight;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_upLevel;
        private System.Windows.Forms.ToolStripButton toolStripButton_downLevel;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_currentKey;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listView_levels;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripButton toolStripButton_wild;
        private System.Windows.Forms.ToolStripButton toolStripButton_stop;
        private System.Windows.Forms.Label label_message;
    }
}