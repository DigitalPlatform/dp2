namespace dp2Circulation
{
    partial class SelectPatronDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectPatronDialog));
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer_rightMain = new System.Windows.Forms.SplitContainer();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.splitContainer_itemInfoMain = new System.Windows.Forms.SplitContainer();
            this.listView_items = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_barcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_gender = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_department = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_idCardNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_recpath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.webBrowser_patron = new System.Windows.Forms.WebBrowser();
            this.label_colorBar = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_message = new System.Windows.Forms.ToolStripLabel();
            this.tableLayoutPanel_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rightMain)).BeginInit();
            this.splitContainer_rightMain.Panel1.SuspendLayout();
            this.splitContainer_rightMain.Panel2.SuspendLayout();
            this.splitContainer_rightMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemInfoMain)).BeginInit();
            this.splitContainer_itemInfoMain.Panel1.SuspendLayout();
            this.splitContainer_itemInfoMain.Panel2.SuspendLayout();
            this.splitContainer_itemInfoMain.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_main.ColumnCount = 2;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_rightMain, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_colorBar, 0, 0);
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(11, 11);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 1;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(440, 276);
            this.tableLayoutPanel_main.TabIndex = 6;
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
            this.splitContainer_rightMain.Panel1.Padding = new System.Windows.Forms.Padding(6);
            // 
            // splitContainer_rightMain.Panel2
            // 
            this.splitContainer_rightMain.Panel2.Controls.Add(this.splitContainer_itemInfoMain);
            this.splitContainer_rightMain.Size = new System.Drawing.Size(395, 276);
            this.splitContainer_rightMain.SplitterDistance = 43;
            this.splitContainer_rightMain.SplitterWidth = 8;
            this.splitContainer_rightMain.TabIndex = 0;
            // 
            // textBox_message
            // 
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_message.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(6, 6);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(383, 31);
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
            this.splitContainer_itemInfoMain.Panel2.Controls.Add(this.webBrowser_patron);
            this.splitContainer_itemInfoMain.Size = new System.Drawing.Size(395, 225);
            this.splitContainer_itemInfoMain.SplitterDistance = 110;
            this.splitContainer_itemInfoMain.SplitterWidth = 8;
            this.splitContainer_itemInfoMain.TabIndex = 1;
            // 
            // listView_items
            // 
            this.listView_items.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView_items.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_barcode,
            this.columnHeader_state,
            this.columnHeader_name,
            this.columnHeader_gender,
            this.columnHeader_department,
            this.columnHeader_idCardNumber,
            this.columnHeader_comment,
            this.columnHeader_recpath});
            this.listView_items.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_items.FullRowSelect = true;
            this.listView_items.HideSelection = false;
            this.listView_items.Location = new System.Drawing.Point(0, 0);
            this.listView_items.Margin = new System.Windows.Forms.Padding(0);
            this.listView_items.MultiSelect = false;
            this.listView_items.Name = "listView_items";
            this.listView_items.Size = new System.Drawing.Size(395, 110);
            this.listView_items.TabIndex = 0;
            this.listView_items.UseCompatibleStateImageBehavior = false;
            this.listView_items.View = System.Windows.Forms.View.Details;
            this.listView_items.SelectedIndexChanged += new System.EventHandler(this.listView_items_SelectedIndexChanged);
            this.listView_items.DoubleClick += new System.EventHandler(this.listView_items_DoubleClick);
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "证条码号";
            this.columnHeader_barcode.Width = 150;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 100;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "姓名";
            this.columnHeader_name.Width = 150;
            // 
            // columnHeader_gender
            // 
            this.columnHeader_gender.Text = "性别";
            this.columnHeader_gender.Width = 50;
            // 
            // columnHeader_department
            // 
            this.columnHeader_department.Text = "单位";
            this.columnHeader_department.Width = 150;
            // 
            // columnHeader_idCardNumber
            // 
            this.columnHeader_idCardNumber.Text = "身份证号";
            this.columnHeader_idCardNumber.Width = 150;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "附注";
            this.columnHeader_comment.Width = 150;
            // 
            // columnHeader_recpath
            // 
            this.columnHeader_recpath.Text = "册记录路径";
            this.columnHeader_recpath.Width = 100;
            // 
            // webBrowser_patron
            // 
            this.webBrowser_patron.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_patron.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_patron.Margin = new System.Windows.Forms.Padding(2);
            this.webBrowser_patron.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_patron.Name = "webBrowser_patron";
            this.webBrowser_patron.Size = new System.Drawing.Size(395, 107);
            this.webBrowser_patron.TabIndex = 2;
            this.webBrowser_patron.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_patron_DocumentCompleted);
            // 
            // label_colorBar
            // 
            this.label_colorBar.BackColor = System.Drawing.Color.Green;
            this.label_colorBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_colorBar.Location = new System.Drawing.Point(0, 0);
            this.label_colorBar.Margin = new System.Windows.Forms.Padding(0);
            this.label_colorBar.Name = "label_colorBar";
            this.label_colorBar.Size = new System.Drawing.Size(45, 276);
            this.label_colorBar.TabIndex = 1;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(376, 291);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(297, 291);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_stop,
            this.toolStripLabel_message});
            this.toolStrip1.Location = new System.Drawing.Point(9, 291);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(286, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_stop
            // 
            this.toolStripButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stop.Image")));
            this.toolStripButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_stop.Name = "toolStripButton_stop";
            this.toolStripButton_stop.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_stop.Text = "停止";
            // 
            // toolStripLabel_message
            // 
            this.toolStripLabel_message.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_message.Name = "toolStripLabel_message";
            this.toolStripLabel_message.Size = new System.Drawing.Size(17, 22);
            this.toolStripLabel_message.Text = "...";
            // 
            // SelectPatronDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(462, 325);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelectPatronDialog";
            this.ShowInTaskbar = false;
            this.Text = "请选择读者记录";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectPatronDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SelectPatronDialog_FormClosed);
            this.Load += new System.EventHandler(this.SelectPatronDialog_Load);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.splitContainer_rightMain.Panel1.ResumeLayout(false);
            this.splitContainer_rightMain.Panel1.PerformLayout();
            this.splitContainer_rightMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rightMain)).EndInit();
            this.splitContainer_rightMain.ResumeLayout(false);
            this.splitContainer_itemInfoMain.Panel1.ResumeLayout(false);
            this.splitContainer_itemInfoMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_itemInfoMain)).EndInit();
            this.splitContainer_itemInfoMain.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.SplitContainer splitContainer_rightMain;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.SplitContainer splitContainer_itemInfoMain;
        private DigitalPlatform.GUI.ListViewNF listView_items;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_gender;
        private System.Windows.Forms.ColumnHeader columnHeader_department;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.Label label_colorBar;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.WebBrowser webBrowser_patron;
        private System.Windows.Forms.ColumnHeader columnHeader_idCardNumber;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_stop;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_message;
    }
}