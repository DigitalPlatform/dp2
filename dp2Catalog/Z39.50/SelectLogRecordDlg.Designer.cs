namespace dp2Catalog
{
	partial class SelectLogRecordDlg
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
            this.listView_index = new System.Windows.Forms.ListView();
            this.columnHeader_no = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_offset = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_logFilename = new System.Windows.Forms.TextBox();
            this.button_findLogFilename = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_loadFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_index
            // 
            this.listView_index.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_index.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_no,
            this.columnHeader_offset});
            this.listView_index.Location = new System.Drawing.Point(12, 134);
            this.listView_index.Name = "listView_index";
            this.listView_index.Size = new System.Drawing.Size(448, 112);
            this.listView_index.TabIndex = 0;
            this.listView_index.UseCompatibleStateImageBehavior = false;
            this.listView_index.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_no
            // 
            this.columnHeader_no.Text = "序号";
            this.columnHeader_no.Width = 100;
            // 
            // columnHeader_offset
            // 
            this.columnHeader_offset.Text = "偏移量";
            this.columnHeader_offset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_offset.Width = 120;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "日志文件名(L):";
            // 
            // textBox_logFilename
            // 
            this.textBox_logFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_logFilename.Location = new System.Drawing.Point(13, 28);
            this.textBox_logFilename.Name = "textBox_logFilename";
            this.textBox_logFilename.Size = new System.Drawing.Size(398, 25);
            this.textBox_logFilename.TabIndex = 2;
            this.textBox_logFilename.TextChanged += new System.EventHandler(this.textBox_logFilename_TextChanged);
            // 
            // button_findLogFilename
            // 
            this.button_findLogFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findLogFilename.Location = new System.Drawing.Point(417, 28);
            this.button_findLogFilename.Name = "button_findLogFilename";
            this.button_findLogFilename.Size = new System.Drawing.Size(43, 28);
            this.button_findLogFilename.TabIndex = 3;
            this.button_findLogFilename.Text = "...";
            this.button_findLogFilename.UseVisualStyleBackColor = true;
            this.button_findLogFilename.Click += new System.EventHandler(this.button_findLogFilename_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "日志记录(R):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(304, 263);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(385, 263);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_loadFile
            // 
            this.button_loadFile.Enabled = false;
            this.button_loadFile.Location = new System.Drawing.Point(13, 60);
            this.button_loadFile.Name = "button_loadFile";
            this.button_loadFile.Size = new System.Drawing.Size(95, 28);
            this.button_loadFile.TabIndex = 7;
            this.button_loadFile.Text = "装载(&L)";
            this.button_loadFile.UseVisualStyleBackColor = true;
            this.button_loadFile.Click += new System.EventHandler(this.button_loadFile_Click);
            // 
            // SelectLogRecordDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 303);
            this.Controls.Add(this.button_loadFile);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_findLogFilename);
            this.Controls.Add(this.textBox_logFilename);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_index);
            this.Name = "SelectLogRecordDlg";
            this.Text = "选择日志记录(包)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SelectLogRecordDlg_FormClosed);
            this.Load += new System.EventHandler(this.SelectLogRecordDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.ListView listView_index;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_logFilename;
        private System.Windows.Forms.Button button_findLogFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader_no;
        private System.Windows.Forms.ColumnHeader columnHeader_offset;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_loadFile;
	}
}