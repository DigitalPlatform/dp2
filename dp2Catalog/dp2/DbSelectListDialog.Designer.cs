namespace dp2Catalog
{
    partial class DbSelectListDialog
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_down = new System.Windows.Forms.Button();
            this.button_up = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_selectAll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(325, 211);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "数据库名";
            this.columnHeader_name.Width = 216;
            // 
            // button_down
            // 
            this.button_down.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_down.Location = new System.Drawing.Point(343, 41);
            this.button_down.Name = "button_down";
            this.button_down.Size = new System.Drawing.Size(75, 23);
            this.button_down.TabIndex = 6;
            this.button_down.Text = "下移(&D)";
            this.button_down.Click += new System.EventHandler(this.button_down_Click);
            // 
            // button_up
            // 
            this.button_up.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_up.Location = new System.Drawing.Point(343, 12);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(75, 23);
            this.button_up.TabIndex = 5;
            this.button_up.Text = "上移(&U)";
            this.button_up.Click += new System.EventHandler(this.button_up_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(343, 200);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(343, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_selectAll
            // 
            this.checkBox_selectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_selectAll.AutoSize = true;
            this.checkBox_selectAll.Location = new System.Drawing.Point(12, 236);
            this.checkBox_selectAll.Name = "checkBox_selectAll";
            this.checkBox_selectAll.Size = new System.Drawing.Size(90, 16);
            this.checkBox_selectAll.TabIndex = 9;
            this.checkBox_selectAll.Text = "全部都要(&A)";
            this.checkBox_selectAll.UseVisualStyleBackColor = true;
            this.checkBox_selectAll.CheckedChanged += new System.EventHandler(this.checkBox_selectAll_CheckedChanged);
            // 
            // DbSelectListDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(430, 264);
            this.Controls.Add(this.checkBox_selectAll);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_down);
            this.Controls.Add(this.button_up);
            this.Controls.Add(this.listView1);
            this.Name = "DbSelectListDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请指定多库命中时的优先选择顺序";
            this.Load += new System.EventHandler(this.DbSelectListDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.Button button_down;
        private System.Windows.Forms.Button button_up;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_selectAll;
    }
}