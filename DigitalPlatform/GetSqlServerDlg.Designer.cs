namespace DigitalPlatform
{
    partial class GetSqlServerDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetSqlServerDlg));
            this.listView_sqlServers = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_instanceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_isClusters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_version = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_sqlServers
            // 
            this.listView_sqlServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_instanceName,
            this.columnHeader_isClusters,
            this.columnHeader_version});
            this.listView_sqlServers.FullRowSelect = true;
            this.listView_sqlServers.HideSelection = false;
            this.listView_sqlServers.Location = new System.Drawing.Point(12, 12);
            this.listView_sqlServers.Name = "listView_sqlServers";
            this.listView_sqlServers.Size = new System.Drawing.Size(327, 182);
            this.listView_sqlServers.TabIndex = 0;
            this.listView_sqlServers.UseCompatibleStateImageBehavior = false;
            this.listView_sqlServers.View = System.Windows.Forms.View.Details;
            this.listView_sqlServers.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView_sqlServers_ItemSelectionChanged);
            this.listView_sqlServers.DoubleClick += new System.EventHandler(this.listView_sqlServers_DoubleClick);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "SQL服务器名";
            this.columnHeader_name.Width = 135;
            // 
            // columnHeader_instanceName
            // 
            this.columnHeader_instanceName.Text = "实例名";
            this.columnHeader_instanceName.Width = 117;
            // 
            // columnHeader_isClusters
            // 
            this.columnHeader_isClusters.Text = "是否集群";
            this.columnHeader_isClusters.Width = 96;
            // 
            // columnHeader_version
            // 
            this.columnHeader_version.Text = "服务器版本";
            this.columnHeader_version.Width = 135;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 210);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "SQL服务器(&S):";
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Location = new System.Drawing.Point(12, 225);
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.Size = new System.Drawing.Size(327, 21);
            this.textBox_sqlServerName.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(183, 252);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(264, 252);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // GetSqlServerDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(351, 287);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_sqlServerName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_sqlServers);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GetSqlServerDlg";
            this.ShowInTaskbar = false;
            this.Text = "获得 Microsoft SQL 服务器名";
            this.Load += new System.EventHandler(this.SqlServerDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView_sqlServers;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_instanceName;
        private System.Windows.Forms.ColumnHeader columnHeader_isClusters;
        private System.Windows.Forms.ColumnHeader columnHeader_version;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;

    }
}