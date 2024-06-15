namespace dp2Circulation
{
    partial class GetConvertTargetDbNameDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetConvertTargetDbNameDlg));
            this.listView_dbnames = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_dbname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.columnHeader_rule = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView_dbnames
            // 
            this.listView_dbnames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_dbnames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_dbname,
            this.columnHeader_rule});
            this.listView_dbnames.FullRowSelect = true;
            this.listView_dbnames.HideSelection = false;
            this.listView_dbnames.Location = new System.Drawing.Point(18, 18);
            this.listView_dbnames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listView_dbnames.MultiSelect = false;
            this.listView_dbnames.Name = "listView_dbnames";
            this.listView_dbnames.Size = new System.Drawing.Size(474, 290);
            this.listView_dbnames.TabIndex = 0;
            this.listView_dbnames.UseCompatibleStateImageBehavior = false;
            this.listView_dbnames.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_dbname
            // 
            this.columnHeader_dbname.Text = "目标库名";
            this.columnHeader_dbname.Width = 257;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(279, 318);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 39);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(390, 318);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 39);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // columnHeader_rule
            // 
            this.columnHeader_rule.Text = "默认编目规则";
            this.columnHeader_rule.Width = 200;
            // 
            // GetConvertTargetDbNameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(510, 374);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_dbnames);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "GetConvertTargetDbNameDlg";
            this.ShowInTaskbar = false;
            this.Text = "请指定格式转换的目标库名";
            this.Load += new System.EventHandler(this.GetAcceptTargetDbNameDlg_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView_dbnames;
        private System.Windows.Forms.ColumnHeader columnHeader_dbname;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ColumnHeader columnHeader_rule;
    }
}