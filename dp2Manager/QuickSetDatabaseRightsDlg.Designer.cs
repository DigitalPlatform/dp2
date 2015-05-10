namespace dp2Manager
{
    partial class QuickSetDatabaseRightsDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickSetDatabaseRightsDlg));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.radioButton_allObjects = new System.Windows.Forms.RadioButton();
            this.radioButton_selectedObjects = new System.Windows.Forms.RadioButton();
            this.listView_style = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listView_objectNames = new System.Windows.Forms.ListView();
            this.columnHeader_objectName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(351, 321);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "放弃";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(271, 321);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // radioButton_allObjects
            // 
            this.radioButton_allObjects.Location = new System.Drawing.Point(31, 161);
            this.radioButton_allObjects.Name = "radioButton_allObjects";
            this.radioButton_allObjects.Size = new System.Drawing.Size(122, 24);
            this.radioButton_allObjects.TabIndex = 8;
            this.radioButton_allObjects.Text = "全部对象(&A)";
            this.radioButton_allObjects.CheckedChanged += new System.EventHandler(this.radioButton_allObjects_CheckedChanged);
            // 
            // radioButton_selectedObjects
            // 
            this.radioButton_selectedObjects.Checked = true;
            this.radioButton_selectedObjects.Location = new System.Drawing.Point(175, 161);
            this.radioButton_selectedObjects.Name = "radioButton_selectedObjects";
            this.radioButton_selectedObjects.Size = new System.Drawing.Size(123, 24);
            this.radioButton_selectedObjects.TabIndex = 9;
            this.radioButton_selectedObjects.TabStop = true;
            this.radioButton_selectedObjects.Text = "所选对象(&S)";
            this.radioButton_selectedObjects.CheckedChanged += new System.EventHandler(this.radioButton_selectedObjects_CheckedChanged);
            // 
            // listView_style
            // 
            this.listView_style.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_style.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_comment});
            this.listView_style.FullRowSelect = true;
            this.listView_style.HideSelection = false;
            this.listView_style.Location = new System.Drawing.Point(7, 9);
            this.listView_style.MultiSelect = false;
            this.listView_style.Name = "listView_style";
            this.listView_style.Size = new System.Drawing.Size(416, 128);
            this.listView_style.TabIndex = 6;
            this.listView_style.UseCompatibleStateImageBehavior = false;
            this.listView_style.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "风格名";
            this.columnHeader_name.Width = 146;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 279;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listView_objectNames);
            this.groupBox1.Location = new System.Drawing.Point(7, 145);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(416, 160);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 针对: ";
            // 
            // listView_objectNames
            // 
            this.listView_objectNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_objectNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_objectName});
            this.listView_objectNames.FullRowSelect = true;
            this.listView_objectNames.HideSelection = false;
            this.listView_objectNames.LargeImageList = this.imageList_resIcon;
            this.listView_objectNames.Location = new System.Drawing.Point(16, 40);
            this.listView_objectNames.Name = "listView_objectNames";
            this.listView_objectNames.Size = new System.Drawing.Size(384, 112);
            this.listView_objectNames.SmallImageList = this.imageList_resIcon;
            this.listView_objectNames.TabIndex = 0;
            this.listView_objectNames.UseCompatibleStateImageBehavior = false;
            this.listView_objectNames.View = System.Windows.Forms.View.Details;
            this.listView_objectNames.SelectedIndexChanged += new System.EventHandler(this.listView_objectNames_SelectedIndexChanged);
            // 
            // columnHeader_objectName
            // 
            this.columnHeader_objectName.Text = "对象名";
            this.columnHeader_objectName.Width = 288;
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon.Images.SetKeyName(0, "");
            this.imageList_resIcon.Images.SetKeyName(1, "");
            this.imageList_resIcon.Images.SetKeyName(2, "");
            this.imageList_resIcon.Images.SetKeyName(3, "");
            this.imageList_resIcon.Images.SetKeyName(4, "");
            this.imageList_resIcon.Images.SetKeyName(5, "");
            // 
            // QuickSetDatabaseRightsDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(432, 352);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.radioButton_allObjects);
            this.Controls.Add(this.radioButton_selectedObjects);
            this.Controls.Add(this.listView_style);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QuickSetDatabaseRightsDlg";
            this.ShowInTaskbar = false;
            this.Text = "快速设置本用户对多个对象的权限";
            this.Load += new System.EventHandler(this.QuikSetDatabaseRightsDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.RadioButton radioButton_allObjects;
        private System.Windows.Forms.RadioButton radioButton_selectedObjects;
        private System.Windows.Forms.ListView listView_style;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listView_objectNames;
        private System.Windows.Forms.ColumnHeader columnHeader_objectName;
        private System.Windows.Forms.ImageList imageList_resIcon;
    }
}