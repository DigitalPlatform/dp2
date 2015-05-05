namespace dp2Circulation
{
    partial class SelectPaperDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_printerName = new System.Windows.Forms.TextBox();
            this.textBox_paperName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listView_papers = new System.Windows.Forms.ListView();
            this.columnHeader_paperName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_kind = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_width = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_height = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_comment = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(255, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(174, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "打印机(&P):";
            // 
            // textBox_printerName
            // 
            this.textBox_printerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_printerName.Location = new System.Drawing.Point(69, 0);
            this.textBox_printerName.Name = "textBox_printerName";
            this.textBox_printerName.ReadOnly = true;
            this.textBox_printerName.Size = new System.Drawing.Size(243, 21);
            this.textBox_printerName.TabIndex = 10;
            // 
            // textBox_paperName
            // 
            this.textBox_paperName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_paperName.Location = new System.Drawing.Point(69, 146);
            this.textBox_paperName.Name = "textBox_paperName";
            this.textBox_paperName.ReadOnly = true;
            this.textBox_paperName.Size = new System.Drawing.Size(243, 21);
            this.textBox_paperName.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "纸张(&A):";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listView_papers);
            this.panel1.Controls.Add(this.textBox_printerName);
            this.panel1.Controls.Add(this.textBox_paperName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 35);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(312, 167);
            this.panel1.TabIndex = 13;
            // 
            // listView_papers
            // 
            this.listView_papers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_papers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_paperName,
            this.columnHeader_kind,
            this.columnHeader_width,
            this.columnHeader_height});
            this.listView_papers.FullRowSelect = true;
            this.listView_papers.HideSelection = false;
            this.listView_papers.Location = new System.Drawing.Point(0, 27);
            this.listView_papers.Name = "listView_papers";
            this.listView_papers.Size = new System.Drawing.Size(312, 113);
            this.listView_papers.TabIndex = 13;
            this.listView_papers.UseCompatibleStateImageBehavior = false;
            this.listView_papers.View = System.Windows.Forms.View.Details;
            this.listView_papers.SelectedIndexChanged += new System.EventHandler(this.listView_papers_SelectedIndexChanged);
            this.listView_papers.DoubleClick += new System.EventHandler(this.listView_papers_DoubleClick);
            // 
            // columnHeader_paperName
            // 
            this.columnHeader_paperName.Text = "纸张名";
            this.columnHeader_paperName.Width = 100;
            // 
            // columnHeader_kind
            // 
            this.columnHeader_kind.Text = "纸张类型";
            this.columnHeader_kind.Width = 100;
            // 
            // columnHeader_width
            // 
            this.columnHeader_width.Text = "宽度";
            // 
            // columnHeader_height
            // 
            this.columnHeader_height.Text = "高度";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label_comment, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(318, 205);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // label_comment
            // 
            this.label_comment.AutoSize = true;
            this.label_comment.BackColor = System.Drawing.SystemColors.Info;
            this.label_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_comment.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label_comment.Location = new System.Drawing.Point(0, 0);
            this.label_comment.Margin = new System.Windows.Forms.Padding(0);
            this.label_comment.MaximumSize = new System.Drawing.Size(312, 0);
            this.label_comment.Name = "label_comment";
            this.label_comment.Padding = new System.Windows.Forms.Padding(8);
            this.label_comment.Size = new System.Drawing.Size(312, 32);
            this.label_comment.TabIndex = 0;
            this.label_comment.Visible = false;
            // 
            // SelectPaperDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(342, 264);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SelectPaperDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "选定纸张";
            this.Load += new System.EventHandler(this.SelectPaperDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_printerName;
        private System.Windows.Forms.TextBox textBox_paperName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView listView_papers;
        private System.Windows.Forms.ColumnHeader columnHeader_paperName;
        private System.Windows.Forms.ColumnHeader columnHeader_kind;
        private System.Windows.Forms.ColumnHeader columnHeader_width;
        private System.Windows.Forms.ColumnHeader columnHeader_height;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label_comment;
    }
}