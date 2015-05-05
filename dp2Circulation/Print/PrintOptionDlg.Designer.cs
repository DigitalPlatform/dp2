namespace dp2Circulation
{
    partial class PrintOptionDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintOptionDlg));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_normal = new System.Windows.Forms.TabPage();
            this.textBox_maxSummaryChars = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_tableTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_pageFooter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_pageHeader = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_linesPerPage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_columns = new System.Windows.Forms.TabPage();
            this.button_columns_modify = new System.Windows.Forms.Button();
            this.button_columns_delete = new System.Windows.Forms.Button();
            this.button_columns_new = new System.Windows.Forms.Button();
            this.button_columns_moveDown = new System.Windows.Forms.Button();
            this.button_columns_moveUp = new System.Windows.Forms.Button();
            this.listView_columns = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_caption = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_maxChars = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_evalue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_templates = new System.Windows.Forms.TabPage();
            this.splitContainer_templates = new System.Windows.Forms.SplitContainer();
            this.listView_templates = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_template_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_template_filepath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_templates_content = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.columnHeader_widthChars = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl_main.SuspendLayout();
            this.tabPage_normal.SuspendLayout();
            this.tabPage_columns.SuspendLayout();
            this.tabPage_templates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_templates)).BeginInit();
            this.splitContainer_templates.Panel1.SuspendLayout();
            this.splitContainer_templates.Panel2.SuspendLayout();
            this.splitContainer_templates.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_normal);
            this.tabControl_main.Controls.Add(this.tabPage_columns);
            this.tabControl_main.Controls.Add(this.tabPage_templates);
            this.tabControl_main.Location = new System.Drawing.Point(9, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(399, 214);
            this.tabControl_main.TabIndex = 10;
            // 
            // tabPage_normal
            // 
            this.tabPage_normal.Controls.Add(this.textBox_maxSummaryChars);
            this.tabPage_normal.Controls.Add(this.label5);
            this.tabPage_normal.Controls.Add(this.textBox_tableTitle);
            this.tabPage_normal.Controls.Add(this.label4);
            this.tabPage_normal.Controls.Add(this.textBox_pageFooter);
            this.tabPage_normal.Controls.Add(this.label3);
            this.tabPage_normal.Controls.Add(this.textBox_pageHeader);
            this.tabPage_normal.Controls.Add(this.label2);
            this.tabPage_normal.Controls.Add(this.textBox_linesPerPage);
            this.tabPage_normal.Controls.Add(this.label1);
            this.tabPage_normal.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normal.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_normal.Name = "tabPage_normal";
            this.tabPage_normal.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_normal.Size = new System.Drawing.Size(391, 188);
            this.tabPage_normal.TabIndex = 0;
            this.tabPage_normal.Text = "一般参数";
            this.tabPage_normal.UseVisualStyleBackColor = true;
            // 
            // textBox_maxSummaryChars
            // 
            this.textBox_maxSummaryChars.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_maxSummaryChars.Location = new System.Drawing.Point(106, 132);
            this.textBox_maxSummaryChars.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_maxSummaryChars.Name = "textBox_maxSummaryChars";
            this.textBox_maxSummaryChars.Size = new System.Drawing.Size(84, 21);
            this.textBox_maxSummaryChars.TabIndex = 9;
            this.textBox_maxSummaryChars.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 134);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "摘要字数上限(&S):";
            this.label5.Visible = false;
            // 
            // textBox_tableTitle
            // 
            this.textBox_tableTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tableTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_tableTitle.Location = new System.Drawing.Point(106, 72);
            this.textBox_tableTitle.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_tableTitle.Name = "textBox_tableTitle";
            this.textBox_tableTitle.Size = new System.Drawing.Size(208, 21);
            this.textBox_tableTitle.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 74);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "表格标题(&T):";
            // 
            // textBox_pageFooter
            // 
            this.textBox_pageFooter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_pageFooter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_pageFooter.Location = new System.Drawing.Point(106, 35);
            this.textBox_pageFooter.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_pageFooter.Name = "textBox_pageFooter";
            this.textBox_pageFooter.Size = new System.Drawing.Size(208, 21);
            this.textBox_pageFooter.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 38);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "页脚文字(&F):";
            // 
            // textBox_pageHeader
            // 
            this.textBox_pageHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_pageHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_pageHeader.Location = new System.Drawing.Point(106, 10);
            this.textBox_pageHeader.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_pageHeader.Name = "textBox_pageHeader";
            this.textBox_pageHeader.Size = new System.Drawing.Size(208, 21);
            this.textBox_pageHeader.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 13);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "页眉文字(&H):";
            // 
            // textBox_linesPerPage
            // 
            this.textBox_linesPerPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_linesPerPage.Location = new System.Drawing.Point(106, 107);
            this.textBox_linesPerPage.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_linesPerPage.Name = "textBox_linesPerPage";
            this.textBox_linesPerPage.Size = new System.Drawing.Size(84, 21);
            this.textBox_linesPerPage.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 110);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "每页行数(&L):";
            // 
            // tabPage_columns
            // 
            this.tabPage_columns.Controls.Add(this.button_columns_modify);
            this.tabPage_columns.Controls.Add(this.button_columns_delete);
            this.tabPage_columns.Controls.Add(this.button_columns_new);
            this.tabPage_columns.Controls.Add(this.button_columns_moveDown);
            this.tabPage_columns.Controls.Add(this.button_columns_moveUp);
            this.tabPage_columns.Controls.Add(this.listView_columns);
            this.tabPage_columns.Location = new System.Drawing.Point(4, 22);
            this.tabPage_columns.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_columns.Name = "tabPage_columns";
            this.tabPage_columns.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_columns.Size = new System.Drawing.Size(391, 188);
            this.tabPage_columns.TabIndex = 1;
            this.tabPage_columns.Text = "栏目定义";
            this.tabPage_columns.UseVisualStyleBackColor = true;
            // 
            // button_columns_modify
            // 
            this.button_columns_modify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_columns_modify.Enabled = false;
            this.button_columns_modify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_columns_modify.Location = new System.Drawing.Point(332, 105);
            this.button_columns_modify.Margin = new System.Windows.Forms.Padding(2);
            this.button_columns_modify.Name = "button_columns_modify";
            this.button_columns_modify.Size = new System.Drawing.Size(56, 22);
            this.button_columns_modify.TabIndex = 4;
            this.button_columns_modify.Text = "修改(&M)";
            this.button_columns_modify.UseVisualStyleBackColor = true;
            this.button_columns_modify.Click += new System.EventHandler(this.button_columns_modify_Click);
            // 
            // button_columns_delete
            // 
            this.button_columns_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_columns_delete.Enabled = false;
            this.button_columns_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_columns_delete.Location = new System.Drawing.Point(332, 132);
            this.button_columns_delete.Margin = new System.Windows.Forms.Padding(2);
            this.button_columns_delete.Name = "button_columns_delete";
            this.button_columns_delete.Size = new System.Drawing.Size(56, 22);
            this.button_columns_delete.TabIndex = 5;
            this.button_columns_delete.Text = "删除(&R)";
            this.button_columns_delete.UseVisualStyleBackColor = true;
            this.button_columns_delete.Click += new System.EventHandler(this.button_columns_delete_Click);
            // 
            // button_columns_new
            // 
            this.button_columns_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_columns_new.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_columns_new.Location = new System.Drawing.Point(332, 78);
            this.button_columns_new.Margin = new System.Windows.Forms.Padding(2);
            this.button_columns_new.Name = "button_columns_new";
            this.button_columns_new.Size = new System.Drawing.Size(56, 22);
            this.button_columns_new.TabIndex = 3;
            this.button_columns_new.Text = "新增(&N)";
            this.button_columns_new.UseVisualStyleBackColor = true;
            this.button_columns_new.Click += new System.EventHandler(this.button_columns_new_Click);
            // 
            // button_columns_moveDown
            // 
            this.button_columns_moveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_columns_moveDown.Enabled = false;
            this.button_columns_moveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_columns_moveDown.Location = new System.Drawing.Point(332, 33);
            this.button_columns_moveDown.Margin = new System.Windows.Forms.Padding(2);
            this.button_columns_moveDown.Name = "button_columns_moveDown";
            this.button_columns_moveDown.Size = new System.Drawing.Size(56, 22);
            this.button_columns_moveDown.TabIndex = 2;
            this.button_columns_moveDown.Text = "下移(&D)";
            this.button_columns_moveDown.UseVisualStyleBackColor = true;
            this.button_columns_moveDown.Click += new System.EventHandler(this.button_columns_moveDown_Click);
            // 
            // button_columns_moveUp
            // 
            this.button_columns_moveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_columns_moveUp.Enabled = false;
            this.button_columns_moveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_columns_moveUp.Location = new System.Drawing.Point(332, 6);
            this.button_columns_moveUp.Margin = new System.Windows.Forms.Padding(2);
            this.button_columns_moveUp.Name = "button_columns_moveUp";
            this.button_columns_moveUp.Size = new System.Drawing.Size(56, 22);
            this.button_columns_moveUp.TabIndex = 1;
            this.button_columns_moveUp.Text = "上移(&U)";
            this.button_columns_moveUp.UseVisualStyleBackColor = true;
            this.button_columns_moveUp.Click += new System.EventHandler(this.button_columns_moveUp_Click);
            // 
            // listView_columns
            // 
            this.listView_columns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_columns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_columns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_caption,
            this.columnHeader_widthChars,
            this.columnHeader_maxChars,
            this.columnHeader_evalue});
            this.listView_columns.FullRowSelect = true;
            this.listView_columns.HideSelection = false;
            this.listView_columns.Location = new System.Drawing.Point(5, 6);
            this.listView_columns.Margin = new System.Windows.Forms.Padding(2);
            this.listView_columns.Name = "listView_columns";
            this.listView_columns.Size = new System.Drawing.Size(323, 182);
            this.listView_columns.TabIndex = 0;
            this.listView_columns.UseCompatibleStateImageBehavior = false;
            this.listView_columns.View = System.Windows.Forms.View.Details;
            this.listView_columns.SelectedIndexChanged += new System.EventHandler(this.listView_columns_SelectedIndexChanged);
            this.listView_columns.DoubleClick += new System.EventHandler(this.listView_columns_DoubleClick);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "栏目名";
            this.columnHeader_name.Width = 200;
            // 
            // columnHeader_caption
            // 
            this.columnHeader_caption.Text = "标题文字";
            this.columnHeader_caption.Width = 150;
            // 
            // columnHeader_maxChars
            // 
            this.columnHeader_maxChars.DisplayIndex = 2;
            this.columnHeader_maxChars.Text = "截断字符数";
            this.columnHeader_maxChars.Width = 100;
            // 
            // columnHeader_evalue
            // 
            this.columnHeader_evalue.DisplayIndex = 3;
            this.columnHeader_evalue.Text = "脚本";
            this.columnHeader_evalue.Width = 200;
            // 
            // tabPage_templates
            // 
            this.tabPage_templates.Controls.Add(this.splitContainer_templates);
            this.tabPage_templates.Location = new System.Drawing.Point(4, 22);
            this.tabPage_templates.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_templates.Name = "tabPage_templates";
            this.tabPage_templates.Size = new System.Drawing.Size(391, 188);
            this.tabPage_templates.TabIndex = 2;
            this.tabPage_templates.Text = "模板";
            this.tabPage_templates.UseVisualStyleBackColor = true;
            // 
            // splitContainer_templates
            // 
            this.splitContainer_templates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_templates.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_templates.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_templates.Name = "splitContainer_templates";
            this.splitContainer_templates.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_templates.Panel1
            // 
            this.splitContainer_templates.Panel1.Controls.Add(this.listView_templates);
            // 
            // splitContainer_templates.Panel2
            // 
            this.splitContainer_templates.Panel2.Controls.Add(this.textBox_templates_content);
            this.splitContainer_templates.Panel2.Controls.Add(this.label7);
            this.splitContainer_templates.Size = new System.Drawing.Size(391, 188);
            this.splitContainer_templates.SplitterDistance = 78;
            this.splitContainer_templates.SplitterWidth = 6;
            this.splitContainer_templates.TabIndex = 0;
            // 
            // listView_templates
            // 
            this.listView_templates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_templates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_templates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_template_name,
            this.columnHeader_template_filepath});
            this.listView_templates.FullRowSelect = true;
            this.listView_templates.HideSelection = false;
            this.listView_templates.Location = new System.Drawing.Point(2, 14);
            this.listView_templates.Margin = new System.Windows.Forms.Padding(2);
            this.listView_templates.Name = "listView_templates";
            this.listView_templates.Size = new System.Drawing.Size(388, 62);
            this.listView_templates.TabIndex = 1;
            this.listView_templates.UseCompatibleStateImageBehavior = false;
            this.listView_templates.View = System.Windows.Forms.View.Details;
            this.listView_templates.SelectedIndexChanged += new System.EventHandler(this.listView_templates_SelectedIndexChanged);
            this.listView_templates.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_templates_MouseUp);
            // 
            // columnHeader_template_name
            // 
            this.columnHeader_template_name.Text = "模板名";
            this.columnHeader_template_name.Width = 120;
            // 
            // columnHeader_template_filepath
            // 
            this.columnHeader_template_filepath.Text = "文件路径";
            this.columnHeader_template_filepath.Width = 522;
            // 
            // textBox_templates_content
            // 
            this.textBox_templates_content.AcceptsReturn = true;
            this.textBox_templates_content.AcceptsTab = true;
            this.textBox_templates_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_templates_content.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_templates_content.Enabled = false;
            this.textBox_templates_content.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_templates_content.HideSelection = false;
            this.textBox_templates_content.Location = new System.Drawing.Point(2, 14);
            this.textBox_templates_content.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_templates_content.MaxLength = 0;
            this.textBox_templates_content.Multiline = true;
            this.textBox_templates_content.Name = "textBox_templates_content";
            this.textBox_templates_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_templates_content.Size = new System.Drawing.Size(388, 85);
            this.textBox_templates_content.TabIndex = 2;
            this.textBox_templates_content.TextChanged += new System.EventHandler(this.textBox_templates_content_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(-1, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 1;
            this.label7.Text = "文件内容(&C):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(291, 229);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(352, 229);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 12;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // columnHeader_widthChars
            // 
            this.columnHeader_widthChars.DisplayIndex = 4;
            this.columnHeader_widthChars.Text = "栏宽字符欻";
            this.columnHeader_widthChars.Width = 100;
            // 
            // PrintOptionDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(417, 257);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PrintOptionDlg";
            this.ShowInTaskbar = false;
            this.Text = "打印参数配置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintOptionDlg_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrintOptionDlg_FormClosed);
            this.Load += new System.EventHandler(this.PrintOptionDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_normal.ResumeLayout(false);
            this.tabPage_normal.PerformLayout();
            this.tabPage_columns.ResumeLayout(false);
            this.tabPage_templates.ResumeLayout(false);
            this.splitContainer_templates.Panel1.ResumeLayout(false);
            this.splitContainer_templates.Panel2.ResumeLayout(false);
            this.splitContainer_templates.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_templates)).EndInit();
            this.splitContainer_templates.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_normal;
        private System.Windows.Forms.TabPage tabPage_columns;
        private System.Windows.Forms.TextBox textBox_maxSummaryChars;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_tableTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_pageFooter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_pageHeader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_linesPerPage;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.GUI.ListViewNF listView_columns;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.Button button_columns_moveUp;
        private System.Windows.Forms.Button button_columns_moveDown;
        private System.Windows.Forms.Button button_columns_delete;
        private System.Windows.Forms.Button button_columns_new;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_columns_modify;
        private System.Windows.Forms.ColumnHeader columnHeader_caption;
        private System.Windows.Forms.ColumnHeader columnHeader_maxChars;
        private System.Windows.Forms.TabPage tabPage_templates;
        private System.Windows.Forms.SplitContainer splitContainer_templates;
        private DigitalPlatform.GUI.ListViewNF listView_templates;
        private System.Windows.Forms.ColumnHeader columnHeader_template_name;
        private System.Windows.Forms.ColumnHeader columnHeader_template_filepath;
        private System.Windows.Forms.TextBox textBox_templates_content;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ColumnHeader columnHeader_evalue;
        private System.Windows.Forms.ColumnHeader columnHeader_widthChars;

    }
}