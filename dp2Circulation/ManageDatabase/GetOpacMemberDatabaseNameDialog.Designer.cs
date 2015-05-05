namespace dp2Circulation
{
    partial class GetOpacMemberDatabaseNameDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetOpacMemberDatabaseNameDialog));
            this.listView_databases = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_databaseName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_databaseName = new System.Windows.Forms.TextBox();
            this.imageList_databaseType = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // listView_databases
            // 
            this.listView_databases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_databases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_databaseName,
            this.columnHeader_type});
            this.listView_databases.FullRowSelect = true;
            this.listView_databases.HideSelection = false;
            this.listView_databases.Location = new System.Drawing.Point(10, 10);
            this.listView_databases.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView_databases.Name = "listView_databases";
            this.listView_databases.Size = new System.Drawing.Size(287, 140);
            this.listView_databases.TabIndex = 0;
            this.listView_databases.UseCompatibleStateImageBehavior = false;
            this.listView_databases.View = System.Windows.Forms.View.Details;
            this.listView_databases.SelectedIndexChanged += new System.EventHandler(this.listView_databases_SelectedIndexChanged);
            this.listView_databases.DoubleClick += new System.EventHandler(this.listView_databases_DoubleClick);
            // 
            // columnHeader_databaseName
            // 
            this.columnHeader_databaseName.Text = "数据库名";
            this.columnHeader_databaseName.Width = 120;
            // 
            // columnHeader_type
            // 
            this.columnHeader_type.Text = "类型";
            this.columnHeader_type.Width = 120;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(239, 182);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(239, 154);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 160);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "数据库名(&D):";
            // 
            // textBox_databaseName
            // 
            this.textBox_databaseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_databaseName.Location = new System.Drawing.Point(86, 154);
            this.textBox_databaseName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_databaseName.Name = "textBox_databaseName";
            this.textBox_databaseName.Size = new System.Drawing.Size(150, 21);
            this.textBox_databaseName.TabIndex = 10;
            // 
            // imageList_databaseType
            // 
            this.imageList_databaseType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_databaseType.ImageStream")));
            this.imageList_databaseType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_databaseType.Images.SetKeyName(0, "database.bmp");
            this.imageList_databaseType.Images.SetKeyName(1, "grayed_database.bmp");
            // 
            // GetOpacMemberDatabaseNameDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(304, 214);
            this.Controls.Add(this.textBox_databaseName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_databases);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "GetOpacMemberDatabaseNameDialog";
            this.ShowInTaskbar = false;
            this.Text = "请指定数据库名";
            this.Load += new System.EventHandler(this.GetOpacMemberDatabaseNameDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView_databases;
        private System.Windows.Forms.ColumnHeader columnHeader_databaseName;
        private System.Windows.Forms.ColumnHeader columnHeader_type;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_databaseName;
        private System.Windows.Forms.ImageList imageList_databaseType;
    }
}