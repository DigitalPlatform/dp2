namespace dp2Circulation
{
    partial class CallNumberForm
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

            this.EventFinish.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CallNumberForm));
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBox_location = new System.Windows.Forms.ComboBox();
            this.button_searchClass = new System.Windows.Forms.Button();
            this.listView_number = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_callNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_barcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_biblioRecPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_lineType = new System.Windows.Forms.ImageList(this.components);
            this.button_searchDouble = new System.Windows.Forms.Button();
            this.button_getTailNumber = new System.Windows.Forms.Button();
            this.textBox_classNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label_message = new System.Windows.Forms.Label();
            this.groupBox_records = new System.Windows.Forms.GroupBox();
            this.textBox_firstEmptyNumber = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_returnBrowseCols = new System.Windows.Forms.CheckBox();
            this.checkBox_topmost = new System.Windows.Forms.CheckBox();
            this.textBox_maxNumber = new System.Windows.Forms.TextBox();
            this.label_maxNumber = new System.Windows.Forms.Label();
            this.button_pushTailNumber = new System.Windows.Forms.Button();
            this.button_copyMaxNumber = new System.Windows.Forms.Button();
            this.groupBox_tailNumber = new System.Windows.Forms.GroupBox();
            this.textBox_tailNumber = new System.Windows.Forms.TextBox();
            this.button_saveTailNumber = new System.Windows.Forms.Button();
            this.label_tailNumberTitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_query = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox_records.SuspendLayout();
            this.groupBox_tailNumber.SuspendLayout();
            this.tableLayoutPanel_query.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "册记录路径";
            this.columnHeader_path.Width = 150;
            // 
            // comboBox_location
            // 
            this.comboBox_location.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_location.FormattingEnabled = true;
            this.comboBox_location.Location = new System.Drawing.Point(192, 50);
            this.comboBox_location.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_location.Name = "comboBox_location";
            this.comboBox_location.Size = new System.Drawing.Size(345, 29);
            this.comboBox_location.TabIndex = 4;
            this.comboBox_location.DropDown += new System.EventHandler(this.comboBox_location_DropDown);
            this.comboBox_location.TextChanged += new System.EventHandler(this.comboBox_location_TextChanged);
            // 
            // button_searchClass
            // 
            this.button_searchClass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_searchClass.Location = new System.Drawing.Point(676, 18);
            this.button_searchClass.Margin = new System.Windows.Forms.Padding(4);
            this.button_searchClass.Name = "button_searchClass";
            this.button_searchClass.Size = new System.Drawing.Size(176, 38);
            this.button_searchClass.TabIndex = 1;
            this.button_searchClass.Text = "检索实体(&S)";
            this.button_searchClass.Click += new System.EventHandler(this.button_searchClass_Click);
            // 
            // listView_number
            // 
            this.listView_number.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_number.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_state,
            this.columnHeader_callNumber,
            this.columnHeader_summary,
            this.columnHeader_location,
            this.columnHeader_barcode,
            this.columnHeader_biblioRecPath});
            this.listView_number.FullRowSelect = true;
            this.listView_number.HideSelection = false;
            this.listView_number.LargeImageList = this.imageList_lineType;
            this.listView_number.Location = new System.Drawing.Point(7, 61);
            this.listView_number.Margin = new System.Windows.Forms.Padding(4);
            this.listView_number.Name = "listView_number";
            this.listView_number.Size = new System.Drawing.Size(841, 191);
            this.listView_number.SmallImageList = this.imageList_lineType;
            this.listView_number.TabIndex = 2;
            this.listView_number.UseCompatibleStateImageBehavior = false;
            this.listView_number.View = System.Windows.Forms.View.Details;
            this.listView_number.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_number_ColumnClick);
            this.listView_number.DoubleClick += new System.EventHandler(this.listView_number_DoubleClick);
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 150;
            // 
            // columnHeader_callNumber
            // 
            this.columnHeader_callNumber.Text = "索取号";
            this.columnHeader_callNumber.Width = 150;
            // 
            // columnHeader_summary
            // 
            this.columnHeader_summary.Text = "书目摘要";
            this.columnHeader_summary.Width = 219;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "馆藏地点";
            this.columnHeader_location.Width = 120;
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "册条码号";
            this.columnHeader_barcode.Width = 120;
            // 
            // columnHeader_biblioRecPath
            // 
            this.columnHeader_biblioRecPath.Text = "种记录路径";
            this.columnHeader_biblioRecPath.Width = 120;
            // 
            // imageList_lineType
            // 
            this.imageList_lineType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_lineType.ImageStream")));
            this.imageList_lineType.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_lineType.Images.SetKeyName(0, "document.ico");
            this.imageList_lineType.Images.SetKeyName(1, "error.ico");
            this.imageList_lineType.Images.SetKeyName(2, "1683_Lightbulb.ico");
            // 
            // button_searchDouble
            // 
            this.button_searchDouble.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_searchDouble.Location = new System.Drawing.Point(545, 4);
            this.button_searchDouble.Margin = new System.Windows.Forms.Padding(4);
            this.button_searchDouble.Name = "button_searchDouble";
            this.button_searchDouble.Size = new System.Drawing.Size(227, 38);
            this.button_searchDouble.TabIndex = 2;
            this.button_searchDouble.Text = "检索实体和尾号(&S)";
            this.button_searchDouble.Click += new System.EventHandler(this.button_searchDouble_Click);
            // 
            // button_getTailNumber
            // 
            this.button_getTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getTailNumber.Location = new System.Drawing.Point(549, 35);
            this.button_getTailNumber.Margin = new System.Windows.Forms.Padding(4);
            this.button_getTailNumber.Name = "button_getTailNumber";
            this.button_getTailNumber.Size = new System.Drawing.Size(117, 38);
            this.button_getTailNumber.TabIndex = 2;
            this.button_getTailNumber.Text = "获取(&G)";
            this.button_getTailNumber.UseVisualStyleBackColor = true;
            this.button_getTailNumber.Click += new System.EventHandler(this.button_getTailNumber_Click);
            // 
            // textBox_classNumber
            // 
            this.textBox_classNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_classNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_classNumber.Location = new System.Drawing.Point(192, 4);
            this.textBox_classNumber.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_classNumber.MinimumSize = new System.Drawing.Size(345, 4);
            this.textBox_classNumber.Name = "textBox_classNumber";
            this.textBox_classNumber.Size = new System.Drawing.Size(345, 31);
            this.textBox_classNumber.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(4, 46);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 37);
            this.label3.TabIndex = 3;
            this.label3.Text = "线索馆藏地点(&L):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_message
            // 
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Location = new System.Drawing.Point(4, 589);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(859, 28);
            this.label_message.TabIndex = 2;
            // 
            // groupBox_records
            // 
            this.groupBox_records.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_records.AutoSize = true;
            this.groupBox_records.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox_records.Controls.Add(this.textBox_firstEmptyNumber);
            this.groupBox_records.Controls.Add(this.label2);
            this.groupBox_records.Controls.Add(this.checkBox_returnBrowseCols);
            this.groupBox_records.Controls.Add(this.checkBox_topmost);
            this.groupBox_records.Controls.Add(this.textBox_maxNumber);
            this.groupBox_records.Controls.Add(this.label_maxNumber);
            this.groupBox_records.Controls.Add(this.button_pushTailNumber);
            this.groupBox_records.Controls.Add(this.button_searchClass);
            this.groupBox_records.Controls.Add(this.listView_number);
            this.groupBox_records.Controls.Add(this.button_copyMaxNumber);
            this.groupBox_records.Location = new System.Drawing.Point(4, 113);
            this.groupBox_records.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_records.Name = "groupBox_records";
            this.groupBox_records.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_records.Size = new System.Drawing.Size(859, 362);
            this.groupBox_records.TabIndex = 0;
            this.groupBox_records.TabStop = false;
            this.groupBox_records.Text = " 当前同类实体 ";
            // 
            // textBox_firstEmptyNumber
            // 
            this.textBox_firstEmptyNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_firstEmptyNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_firstEmptyNumber.Location = new System.Drawing.Point(532, 270);
            this.textBox_firstEmptyNumber.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_firstEmptyNumber.Name = "textBox_firstEmptyNumber";
            this.textBox_firstEmptyNumber.ReadOnly = true;
            this.textBox_firstEmptyNumber.Size = new System.Drawing.Size(102, 31);
            this.textBox_firstEmptyNumber.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(365, 273);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 21);
            this.label2.TabIndex = 8;
            this.label2.Text = "第一个空号(&E):";
            // 
            // checkBox_returnBrowseCols
            // 
            this.checkBox_returnBrowseCols.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_returnBrowseCols.AutoSize = true;
            this.checkBox_returnBrowseCols.Location = new System.Drawing.Point(495, 24);
            this.checkBox_returnBrowseCols.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_returnBrowseCols.Name = "checkBox_returnBrowseCols";
            this.checkBox_returnBrowseCols.Size = new System.Drawing.Size(174, 25);
            this.checkBox_returnBrowseCols.TabIndex = 0;
            this.checkBox_returnBrowseCols.Text = "返回浏览列(&B)";
            this.checkBox_returnBrowseCols.UseVisualStyleBackColor = true;
            // 
            // checkBox_topmost
            // 
            this.checkBox_topmost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_topmost.AutoSize = true;
            this.checkBox_topmost.Location = new System.Drawing.Point(7, 320);
            this.checkBox_topmost.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_topmost.Name = "checkBox_topmost";
            this.checkBox_topmost.Size = new System.Drawing.Size(111, 25);
            this.checkBox_topmost.TabIndex = 6;
            this.checkBox_topmost.Text = "浮动(&T)";
            this.checkBox_topmost.UseVisualStyleBackColor = true;
            this.checkBox_topmost.CheckedChanged += new System.EventHandler(this.checkBox_topmost_CheckedChanged);
            // 
            // textBox_maxNumber
            // 
            this.textBox_maxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_maxNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_maxNumber.Location = new System.Drawing.Point(255, 270);
            this.textBox_maxNumber.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_maxNumber.Name = "textBox_maxNumber";
            this.textBox_maxNumber.ReadOnly = true;
            this.textBox_maxNumber.Size = new System.Drawing.Size(102, 31);
            this.textBox_maxNumber.TabIndex = 4;
            // 
            // label_maxNumber
            // 
            this.label_maxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_maxNumber.AutoSize = true;
            this.label_maxNumber.Location = new System.Drawing.Point(4, 273);
            this.label_maxNumber.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_maxNumber.Name = "label_maxNumber";
            this.label_maxNumber.Size = new System.Drawing.Size(243, 21);
            this.label_maxNumber.TabIndex = 3;
            this.label_maxNumber.Text = "上述区分号之最大值(&M):";
            // 
            // button_pushTailNumber
            // 
            this.button_pushTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_pushTailNumber.Location = new System.Drawing.Point(317, 310);
            this.button_pushTailNumber.Margin = new System.Windows.Forms.Padding(4);
            this.button_pushTailNumber.Name = "button_pushTailNumber";
            this.button_pushTailNumber.Size = new System.Drawing.Size(535, 38);
            this.button_pushTailNumber.TabIndex = 7;
            this.button_pushTailNumber.Text = "据此最大号推动种次号库尾号(&P)";
            this.button_pushTailNumber.Click += new System.EventHandler(this.button_pushTailNumber_Click);
            // 
            // button_copyMaxNumber
            // 
            this.button_copyMaxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_copyMaxNumber.Location = new System.Drawing.Point(676, 264);
            this.button_copyMaxNumber.Margin = new System.Windows.Forms.Padding(4);
            this.button_copyMaxNumber.Name = "button_copyMaxNumber";
            this.button_copyMaxNumber.Size = new System.Drawing.Size(176, 38);
            this.button_copyMaxNumber.TabIndex = 5;
            this.button_copyMaxNumber.Text = "复制可用号";
            this.button_copyMaxNumber.Click += new System.EventHandler(this.button_copyMaxNumber_Click);
            // 
            // groupBox_tailNumber
            // 
            this.groupBox_tailNumber.Controls.Add(this.button_getTailNumber);
            this.groupBox_tailNumber.Controls.Add(this.textBox_tailNumber);
            this.groupBox_tailNumber.Controls.Add(this.button_saveTailNumber);
            this.groupBox_tailNumber.Controls.Add(this.label_tailNumberTitle);
            this.groupBox_tailNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_tailNumber.Location = new System.Drawing.Point(4, 483);
            this.groupBox_tailNumber.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_tailNumber.Name = "groupBox_tailNumber";
            this.groupBox_tailNumber.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_tailNumber.Size = new System.Drawing.Size(859, 102);
            this.groupBox_tailNumber.TabIndex = 1;
            this.groupBox_tailNumber.TabStop = false;
            this.groupBox_tailNumber.Text = " 种次号库中的尾号 ";
            // 
            // textBox_tailNumber
            // 
            this.textBox_tailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tailNumber.Location = new System.Drawing.Point(128, 38);
            this.textBox_tailNumber.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_tailNumber.Name = "textBox_tailNumber";
            this.textBox_tailNumber.Size = new System.Drawing.Size(412, 31);
            this.textBox_tailNumber.TabIndex = 1;
            // 
            // button_saveTailNumber
            // 
            this.button_saveTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveTailNumber.Location = new System.Drawing.Point(676, 35);
            this.button_saveTailNumber.Margin = new System.Windows.Forms.Padding(4);
            this.button_saveTailNumber.Name = "button_saveTailNumber";
            this.button_saveTailNumber.Size = new System.Drawing.Size(117, 38);
            this.button_saveTailNumber.TabIndex = 3;
            this.button_saveTailNumber.Text = "保存(&S)";
            this.button_saveTailNumber.Click += new System.EventHandler(this.button_saveTailNumber_Click);
            // 
            // label_tailNumberTitle
            // 
            this.label_tailNumberTitle.AutoSize = true;
            this.label_tailNumberTitle.Location = new System.Drawing.Point(4, 46);
            this.label_tailNumberTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_tailNumberTitle.Name = "label_tailNumberTitle";
            this.label_tailNumberTitle.Size = new System.Drawing.Size(96, 21);
            this.label_tailNumberTitle.TabIndex = 0;
            this.label_tailNumberTitle.Text = "尾号(&T):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "类号(&C):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel_query
            // 
            this.tableLayoutPanel_query.AutoSize = true;
            this.tableLayoutPanel_query.ColumnCount = 3;
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_query.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_query.Controls.Add(this.button_searchDouble, 2, 0);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_location, 1, 1);
            this.tableLayoutPanel_query.Controls.Add(this.textBox_classNumber, 1, 0);
            this.tableLayoutPanel_query.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel_query.Location = new System.Drawing.Point(4, 22);
            this.tableLayoutPanel_query.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_query.Name = "tableLayoutPanel_query";
            this.tableLayoutPanel_query.RowCount = 2;
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.Size = new System.Drawing.Size(776, 83);
            this.tableLayoutPanel_query.TabIndex = 17;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.tableLayoutPanel_query, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.groupBox_records, 0, 1);
            this.tableLayoutPanel_main.Controls.Add(this.groupBox_tailNumber, 0, 2);
            this.tableLayoutPanel_main.Controls.Add(this.label_message, 0, 4);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.Padding = new System.Windows.Forms.Padding(0, 18, 0, 18);
            this.tableLayoutPanel_main.RowCount = 4;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(867, 635);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // CallNumberForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(867, 635);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CallNumberForm";
            this.ShowInTaskbar = false;
            this.Text = "同类书区分号";
            this.Activated += new System.EventHandler(this.CallNumberForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CallNumberForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CallNumberForm_FormClosed);
            this.Load += new System.EventHandler(this.CallNumberForm_Load);
            this.groupBox_records.ResumeLayout(false);
            this.groupBox_records.PerformLayout();
            this.groupBox_tailNumber.ResumeLayout(false);
            this.groupBox_tailNumber.PerformLayout();
            this.tableLayoutPanel_query.ResumeLayout(false);
            this.tableLayoutPanel_query.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ComboBox comboBox_location;
        private System.Windows.Forms.Button button_searchClass;
        private DigitalPlatform.GUI.ListViewNF listView_number;
        private System.Windows.Forms.ColumnHeader columnHeader_callNumber;
        private System.Windows.Forms.ColumnHeader columnHeader_summary;
        private System.Windows.Forms.ImageList imageList_lineType;
        private System.Windows.Forms.Button button_searchDouble;
        private System.Windows.Forms.Button button_getTailNumber;
        private System.Windows.Forms.TextBox textBox_classNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.GroupBox groupBox_records;
        private System.Windows.Forms.TextBox textBox_maxNumber;
        private System.Windows.Forms.Label label_maxNumber;
        private System.Windows.Forms.Button button_pushTailNumber;
        private System.Windows.Forms.Button button_copyMaxNumber;
        private System.Windows.Forms.GroupBox groupBox_tailNumber;
        private System.Windows.Forms.TextBox textBox_tailNumber;
        private System.Windows.Forms.Button button_saveTailNumber;
        private System.Windows.Forms.Label label_tailNumberTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_biblioRecPath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_query;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.CheckBox checkBox_topmost;
        private System.Windows.Forms.CheckBox checkBox_returnBrowseCols;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.TextBox textBox_firstEmptyNumber;
        private System.Windows.Forms.Label label2;
    }
}