namespace DigitalPlatform.CommonDialog
{
    partial class CategoryPropertyDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CategoryPropertyDlg));
            this.button_noneAll = new System.Windows.Forms.Button();
            this.button_onAll = new System.Windows.Forms.Button();
            this.label_property = new System.Windows.Forms.Label();
            this.textBox_property = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_property = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_category = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_checkState = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_category = new System.Windows.Forms.ComboBox();
            this.button_offAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_noneAll
            // 
            this.button_noneAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_noneAll.Location = new System.Drawing.Point(12, 172);
            this.button_noneAll.Name = "button_noneAll";
            this.button_noneAll.Size = new System.Drawing.Size(75, 23);
            this.button_noneAll.TabIndex = 3;
            this.button_noneAll.Text = "清除(&C)";
            this.button_noneAll.Click += new System.EventHandler(this.button_noneAll_Click);
            // 
            // button_onAll
            // 
            this.button_onAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_onAll.Location = new System.Drawing.Point(93, 172);
            this.button_onAll.Name = "button_onAll";
            this.button_onAll.Size = new System.Drawing.Size(75, 23);
            this.button_onAll.TabIndex = 4;
            this.button_onAll.Text = "全许可(&A)";
            this.button_onAll.Click += new System.EventHandler(this.button_onAll_Click);
            // 
            // label_property
            // 
            this.label_property.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_property.AutoSize = true;
            this.label_property.Location = new System.Drawing.Point(10, 210);
            this.label_property.Name = "label_property";
            this.label_property.Size = new System.Drawing.Size(41, 12);
            this.label_property.TabIndex = 6;
            this.label_property.Text = "值(&V):";
            this.label_property.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_property
            // 
            this.textBox_property.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_property.Location = new System.Drawing.Point(12, 227);
            this.textBox_property.MaxLength = 0;
            this.textBox_property.Multiline = true;
            this.textBox_property.Name = "textBox_property";
            this.textBox_property.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_property.Size = new System.Drawing.Size(376, 63);
            this.textBox_property.TabIndex = 7;
            this.textBox_property.TextChanged += new System.EventHandler(this.textBox_property_TextChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(310, 296);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(78, 23);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(226, 296);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(78, 23);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_property
            // 
            this.listView_property.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_property.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_category,
            this.columnHeader_value,
            this.columnHeader_comment});
            this.listView_property.FullRowSelect = true;
            this.listView_property.LargeImageList = this.imageList_checkState;
            this.listView_property.Location = new System.Drawing.Point(12, 38);
            this.listView_property.Name = "listView_property";
            this.listView_property.Size = new System.Drawing.Size(376, 128);
            this.listView_property.SmallImageList = this.imageList_checkState;
            this.listView_property.TabIndex = 2;
            this.listView_property.UseCompatibleStateImageBehavior = false;
            this.listView_property.View = System.Windows.Forms.View.Details;
            this.listView_property.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView_property_MouseClick);
            // 
            // columnHeader_category
            // 
            this.columnHeader_category.Text = "类目";
            this.columnHeader_category.Width = 120;
            // 
            // columnHeader_value
            // 
            this.columnHeader_value.Text = "值";
            this.columnHeader_value.Width = 120;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 244;
            // 
            // imageList_checkState
            // 
            this.imageList_checkState.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_checkState.ImageStream")));
            this.imageList_checkState.TransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.imageList_checkState.Images.SetKeyName(0, "state_none.bmp");
            this.imageList_checkState.Images.SetKeyName(1, "state_off.bmp");
            this.imageList_checkState.Images.SetKeyName(2, "state_on.bmp");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "类目(&C):";
            // 
            // comboBox_category
            // 
            this.comboBox_category.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_category.FormattingEnabled = true;
            this.comboBox_category.Location = new System.Drawing.Point(82, 12);
            this.comboBox_category.Name = "comboBox_category";
            this.comboBox_category.Size = new System.Drawing.Size(306, 20);
            this.comboBox_category.TabIndex = 1;
            this.comboBox_category.SelectedIndexChanged += new System.EventHandler(this.comboBox_category_SelectedIndexChanged);
            // 
            // button_offAll
            // 
            this.button_offAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_offAll.Location = new System.Drawing.Point(174, 172);
            this.button_offAll.Name = "button_offAll";
            this.button_offAll.Size = new System.Drawing.Size(75, 23);
            this.button_offAll.TabIndex = 5;
            this.button_offAll.Text = "全拒绝(&D)";
            this.button_offAll.Click += new System.EventHandler(this.button_offAll_Click);
            // 
            // CategoryPropertyDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(400, 331);
            this.Controls.Add(this.button_offAll);
            this.Controls.Add(this.comboBox_category);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_noneAll);
            this.Controls.Add(this.button_onAll);
            this.Controls.Add(this.label_property);
            this.Controls.Add(this.textBox_property);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_property);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CategoryPropertyDlg";
            this.ShowInTaskbar = false;
            this.Text = "CategoryPropertyDlg";
            this.Load += new System.EventHandler(this.CategoryPropertyDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_noneAll;
        private System.Windows.Forms.Button button_onAll;
        private System.Windows.Forms.Label label_property;
        private System.Windows.Forms.TextBox textBox_property;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private DigitalPlatform.GUI.ListViewNF listView_property;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_category;
        private System.Windows.Forms.ColumnHeader columnHeader_category;
        private System.Windows.Forms.ColumnHeader columnHeader_value;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ImageList imageList_checkState;
        private System.Windows.Forms.Button button_offAll;
    }
}