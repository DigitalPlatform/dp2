namespace dp2Manager
{
    partial class ExportTemplateDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportTemplateDlg));
            this.listView_objects = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_exportFileName = new System.Windows.Forms.TextBox();
            this.button_findExportFileName = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
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
            this.listView_objects.Location = new System.Drawing.Point(13, 13);
            this.listView_objects.Name = "listView_objects";
            this.listView_objects.Size = new System.Drawing.Size(361, 177);
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
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 205);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "输出文件名(&F):";
            // 
            // textBox_exportFileName
            // 
            this.textBox_exportFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_exportFileName.Location = new System.Drawing.Point(14, 221);
            this.textBox_exportFileName.Name = "textBox_exportFileName";
            this.textBox_exportFileName.Size = new System.Drawing.Size(321, 21);
            this.textBox_exportFileName.TabIndex = 2;
            // 
            // button_findExportFileName
            // 
            this.button_findExportFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findExportFileName.Location = new System.Drawing.Point(341, 221);
            this.button_findExportFileName.Name = "button_findExportFileName";
            this.button_findExportFileName.Size = new System.Drawing.Size(33, 23);
            this.button_findExportFileName.TabIndex = 3;
            this.button_findExportFileName.Text = "...";
            this.button_findExportFileName.UseVisualStyleBackColor = true;
            this.button_findExportFileName.Click += new System.EventHandler(this.button_findExportFileName_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(218, 254);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(299, 254);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "放弃";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // ExportTemplateDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(386, 289);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_findExportFileName);
            this.Controls.Add(this.textBox_exportFileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_objects);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportTemplateDlg";
            this.ShowInTaskbar = false;
            this.Text = "导出模板文件";
            this.Load += new System.EventHandler(this.ExportTemplateDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView_objects;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_exportFileName;
        private System.Windows.Forms.Button button_findExportFileName;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ColumnHeader columnHeader_url;
    }
}