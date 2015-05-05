namespace dp2Circulation
{
    partial class CompareReaderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareReaderForm));
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_existing = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.readerEditControl_existing = new dp2Circulation.ReaderEditControl();
            this.tableLayoutPanel_unSaved = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.readerEditControl_unSaved = new dp2Circulation.ReaderEditControl();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_existing.SuspendLayout();
            this.tableLayoutPanel_unSaved.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(9, 10);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(428, 43);
            this.textBox_message.TabIndex = 0;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(10, 58);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_existing);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_unSaved);
            this.splitContainer_main.Size = new System.Drawing.Size(427, 259);
            this.splitContainer_main.SplitterDistance = 211;
            this.splitContainer_main.SplitterWidth = 3;
            this.splitContainer_main.TabIndex = 1;
            // 
            // tableLayoutPanel_existing
            // 
            this.tableLayoutPanel_existing.ColumnCount = 1;
            this.tableLayoutPanel_existing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_existing.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_existing.Controls.Add(this.readerEditControl_existing, 0, 1);
            this.tableLayoutPanel_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_existing.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_existing.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_existing.Name = "tableLayoutPanel_existing";
            this.tableLayoutPanel_existing.RowCount = 2;
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_existing.Size = new System.Drawing.Size(211, 259);
            this.tableLayoutPanel_existing.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据库中记录";
            // 
            // readerEditControl_existing
            // 
            this.readerEditControl_existing.Address = "";
            this.readerEditControl_existing.Barcode = "";
            this.readerEditControl_existing.CardNumber = "";
            this.readerEditControl_existing.Changed = false;
            this.readerEditControl_existing.Comment = "";
            this.readerEditControl_existing.CreateDate = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_existing.DateOfBirth = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_existing.Department = "";
            this.readerEditControl_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.readerEditControl_existing.Email = "";
            this.readerEditControl_existing.ExpireDate = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_existing.Foregift = "";
            this.readerEditControl_existing.Gender = "";
            this.readerEditControl_existing.HireExpireDate = "";
            this.readerEditControl_existing.HirePeriod = "";
            this.readerEditControl_existing.IdCardNumber = "";
            this.readerEditControl_existing.Initializing = true;
            this.readerEditControl_existing.Location = new System.Drawing.Point(2, 14);
            this.readerEditControl_existing.Margin = new System.Windows.Forms.Padding(2);
            this.readerEditControl_existing.Name = "readerEditControl_existing";
            this.readerEditControl_existing.NameString = "";
            this.readerEditControl_existing.Post = "";
            this.readerEditControl_existing.ReaderType = "";
            this.readerEditControl_existing.RecPath = "";
            this.readerEditControl_existing.Size = new System.Drawing.Size(207, 243);
            this.readerEditControl_existing.State = "";
            this.readerEditControl_existing.TabIndex = 1;
            this.readerEditControl_existing.Tel = "";
            this.readerEditControl_existing.GetLibraryCode += new dp2Circulation.GetLibraryCodeEventHandler(this.readerEditControl_existing_GetLibraryCode);
            // 
            // tableLayoutPanel_unSaved
            // 
            this.tableLayoutPanel_unSaved.ColumnCount = 1;
            this.tableLayoutPanel_unSaved.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_unSaved.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel_unSaved.Controls.Add(this.readerEditControl_unSaved, 0, 1);
            this.tableLayoutPanel_unSaved.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_unSaved.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_unSaved.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_unSaved.Name = "tableLayoutPanel_unSaved";
            this.tableLayoutPanel_unSaved.RowCount = 2;
            this.tableLayoutPanel_unSaved.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_unSaved.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_unSaved.Size = new System.Drawing.Size(213, 259);
            this.tableLayoutPanel_unSaved.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "尚未保存的记录";
            // 
            // readerEditControl_unSaved
            // 
            this.readerEditControl_unSaved.Address = "";
            this.readerEditControl_unSaved.Barcode = "";
            this.readerEditControl_unSaved.CardNumber = "";
            this.readerEditControl_unSaved.Changed = false;
            this.readerEditControl_unSaved.Comment = "";
            this.readerEditControl_unSaved.CreateDate = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_unSaved.DateOfBirth = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_unSaved.Department = "";
            this.readerEditControl_unSaved.Dock = System.Windows.Forms.DockStyle.Fill;
            this.readerEditControl_unSaved.Email = "";
            this.readerEditControl_unSaved.ExpireDate = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_unSaved.Foregift = "";
            this.readerEditControl_unSaved.Gender = "";
            this.readerEditControl_unSaved.HireExpireDate = "";
            this.readerEditControl_unSaved.HirePeriod = "";
            this.readerEditControl_unSaved.IdCardNumber = "";
            this.readerEditControl_unSaved.Initializing = true;
            this.readerEditControl_unSaved.Location = new System.Drawing.Point(2, 14);
            this.readerEditControl_unSaved.Margin = new System.Windows.Forms.Padding(2);
            this.readerEditControl_unSaved.Name = "readerEditControl_unSaved";
            this.readerEditControl_unSaved.NameString = "";
            this.readerEditControl_unSaved.Post = "";
            this.readerEditControl_unSaved.ReaderType = "";
            this.readerEditControl_unSaved.RecPath = "";
            this.readerEditControl_unSaved.Size = new System.Drawing.Size(209, 243);
            this.readerEditControl_unSaved.State = "";
            this.readerEditControl_unSaved.TabIndex = 1;
            this.readerEditControl_unSaved.Tel = "";
            this.readerEditControl_unSaved.GetLibraryCode += new dp2Circulation.GetLibraryCodeEventHandler(this.readerEditControl_unSaved_GetLibraryCode);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(320, 322);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(380, 322);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // CompareReaderForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(446, 354);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.textBox_message);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "CompareReaderForm";
            this.ShowInTaskbar = false;
            this.Text = "比对读者信息";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CompareReaderForm_FormClosed);
            this.Load += new System.EventHandler(this.CompareReaderForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_existing.ResumeLayout(false);
            this.tableLayoutPanel_existing.PerformLayout();
            this.tableLayoutPanel_unSaved.ResumeLayout(false);
            this.tableLayoutPanel_unSaved.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_existing;
        private System.Windows.Forms.Label label1;
        private ReaderEditControl readerEditControl_existing;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_unSaved;
        private System.Windows.Forms.Label label2;
        private ReaderEditControl readerEditControl_unSaved;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}