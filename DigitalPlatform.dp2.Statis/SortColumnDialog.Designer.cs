namespace DigitalPlatform.dp2.Statis
{
    partial class SortColumnDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SortColumnDialog));
            this.listView_columns = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_index = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_name = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_asc = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_dataType = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_ignoreCase = new System.Windows.Forms.ColumnHeader();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_columns
            // 
            this.listView_columns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_columns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_name,
            this.columnHeader_asc,
            this.columnHeader_dataType,
            this.columnHeader_ignoreCase});
            this.listView_columns.FullRowSelect = true;
            this.listView_columns.HideSelection = false;
            this.listView_columns.Location = new System.Drawing.Point(12, 12);
            this.listView_columns.Name = "listView_columns";
            this.listView_columns.Size = new System.Drawing.Size(451, 193);
            this.listView_columns.TabIndex = 0;
            this.listView_columns.UseCompatibleStateImageBehavior = false;
            this.listView_columns.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "序号";
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "列名";
            this.columnHeader_name.Width = 100;
            // 
            // columnHeader_asc
            // 
            this.columnHeader_asc.Text = "升降序";
            this.columnHeader_asc.Width = 70;
            // 
            // columnHeader_dataType
            // 
            this.columnHeader_dataType.Text = "数据类型";
            this.columnHeader_dataType.Width = 100;
            // 
            // columnHeader_ignoreCase
            // 
            this.columnHeader_ignoreCase.Text = "忽略大小写";
            this.columnHeader_ignoreCase.Width = 100;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(307, 257);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(388, 257);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // SortColumnDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(475, 297);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_columns);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SortColumnDialog";
            this.ShowInTaskbar = false;
            this.Text = "配置排序列定义";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SortColumnDialog_FormClosed);
            this.Load += new System.EventHandler(this.SortColumnDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView_columns;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_asc;
        private System.Windows.Forms.ColumnHeader columnHeader_dataType;
        private System.Windows.Forms.ColumnHeader columnHeader_ignoreCase;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}