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
            this.button_uncheckAll = new System.Windows.Forms.Button();
            this.button_checkAll = new System.Windows.Forms.Button();
            this.label_property = new System.Windows.Forms.Label();
            this.textBox_property = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_property = new System.Windows.Forms.ListView();
            this.columnHeader_category = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_value = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_category = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_uncheckAll
            // 
            this.button_uncheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_uncheckAll.Location = new System.Drawing.Point(88, 207);
            this.button_uncheckAll.Name = "button_uncheckAll";
            this.button_uncheckAll.Size = new System.Drawing.Size(75, 23);
            this.button_uncheckAll.TabIndex = 13;
            this.button_uncheckAll.Text = "清除(&C)";
            this.button_uncheckAll.Click += new System.EventHandler(this.button_uncheckAll_Click);
            // 
            // button_checkAll
            // 
            this.button_checkAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_checkAll.Location = new System.Drawing.Point(8, 207);
            this.button_checkAll.Name = "button_checkAll";
            this.button_checkAll.Size = new System.Drawing.Size(75, 23);
            this.button_checkAll.TabIndex = 12;
            this.button_checkAll.Text = "全选(&A)";
            this.button_checkAll.Click += new System.EventHandler(this.button_checkAll_Click);
            // 
            // label_property
            // 
            this.label_property.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_property.Location = new System.Drawing.Point(6, 239);
            this.label_property.Name = "label_property";
            this.label_property.Size = new System.Drawing.Size(382, 18);
            this.label_property.TabIndex = 8;
            this.label_property.Text = "值(&V):";
            this.label_property.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_property
            // 
            this.textBox_property.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_property.Location = new System.Drawing.Point(6, 263);
            this.textBox_property.Multiline = true;
            this.textBox_property.Name = "textBox_property";
            this.textBox_property.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_property.Size = new System.Drawing.Size(386, 62);
            this.textBox_property.TabIndex = 9;
            this.textBox_property.TextChanged += new System.EventHandler(this.textBox_property_TextChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(316, 335);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(78, 23);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(230, 335);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(78, 23);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_property
            // 
            this.listView_property.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_property.CheckBoxes = true;
            this.listView_property.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_category,
            this.columnHeader_value,
            this.columnHeader_comment});
            this.listView_property.FullRowSelect = true;
            this.listView_property.Location = new System.Drawing.Point(6, 48);
            this.listView_property.Name = "listView_property";
            this.listView_property.Size = new System.Drawing.Size(388, 151);
            this.listView_property.TabIndex = 7;
            this.listView_property.UseCompatibleStateImageBehavior = false;
            this.listView_property.View = System.Windows.Forms.View.Details;
            this.listView_property.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_property_ItemChecked);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 14;
            this.label1.Text = "类目(&C):";
            // 
            // comboBox_category
            // 
            this.comboBox_category.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_category.FormattingEnabled = true;
            this.comboBox_category.Location = new System.Drawing.Point(76, 13);
            this.comboBox_category.Name = "comboBox_category";
            this.comboBox_category.Size = new System.Drawing.Size(318, 20);
            this.comboBox_category.TabIndex = 15;
            this.comboBox_category.SelectedIndexChanged += new System.EventHandler(this.comboBox_category_SelectedIndexChanged);
            // 
            // CategoryPropertyDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 367);
            this.Controls.Add(this.comboBox_category);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_uncheckAll);
            this.Controls.Add(this.button_checkAll);
            this.Controls.Add(this.label_property);
            this.Controls.Add(this.textBox_property);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_property);
            this.Name = "CategoryPropertyDlg";
            this.Text = "CategoryPropertyDlg";
            this.Load += new System.EventHandler(this.CategoryPropertyDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_uncheckAll;
        private System.Windows.Forms.Button button_checkAll;
        private System.Windows.Forms.Label label_property;
        private System.Windows.Forms.TextBox textBox_property;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ListView listView_property;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_category;
        private System.Windows.Forms.ColumnHeader columnHeader_category;
        private System.Windows.Forms.ColumnHeader columnHeader_value;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
    }
}