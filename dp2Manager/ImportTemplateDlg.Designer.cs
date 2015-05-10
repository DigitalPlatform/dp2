namespace dp2Manager
{
    partial class ImportTemplateDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportTemplateDlg));
            this.listView_objects = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_create = new System.Windows.Forms.Button();
            this.button_selectAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_objects
            // 
            this.listView_objects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_objects.CheckBoxes = true;
            this.listView_objects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_url});
            this.listView_objects.FullRowSelect = true;
            this.listView_objects.Location = new System.Drawing.Point(12, 12);
            this.listView_objects.Name = "listView_objects";
            this.listView_objects.Size = new System.Drawing.Size(374, 186);
            this.listView_objects.TabIndex = 0;
            this.listView_objects.UseCompatibleStateImageBehavior = false;
            this.listView_objects.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "对象名";
            this.columnHeader_name.Width = 200;
            // 
            // columnHeader_url
            // 
            this.columnHeader_url.Text = "服务器URL";
            this.columnHeader_url.Width = 300;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(311, 236);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "关闭";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_create
            // 
            this.button_create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_create.Location = new System.Drawing.Point(12, 205);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(107, 23);
            this.button_create.TabIndex = 8;
            this.button_create.Text = "创建(&C) ...";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // button_selectAll
            // 
            this.button_selectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_selectAll.Location = new System.Drawing.Point(125, 205);
            this.button_selectAll.Name = "button_selectAll";
            this.button_selectAll.Size = new System.Drawing.Size(75, 23);
            this.button_selectAll.TabIndex = 9;
            this.button_selectAll.Text = "全选";
            this.button_selectAll.UseVisualStyleBackColor = true;
            this.button_selectAll.Click += new System.EventHandler(this.button_selectAll_Click);
            // 
            // ImportTemplateDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 271);
            this.Controls.Add(this.button_selectAll);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.listView_objects);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImportTemplateDlg";
            this.ShowInTaskbar = false;
            this.Text = "导入模板文件";
            this.Load += new System.EventHandler(this.ImportTemplateDlg_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_objects;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_url;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.Button button_selectAll;
    }
}