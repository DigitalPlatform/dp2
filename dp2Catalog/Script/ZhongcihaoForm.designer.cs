namespace dp2Catalog
{
    partial class ZhongcihaoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZhongcihaoForm));
            this.button_pushTailNumber = new System.Windows.Forms.Button();
            this.button_saveTailNumber = new System.Windows.Forms.Button();
            this.textBox_tailNumber = new System.Windows.Forms.TextBox();
            this.textBox_maxNumber = new System.Windows.Forms.TextBox();
            this.textBox_classNumber = new System.Windows.Forms.TextBox();
            this.label_tailNumberTitle = new System.Windows.Forms.Label();
            this.button_copyMaxNumber = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label_message = new System.Windows.Forms.Label();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_searchClass = new System.Windows.Forms.Button();
            this.listView_number = new System.Windows.Forms.ListView();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_number = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_returnBrowseCols = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_getTailNumber = new System.Windows.Forms.Button();
            this.button_searchDouble = new System.Windows.Forms.Button();
            this.comboBox_biblioDbName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serverName = new System.Windows.Forms.TextBox();
            this.button_findServerName = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_pushTailNumber
            // 
            this.button_pushTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.button_pushTailNumber.Location = new System.Drawing.Point(173, 162);
            this.button_pushTailNumber.Margin = new System.Windows.Forms.Padding(2);
            this.button_pushTailNumber.Name = "button_pushTailNumber";
            this.button_pushTailNumber.Size = new System.Drawing.Size(269, 22);
            this.button_pushTailNumber.TabIndex = 5;
            this.button_pushTailNumber.Text = "据此最大号推动种次号库尾号(&P)";
            this.button_pushTailNumber.Click += new System.EventHandler(this.button_pushTailNumber_Click);
            // 
            // button_saveTailNumber
            // 
            this.button_saveTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveTailNumber.Location = new System.Drawing.Point(346, 22);
            this.button_saveTailNumber.Margin = new System.Windows.Forms.Padding(2);
            this.button_saveTailNumber.Name = "button_saveTailNumber";
            this.button_saveTailNumber.Size = new System.Drawing.Size(64, 22);
            this.button_saveTailNumber.TabIndex = 3;
            this.button_saveTailNumber.Text = "保存(&S)";
            this.button_saveTailNumber.Click += new System.EventHandler(this.button_saveTailNumber_Click);
            // 
            // textBox_tailNumber
            // 
            this.textBox_tailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tailNumber.Location = new System.Drawing.Point(70, 22);
            this.textBox_tailNumber.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_tailNumber.Name = "textBox_tailNumber";
            this.textBox_tailNumber.Size = new System.Drawing.Size(204, 21);
            this.textBox_tailNumber.TabIndex = 1;
            // 
            // textBox_maxNumber
            // 
            this.textBox_maxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_maxNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_maxNumber.Location = new System.Drawing.Point(173, 138);
            this.textBox_maxNumber.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_maxNumber.Name = "textBox_maxNumber";
            this.textBox_maxNumber.ReadOnly = true;
            this.textBox_maxNumber.Size = new System.Drawing.Size(170, 21);
            this.textBox_maxNumber.TabIndex = 3;
            // 
            // textBox_classNumber
            // 
            this.textBox_classNumber.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_classNumber.Location = new System.Drawing.Point(118, 37);
            this.textBox_classNumber.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_classNumber.Name = "textBox_classNumber";
            this.textBox_classNumber.Size = new System.Drawing.Size(180, 21);
            this.textBox_classNumber.TabIndex = 4;
            // 
            // label_tailNumberTitle
            // 
            this.label_tailNumberTitle.AutoSize = true;
            this.label_tailNumberTitle.Location = new System.Drawing.Point(2, 26);
            this.label_tailNumberTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_tailNumberTitle.Name = "label_tailNumberTitle";
            this.label_tailNumberTitle.Size = new System.Drawing.Size(53, 12);
            this.label_tailNumberTitle.TabIndex = 0;
            this.label_tailNumberTitle.Text = "尾号(&T):";
            // 
            // button_copyMaxNumber
            // 
            this.button_copyMaxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_copyMaxNumber.Location = new System.Drawing.Point(346, 136);
            this.button_copyMaxNumber.Margin = new System.Windows.Forms.Padding(2);
            this.button_copyMaxNumber.Name = "button_copyMaxNumber";
            this.button_copyMaxNumber.Size = new System.Drawing.Size(96, 22);
            this.button_copyMaxNumber.TabIndex = 4;
            this.button_copyMaxNumber.Text = "复制最大号+1";
            this.button_copyMaxNumber.Click += new System.EventHandler(this.button_copyMaxNumber_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 142);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "上述种次号之最大值(&M):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 64);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "线索书目库名(&B):";
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(7, 347);
            this.label_message.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(449, 23);
            this.label_message.TabIndex = 11;
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(381, 64);
            this.button_stop.Margin = new System.Windows.Forms.Padding(2);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(75, 22);
            this.button_stop.TabIndex = 8;
            this.button_stop.Text = "停止(&S)";
            this.button_stop.Visible = false;
            // 
            // button_searchClass
            // 
            this.button_searchClass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_searchClass.Location = new System.Drawing.Point(346, 10);
            this.button_searchClass.Margin = new System.Windows.Forms.Padding(2);
            this.button_searchClass.Name = "button_searchClass";
            this.button_searchClass.Size = new System.Drawing.Size(96, 22);
            this.button_searchClass.TabIndex = 0;
            this.button_searchClass.Text = "检索书目(&S)";
            this.button_searchClass.Click += new System.EventHandler(this.button_search_Click);
            // 
            // listView_number
            // 
            this.listView_number.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_number.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_number});
            this.listView_number.FullRowSelect = true;
            this.listView_number.HideSelection = false;
            this.listView_number.Location = new System.Drawing.Point(4, 35);
            this.listView_number.Margin = new System.Windows.Forms.Padding(2);
            this.listView_number.Name = "listView_number";
            this.listView_number.Size = new System.Drawing.Size(439, 97);
            this.listView_number.TabIndex = 1;
            this.listView_number.UseCompatibleStateImageBehavior = false;
            this.listView_number.View = System.Windows.Forms.View.Details;
            this.listView_number.SelectedIndexChanged += new System.EventHandler(this.listView_number_SelectedIndexChanged);
            this.listView_number.DoubleClick += new System.EventHandler(this.listView_number_DoubleClick);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "记录路径";
            this.columnHeader_path.Width = 93;
            // 
            // columnHeader_number
            // 
            this.columnHeader_number.Text = "种次号";
            this.columnHeader_number.Width = 115;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 39);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "类号(&C):";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBox_returnBrowseCols);
            this.groupBox2.Controls.Add(this.textBox_maxNumber);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.button_pushTailNumber);
            this.groupBox2.Controls.Add(this.button_searchClass);
            this.groupBox2.Controls.Add(this.listView_number);
            this.groupBox2.Controls.Add(this.button_copyMaxNumber);
            this.groupBox2.Location = new System.Drawing.Point(9, 91);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(447, 190);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " 当前同类书目 ";
            // 
            // checkBox_returnBrowseCols
            // 
            this.checkBox_returnBrowseCols.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_returnBrowseCols.AutoSize = true;
            this.checkBox_returnBrowseCols.Location = new System.Drawing.Point(241, 14);
            this.checkBox_returnBrowseCols.Name = "checkBox_returnBrowseCols";
            this.checkBox_returnBrowseCols.Size = new System.Drawing.Size(102, 16);
            this.checkBox_returnBrowseCols.TabIndex = 6;
            this.checkBox_returnBrowseCols.Text = "返回浏览列(&B)";
            this.checkBox_returnBrowseCols.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_getTailNumber);
            this.groupBox1.Controls.Add(this.textBox_tailNumber);
            this.groupBox1.Controls.Add(this.button_saveTailNumber);
            this.groupBox1.Controls.Add(this.label_tailNumberTitle);
            this.groupBox1.Location = new System.Drawing.Point(9, 286);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(447, 58);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 种次号库中的尾号 ";
            // 
            // button_getTailNumber
            // 
            this.button_getTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getTailNumber.Location = new System.Drawing.Point(278, 22);
            this.button_getTailNumber.Margin = new System.Windows.Forms.Padding(2);
            this.button_getTailNumber.Name = "button_getTailNumber";
            this.button_getTailNumber.Size = new System.Drawing.Size(64, 22);
            this.button_getTailNumber.TabIndex = 2;
            this.button_getTailNumber.Text = "获取(&G)";
            this.button_getTailNumber.UseVisualStyleBackColor = true;
            this.button_getTailNumber.Click += new System.EventHandler(this.button_getTailNumber_Click);
            // 
            // button_searchDouble
            // 
            this.button_searchDouble.Location = new System.Drawing.Point(302, 37);
            this.button_searchDouble.Margin = new System.Windows.Forms.Padding(2);
            this.button_searchDouble.Name = "button_searchDouble";
            this.button_searchDouble.Size = new System.Drawing.Size(154, 22);
            this.button_searchDouble.TabIndex = 5;
            this.button_searchDouble.Text = "检索书目和尾号(&S)";
            this.button_searchDouble.Click += new System.EventHandler(this.button_searchDouble_Click);
            // 
            // comboBox_biblioDbName
            // 
            this.comboBox_biblioDbName.FormattingEnabled = true;
            this.comboBox_biblioDbName.Location = new System.Drawing.Point(118, 62);
            this.comboBox_biblioDbName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_biblioDbName.Name = "comboBox_biblioDbName";
            this.comboBox_biblioDbName.Size = new System.Drawing.Size(180, 20);
            this.comboBox_biblioDbName.TabIndex = 7;
            this.comboBox_biblioDbName.DropDown += new System.EventHandler(this.comboBox_biblioDbName_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 14);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "服务器名(&S):";
            // 
            // textBox_serverName
            // 
            this.textBox_serverName.Location = new System.Drawing.Point(118, 12);
            this.textBox_serverName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_serverName.Name = "textBox_serverName";
            this.textBox_serverName.Size = new System.Drawing.Size(180, 21);
            this.textBox_serverName.TabIndex = 1;
            // 
            // button_findServerName
            // 
            this.button_findServerName.Location = new System.Drawing.Point(302, 10);
            this.button_findServerName.Margin = new System.Windows.Forms.Padding(2);
            this.button_findServerName.Name = "button_findServerName";
            this.button_findServerName.Size = new System.Drawing.Size(40, 22);
            this.button_findServerName.TabIndex = 2;
            this.button_findServerName.Text = "...";
            this.button_findServerName.Click += new System.EventHandler(this.button_findServerName_Click);
            // 
            // ZhongcihaoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 378);
            this.Controls.Add(this.button_findServerName);
            this.Controls.Add(this.textBox_serverName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_biblioDbName);
            this.Controls.Add(this.button_searchDouble);
            this.Controls.Add(this.textBox_classNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ZhongcihaoForm";
            this.Text = "种次号窗";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ZhongcihaoForm_FormClosed);
            this.Load += new System.EventHandler(this.ZhongcihaoForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_pushTailNumber;
        private System.Windows.Forms.Button button_saveTailNumber;
        private System.Windows.Forms.TextBox textBox_tailNumber;
        private System.Windows.Forms.TextBox textBox_maxNumber;
        private System.Windows.Forms.TextBox textBox_classNumber;
        private System.Windows.Forms.Label label_tailNumberTitle;
        private System.Windows.Forms.Button button_copyMaxNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_searchClass;
        public System.Windows.Forms.ListView listView_number;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_number;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_getTailNumber;
        private System.Windows.Forms.Button button_searchDouble;
        private System.Windows.Forms.ComboBox comboBox_biblioDbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_serverName;
        private System.Windows.Forms.Button button_findServerName;
        private System.Windows.Forms.CheckBox checkBox_returnBrowseCols;
    }
}