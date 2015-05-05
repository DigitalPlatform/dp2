namespace DigitalPlatform.Library
{
    partial class GetLinkDbDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetLinkDbDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_serverUrl = new System.Windows.Forms.TextBox();
            this.button_findServer = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_dbs = new System.Windows.Forms.ListView();
            this.columnHeader_biblioDbName = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_itemDbName = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器(&S):";
            // 
            // textBox_serverUrl
            // 
            this.textBox_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverUrl.Location = new System.Drawing.Point(16, 30);
            this.textBox_serverUrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_serverUrl.Name = "textBox_serverUrl";
            this.textBox_serverUrl.Size = new System.Drawing.Size(372, 25);
            this.textBox_serverUrl.TabIndex = 1;
            // 
            // button_findServer
            // 
            this.button_findServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findServer.Location = new System.Drawing.Point(397, 28);
            this.button_findServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_findServer.Name = "button_findServer";
            this.button_findServer.Size = new System.Drawing.Size(48, 29);
            this.button_findServer.TabIndex = 2;
            this.button_findServer.Text = "...";
            this.button_findServer.UseVisualStyleBackColor = true;
            this.button_findServer.Click += new System.EventHandler(this.button_findServer_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "书目和实体库(&D):";
            // 
            // listView_dbs
            // 
            this.listView_dbs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_dbs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_biblioDbName,
            this.columnHeader_itemDbName,
            this.columnHeader_comment});
            this.listView_dbs.FullRowSelect = true;
            this.listView_dbs.HideSelection = false;
            this.listView_dbs.Location = new System.Drawing.Point(16, 101);
            this.listView_dbs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listView_dbs.MultiSelect = false;
            this.listView_dbs.Name = "listView_dbs";
            this.listView_dbs.Size = new System.Drawing.Size(428, 180);
            this.listView_dbs.TabIndex = 4;
            this.listView_dbs.UseCompatibleStateImageBehavior = false;
            this.listView_dbs.View = System.Windows.Forms.View.Details;
            this.listView_dbs.SelectedIndexChanged += new System.EventHandler(this.listView_dbs_SelectedIndexChanged);
            this.listView_dbs.DoubleClick += new System.EventHandler(this.listView_dbs_DoubleClick);
            // 
            // columnHeader_biblioDbName
            // 
            this.columnHeader_biblioDbName.Text = "书目库";
            this.columnHeader_biblioDbName.Width = 150;
            // 
            // columnHeader_itemDbName
            // 
            this.columnHeader_itemDbName.Text = "实体库";
            this.columnHeader_itemDbName.Width = 150;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 300;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(237, 301);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(100, 29);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(345, 301);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(100, 29);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // GetLinkDbDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 345);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_dbs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_findServer);
            this.Controls.Add(this.textBox_serverUrl);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "GetLinkDbDlg";
            this.Text = "指定书目和实体库";
            this.Load += new System.EventHandler(this.GetLinkDbDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_serverUrl;
        private System.Windows.Forms.Button button_findServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView_dbs;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ColumnHeader columnHeader_biblioDbName;
        private System.Windows.Forms.ColumnHeader columnHeader_itemDbName;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
    }
}