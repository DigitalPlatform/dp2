namespace dp2Circulation
{
    partial class ItemBarcodeDupDlg
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
            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemBarcodeDupDlg));
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.splitContainer_itemInfoMain = new System.Windows.Forms.SplitContainer();
            this.listView_items = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_recpath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_barcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_price = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_bookType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_registerNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_mergeComment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_batchNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_borrower = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_borrowDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_borrowPeriod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer_buttom = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_itemRecord = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.entityEditControl1 = new dp2Circulation.EntityEditControl();
            this.tableLayoutPanel_biblio = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer_rightMain = new System.Windows.Forms.SplitContainer();
            this.label_colorBar = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemInfoMain)).BeginInit();
            this.splitContainer_itemInfoMain.Panel1.SuspendLayout();
            this.splitContainer_itemInfoMain.Panel2.SuspendLayout();
            this.splitContainer_itemInfoMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_buttom)).BeginInit();
            this.splitContainer_buttom.Panel1.SuspendLayout();
            this.splitContainer_buttom.Panel2.SuspendLayout();
            this.splitContainer_buttom.SuspendLayout();
            this.tableLayoutPanel_itemRecord.SuspendLayout();
            this.tableLayoutPanel_biblio.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rightMain)).BeginInit();
            this.splitContainer_rightMain.Panel1.SuspendLayout();
            this.splitContainer_rightMain.Panel2.SuspendLayout();
            this.splitContainer_rightMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_message
            // 
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_message.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(0, 0);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(398, 52);
            this.textBox_message.TabIndex = 0;
            // 
            // splitContainer_itemInfoMain
            // 
            this.splitContainer_itemInfoMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_itemInfoMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_itemInfoMain.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_itemInfoMain.Name = "splitContainer_itemInfoMain";
            this.splitContainer_itemInfoMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_itemInfoMain.Panel1
            // 
            this.splitContainer_itemInfoMain.Panel1.Controls.Add(this.listView_items);
            // 
            // splitContainer_itemInfoMain.Panel2
            // 
            this.splitContainer_itemInfoMain.Panel2.Controls.Add(this.splitContainer_buttom);
            this.splitContainer_itemInfoMain.Size = new System.Drawing.Size(398, 227);
            this.splitContainer_itemInfoMain.SplitterDistance = 65;
            this.splitContainer_itemInfoMain.SplitterWidth = 3;
            this.splitContainer_itemInfoMain.TabIndex = 1;
            // 
            // listView_items
            // 
            this.listView_items.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_recpath,
            this.columnHeader_barcode,
            this.columnHeader_state,
            this.columnHeader_location,
            this.columnHeader_price,
            this.columnHeader_bookType,
            this.columnHeader_registerNo,
            this.columnHeader_comment,
            this.columnHeader_mergeComment,
            this.columnHeader_batchNo,
            this.columnHeader_borrower,
            this.columnHeader_borrowDate,
            this.columnHeader_borrowPeriod});
            this.listView_items.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_items.FullRowSelect = true;
            this.listView_items.HideSelection = false;
            this.listView_items.Location = new System.Drawing.Point(0, 0);
            this.listView_items.Margin = new System.Windows.Forms.Padding(0);
            this.listView_items.MultiSelect = false;
            this.listView_items.Name = "listView_items";
            this.listView_items.Size = new System.Drawing.Size(398, 65);
            this.listView_items.TabIndex = 0;
            this.listView_items.UseCompatibleStateImageBehavior = false;
            this.listView_items.View = System.Windows.Forms.View.Details;
            this.listView_items.SelectedIndexChanged += new System.EventHandler(this.listView_items_SelectedIndexChanged);
            this.listView_items.DoubleClick += new System.EventHandler(this.listView_items_DoubleClick);
            // 
            // columnHeader_recpath
            // 
            this.columnHeader_recpath.Text = "册记录路径";
            this.columnHeader_recpath.Width = 150;
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "册条码号";
            this.columnHeader_barcode.Width = 150;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 100;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "馆藏地点";
            this.columnHeader_location.Width = 150;
            // 
            // columnHeader_price
            // 
            this.columnHeader_price.Text = "册价格";
            this.columnHeader_price.Width = 150;
            // 
            // columnHeader_bookType
            // 
            this.columnHeader_bookType.Text = "册类型";
            this.columnHeader_bookType.Width = 150;
            // 
            // columnHeader_registerNo
            // 
            this.columnHeader_registerNo.Text = "登录号";
            this.columnHeader_registerNo.Width = 150;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "附注";
            this.columnHeader_comment.Width = 150;
            // 
            // columnHeader_mergeComment
            // 
            this.columnHeader_mergeComment.Text = "合并注释";
            this.columnHeader_mergeComment.Width = 150;
            // 
            // columnHeader_batchNo
            // 
            this.columnHeader_batchNo.Text = "批次号";
            // 
            // columnHeader_borrower
            // 
            this.columnHeader_borrower.Text = "借阅者";
            this.columnHeader_borrower.Width = 150;
            // 
            // columnHeader_borrowDate
            // 
            this.columnHeader_borrowDate.Text = "借阅日期";
            this.columnHeader_borrowDate.Width = 150;
            // 
            // columnHeader_borrowPeriod
            // 
            this.columnHeader_borrowPeriod.Text = "借阅期限";
            this.columnHeader_borrowPeriod.Width = 150;
            // 
            // splitContainer_buttom
            // 
            this.splitContainer_buttom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_buttom.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_buttom.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_buttom.Name = "splitContainer_buttom";
            // 
            // splitContainer_buttom.Panel1
            // 
            this.splitContainer_buttom.Panel1.Controls.Add(this.tableLayoutPanel_itemRecord);
            // 
            // splitContainer_buttom.Panel2
            // 
            this.splitContainer_buttom.Panel2.Controls.Add(this.tableLayoutPanel_biblio);
            this.splitContainer_buttom.Size = new System.Drawing.Size(398, 159);
            this.splitContainer_buttom.SplitterDistance = 185;
            this.splitContainer_buttom.SplitterWidth = 3;
            this.splitContainer_buttom.TabIndex = 0;
            // 
            // tableLayoutPanel_itemRecord
            // 
            this.tableLayoutPanel_itemRecord.ColumnCount = 1;
            this.tableLayoutPanel_itemRecord.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_itemRecord.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_itemRecord.Controls.Add(this.entityEditControl1, 0, 1);
            this.tableLayoutPanel_itemRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_itemRecord.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_itemRecord.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_itemRecord.Name = "tableLayoutPanel_itemRecord";
            this.tableLayoutPanel_itemRecord.RowCount = 2;
            this.tableLayoutPanel_itemRecord.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_itemRecord.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_itemRecord.Size = new System.Drawing.Size(185, 159);
            this.tableLayoutPanel_itemRecord.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "册记录";
            // 
            // entityEditControl1
            // 
            this.entityEditControl1.AccessNo = "";
            this.entityEditControl1.AutoScroll = true;
            this.entityEditControl1.Barcode = "";
            this.entityEditControl1.BatchNo = "";
            this.entityEditControl1.Binding = "";
            this.entityEditControl1.BookType = "";
            this.entityEditControl1.BorrowDate = "";
            this.entityEditControl1.Borrower = "";
            this.entityEditControl1.BorrowPeriod = "";
            this.entityEditControl1.Changed = false;
            this.entityEditControl1.Comment = "";
            this.entityEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl1.Initializing = true;
            this.entityEditControl1.Intact = "";
            this.entityEditControl1.Location = new System.Drawing.Point(2, 14);
            this.entityEditControl1.LocationString = "";
            this.entityEditControl1.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl1.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl1.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl1.MergeComment = "";
            this.entityEditControl1.MinimumSize = new System.Drawing.Size(56, 0);
            this.entityEditControl1.Name = "entityEditControl1";
            this.entityEditControl1.Operations = "";
            this.entityEditControl1.ParentId = "";
            this.entityEditControl1.Price = "";
            this.entityEditControl1.PublishTime = "";
            this.entityEditControl1.RecPath = "";
            this.entityEditControl1.RefID = "";
            this.entityEditControl1.RegisterNo = "";
            this.entityEditControl1.Seller = "";
            this.entityEditControl1.Size = new System.Drawing.Size(181, 143);
            this.entityEditControl1.Source = "";
            this.entityEditControl1.State = "";
            this.entityEditControl1.TabIndex = 1;
            this.entityEditControl1.Volume = "";
            // 
            // tableLayoutPanel_biblio
            // 
            this.tableLayoutPanel_biblio.ColumnCount = 1;
            this.tableLayoutPanel_biblio.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblio.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel_biblio.Controls.Add(this.webBrowser_biblio, 0, 1);
            this.tableLayoutPanel_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_biblio.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_biblio.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_biblio.Name = "tableLayoutPanel_biblio";
            this.tableLayoutPanel_biblio.RowCount = 2;
            this.tableLayoutPanel_biblio.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblio.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblio.Size = new System.Drawing.Size(210, 159);
            this.tableLayoutPanel_biblio.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(2, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "书目信息";
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(2, 14);
            this.webBrowser_biblio.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(206, 143);
            this.webBrowser_biblio.TabIndex = 1;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(336, 297);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "重试";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(397, 297);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_main.ColumnCount = 2;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_rightMain, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_colorBar, 0, 0);
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 1;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(443, 282);
            this.tableLayoutPanel_main.TabIndex = 3;
            // 
            // splitContainer_rightMain
            // 
            this.splitContainer_rightMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_rightMain.Location = new System.Drawing.Point(45, 0);
            this.splitContainer_rightMain.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_rightMain.Name = "splitContainer_rightMain";
            this.splitContainer_rightMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_rightMain.Panel1
            // 
            this.splitContainer_rightMain.Panel1.Controls.Add(this.textBox_message);
            // 
            // splitContainer_rightMain.Panel2
            // 
            this.splitContainer_rightMain.Panel2.Controls.Add(this.splitContainer_itemInfoMain);
            this.splitContainer_rightMain.Size = new System.Drawing.Size(398, 282);
            this.splitContainer_rightMain.SplitterDistance = 52;
            this.splitContainer_rightMain.SplitterWidth = 3;
            this.splitContainer_rightMain.TabIndex = 0;
            // 
            // label_colorBar
            // 
            this.label_colorBar.BackColor = System.Drawing.Color.Green;
            this.label_colorBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_colorBar.Location = new System.Drawing.Point(0, 0);
            this.label_colorBar.Margin = new System.Windows.Forms.Padding(0);
            this.label_colorBar.Name = "label_colorBar";
            this.label_colorBar.Size = new System.Drawing.Size(45, 282);
            this.label_colorBar.TabIndex = 1;
            // 
            // ItemBarcodeDupDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(462, 325);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ItemBarcodeDupDlg";
            this.ShowInTaskbar = false;
            this.Text = "册条码号发生重复";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ItemBarcodeDupDlg_FormClosed);
            this.Load += new System.EventHandler(this.ItemBarcodeDupDlg_Load);
            this.splitContainer_itemInfoMain.Panel1.ResumeLayout(false);
            this.splitContainer_itemInfoMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemInfoMain)).EndInit();
            this.splitContainer_itemInfoMain.ResumeLayout(false);
            this.splitContainer_buttom.Panel1.ResumeLayout(false);
            this.splitContainer_buttom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_buttom)).EndInit();
            this.splitContainer_buttom.ResumeLayout(false);
            this.tableLayoutPanel_itemRecord.ResumeLayout(false);
            this.tableLayoutPanel_itemRecord.PerformLayout();
            this.tableLayoutPanel_biblio.ResumeLayout(false);
            this.tableLayoutPanel_biblio.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.splitContainer_rightMain.Panel1.ResumeLayout(false);
            this.splitContainer_rightMain.Panel1.PerformLayout();
            this.splitContainer_rightMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rightMain)).EndInit();
            this.splitContainer_rightMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.SplitContainer splitContainer_itemInfoMain;
        private DigitalPlatform.GUI.ListViewNF listView_items;
        private System.Windows.Forms.SplitContainer splitContainer_buttom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_itemRecord;
        private System.Windows.Forms.Label label1;
        private EntityEditControl entityEditControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_biblio;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.WebBrowser webBrowser_biblio;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ColumnHeader columnHeader_price;
        private System.Windows.Forms.ColumnHeader columnHeader_bookType;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_borrower;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowDate;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowPeriod;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ColumnHeader columnHeader_registerNo;
        private System.Windows.Forms.ColumnHeader columnHeader_mergeComment;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.SplitContainer splitContainer_rightMain;
        private System.Windows.Forms.Label label_colorBar;
    }
}