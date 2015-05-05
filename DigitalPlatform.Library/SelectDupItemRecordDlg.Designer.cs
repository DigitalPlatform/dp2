namespace DigitalPlatform.Library
{
    partial class SelectDupItemRecordDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDupItemRecordDlg));
            this.label_message = new System.Windows.Forms.Label();
            this.listView_paths = new System.Windows.Forms.ListView();
            this.columnHeader_itemRecPath = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_biblioRecPath = new System.Windows.Forms.ColumnHeader();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(16, 11);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(353, 88);
            this.label_message.TabIndex = 0;
            this.label_message.Text = "text";
            // 
            // listView_paths
            // 
            this.listView_paths.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_paths.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_itemRecPath,
            this.columnHeader_biblioRecPath});
            this.listView_paths.FullRowSelect = true;
            this.listView_paths.HideSelection = false;
            this.listView_paths.Location = new System.Drawing.Point(16, 102);
            this.listView_paths.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listView_paths.Name = "listView_paths";
            this.listView_paths.Size = new System.Drawing.Size(356, 168);
            this.listView_paths.TabIndex = 1;
            this.listView_paths.UseCompatibleStateImageBehavior = false;
            this.listView_paths.View = System.Windows.Forms.View.Details;
            this.listView_paths.SelectedIndexChanged += new System.EventHandler(this.listView_paths_SelectedIndexChanged);
            this.listView_paths.DoubleClick += new System.EventHandler(this.listView_paths_DoubleClick);
            // 
            // columnHeader_itemRecPath
            // 
            this.columnHeader_itemRecPath.Text = "册记录路径";
            this.columnHeader_itemRecPath.Width = 150;
            // 
            // columnHeader_biblioRecPath
            // 
            this.columnHeader_biblioRecPath.Text = "所属种记录路径";
            this.columnHeader_biblioRecPath.Width = 150;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(165, 301);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(100, 29);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(273, 301);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(100, 29);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // SelectDupItemRecordDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 345);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_paths);
            this.Controls.Add(this.label_message);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SelectDupItemRecordDlg";
            this.Text = "请选择册记录路径";
            this.Load += new System.EventHandler(this.SelectDupItemRecord_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.ListView listView_paths;
        private System.Windows.Forms.ColumnHeader columnHeader_itemRecPath;
        private System.Windows.Forms.ColumnHeader columnHeader_biblioRecPath;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}