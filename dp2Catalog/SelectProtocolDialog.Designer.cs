namespace dp2Catalog
{
    partial class SelectProtocolDialog
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "dp2library",
            "dp2图书馆集成系统"}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "DTLP",
            "dt1000/dt1500图书馆集成系统"}, -1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "Z39.50",
            "通用信息检索协议"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectProtocolDialog));
            this.listView_protocol = new System.Windows.Forms.ListView();
            this.columnHeader_name = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_protocol
            // 
            this.listView_protocol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_protocol.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_comment});
            this.listView_protocol.FullRowSelect = true;
            this.listView_protocol.HideSelection = false;
            this.listView_protocol.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.listView_protocol.Location = new System.Drawing.Point(13, 13);
            this.listView_protocol.MultiSelect = false;
            this.listView_protocol.Name = "listView_protocol";
            this.listView_protocol.Size = new System.Drawing.Size(477, 117);
            this.listView_protocol.TabIndex = 0;
            this.listView_protocol.UseCompatibleStateImageBehavior = false;
            this.listView_protocol.View = System.Windows.Forms.View.Details;
            this.listView_protocol.DoubleClick += new System.EventHandler(this.listView_protocol_DoubleClick);
            this.listView_protocol.SelectedIndexChanged += new System.EventHandler(this.listView_protocol_SelectedIndexChanged);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "协议名";
            this.columnHeader_name.Width = 161;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 300;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(390, 136);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(100, 28);
            this.button_Cancel.TabIndex = 13;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(284, 136);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(100, 28);
            this.button_OK.TabIndex = 12;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // SelectProtocolDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(502, 176);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_protocol);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelectProtocolDialog";
            this.ShowInTaskbar = false;
            this.Text = "请选择一个图书馆应用协议";
            this.Load += new System.EventHandler(this.SelectProtocolDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_protocol;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}