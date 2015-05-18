namespace dp2LibraryXE
{
    partial class SelectSqlServerDialog
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
            this.radioButton_sqlite = new System.Windows.Forms.RadioButton();
            this.radioButton_localdb = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_installLocalDB = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_refresh = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton_sqlite
            // 
            this.radioButton_sqlite.AutoSize = true;
            this.radioButton_sqlite.Checked = true;
            this.radioButton_sqlite.Location = new System.Drawing.Point(24, 34);
            this.radioButton_sqlite.Name = "radioButton_sqlite";
            this.radioButton_sqlite.Size = new System.Drawing.Size(59, 16);
            this.radioButton_sqlite.TabIndex = 2;
            this.radioButton_sqlite.TabStop = true;
            this.radioButton_sqlite.Text = "SQLite";
            this.radioButton_sqlite.UseVisualStyleBackColor = true;
            // 
            // radioButton_localdb
            // 
            this.radioButton_localdb.AutoSize = true;
            this.radioButton_localdb.Location = new System.Drawing.Point(24, 66);
            this.radioButton_localdb.Name = "radioButton_localdb";
            this.radioButton_localdb.Size = new System.Drawing.Size(149, 16);
            this.radioButton_localdb.TabIndex = 3;
            this.radioButton_localdb.Text = "MS SQL Server LocalDB";
            this.radioButton_localdb.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_installLocalDB);
            this.groupBox1.Controls.Add(this.radioButton_sqlite);
            this.groupBox1.Controls.Add(this.radioButton_localdb);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(436, 114);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " SQL 服务器类型 ";
            // 
            // button_installLocalDB
            // 
            this.button_installLocalDB.Location = new System.Drawing.Point(207, 63);
            this.button_installLocalDB.Name = "button_installLocalDB";
            this.button_installLocalDB.Size = new System.Drawing.Size(223, 23);
            this.button_installLocalDB.TabIndex = 4;
            this.button_installLocalDB.Text = "安装 MS SQL Server LocalDB ...";
            this.button_installLocalDB.UseVisualStyleBackColor = true;
            this.button_installLocalDB.Click += new System.EventHandler(this.button_installLocalDB_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(292, 146);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(373, 146);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_refresh
            // 
            this.button_refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_refresh.Location = new System.Drawing.Point(12, 146);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(75, 23);
            this.button_refresh.TabIndex = 7;
            this.button_refresh.Text = "刷新";
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // SelectSqlServerDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(460, 181);
            this.Controls.Add(this.button_refresh);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Name = "SelectSqlServerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请选择 SQL 服务器类型";
            this.Load += new System.EventHandler(this.SelectSqlServerDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton_sqlite;
        private System.Windows.Forms.RadioButton radioButton_localdb;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_installLocalDB;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_refresh;
    }
}