namespace dp2Circulation
{
    partial class PrintBindingForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintBindingForm));
            this.button_print_option = new System.Windows.Forms.Button();
            this.splitContainer_inAndOutof = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.listView_parent = new DigitalPlatform.GUI.ListViewNF();
            this.imageList_lineType = new System.Windows.Forms.ImageList(this.components);
            this.listView_member = new DigitalPlatform.GUI.ListViewNF();
            this.label2 = new System.Windows.Forms.Label();
            this.button_print_print = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.checkBox_print_barcodeFix = new System.Windows.Forms.CheckBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_load = new System.Windows.Forms.TabPage();
            this.button_load_loadFromRecPathFile = new System.Windows.Forms.Button();
            this.button_load_loadFromBatchNo = new System.Windows.Forms.Button();
            this.button_load_loadFromBarcodeFile = new System.Windows.Forms.Button();
            this.tabPage_sort = new System.Windows.Forms.TabPage();
            this.comboBox_sort_sortStyle = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_member = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_bind = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_inAndOutof)).BeginInit();
            this.splitContainer_inAndOutof.Panel1.SuspendLayout();
            this.splitContainer_inAndOutof.Panel2.SuspendLayout();
            this.splitContainer_inAndOutof.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_load.SuspendLayout();
            this.tabPage_sort.SuspendLayout();
            this.tableLayoutPanel_member.SuspendLayout();
            this.tableLayoutPanel_bind.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_print_option
            // 
            this.button_print_option.Location = new System.Drawing.Point(216, 6);
            this.button_print_option.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_print_option.Name = "button_print_option";
            this.button_print_option.Size = new System.Drawing.Size(203, 28);
            this.button_print_option.TabIndex = 2;
            this.button_print_option.Text = "打印配置(&O)...";
            this.button_print_option.UseVisualStyleBackColor = true;
            this.button_print_option.Click += new System.EventHandler(this.button_print_option_Click);
            // 
            // splitContainer_inAndOutof
            // 
            this.splitContainer_inAndOutof.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_inAndOutof.Location = new System.Drawing.Point(0, 158);
            this.splitContainer_inAndOutof.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer_inAndOutof.Name = "splitContainer_inAndOutof";
            this.splitContainer_inAndOutof.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_inAndOutof.Panel1
            // 
            this.splitContainer_inAndOutof.Panel1.Controls.Add(this.tableLayoutPanel_bind);
            // 
            // splitContainer_inAndOutof.Panel2
            // 
            this.splitContainer_inAndOutof.Panel2.Controls.Add(this.tableLayoutPanel_member);
            this.splitContainer_inAndOutof.Size = new System.Drawing.Size(552, 218);
            this.splitContainer_inAndOutof.SplitterDistance = 103;
            this.splitContainer_inAndOutof.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "合订册";
            // 
            // listView_parent
            // 
            this.listView_parent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_parent.FullRowSelect = true;
            this.listView_parent.HideSelection = false;
            this.listView_parent.LargeImageList = this.imageList_lineType;
            this.listView_parent.Location = new System.Drawing.Point(3, 17);
            this.listView_parent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listView_parent.Name = "listView_parent";
            this.listView_parent.Size = new System.Drawing.Size(546, 84);
            this.listView_parent.SmallImageList = this.imageList_lineType;
            this.listView_parent.TabIndex = 1;
            this.listView_parent.UseCompatibleStateImageBehavior = false;
            this.listView_parent.View = System.Windows.Forms.View.Details;
            this.listView_parent.SelectedIndexChanged += new System.EventHandler(this.listView_parent_SelectedIndexChanged);
            this.listView_parent.DoubleClick += new System.EventHandler(this.listView_parent_DoubleClick);
            this.listView_parent.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_parent_MouseUp);
            // 
            // imageList_lineType
            // 
            this.imageList_lineType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_lineType.ImageStream")));
            this.imageList_lineType.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_lineType.Images.SetKeyName(0, "WarningHS.png");
            this.imageList_lineType.Images.SetKeyName(1, "Book_angleHS.png");
            this.imageList_lineType.Images.SetKeyName(2, "Book_openHS.png");
            // 
            // listView_member
            // 
            this.listView_member.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_member.FullRowSelect = true;
            this.listView_member.HideSelection = false;
            this.listView_member.LargeImageList = this.imageList_lineType;
            this.listView_member.Location = new System.Drawing.Point(3, 17);
            this.listView_member.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listView_member.Name = "listView_member";
            this.listView_member.Size = new System.Drawing.Size(546, 92);
            this.listView_member.SmallImageList = this.imageList_lineType;
            this.listView_member.TabIndex = 1;
            this.listView_member.UseCompatibleStateImageBehavior = false;
            this.listView_member.View = System.Windows.Forms.View.Details;
            this.listView_member.DoubleClick += new System.EventHandler(this.listView_member_DoubleClick);
            this.listView_member.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_member_MouseUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "成员册";
            // 
            // button_print_print
            // 
            this.button_print_print.Location = new System.Drawing.Point(7, 6);
            this.button_print_print.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_print_print.Name = "button_print_print";
            this.button_print_print.Size = new System.Drawing.Size(203, 28);
            this.button_print_print.TabIndex = 0;
            this.button_print_print.Text = "打印装订单(&P)...";
            this.button_print_print.UseVisualStyleBackColor = true;
            this.button_print_print.Click += new System.EventHandler(this.button_print_print_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(441, 122);
            this.button_next.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(111, 28);
            this.button_next.TabIndex = 1;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // tabPage_print
            // 
            this.tabPage_print.AutoScroll = true;
            this.tabPage_print.Controls.Add(this.checkBox_print_barcodeFix);
            this.tabPage_print.Controls.Add(this.button_print_option);
            this.tabPage_print.Controls.Add(this.button_print_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 25);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(544, 73);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = "打印";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // checkBox_print_barcodeFix
            // 
            this.checkBox_print_barcodeFix.AutoSize = true;
            this.checkBox_print_barcodeFix.Location = new System.Drawing.Point(11, 40);
            this.checkBox_print_barcodeFix.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_print_barcodeFix.Name = "checkBox_print_barcodeFix";
            this.checkBox_print_barcodeFix.Size = new System.Drawing.Size(412, 24);
            this.checkBox_print_barcodeFix.TabIndex = 3;
            this.checkBox_print_barcodeFix.Text = "(合订册)条码号打印时加上前后缀字符(&F)";
            this.checkBox_print_barcodeFix.UseVisualStyleBackColor = true;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_load);
            this.tabControl_main.Controls.Add(this.tabPage_sort);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(0, 15);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(552, 102);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_load
            // 
            this.tabPage_load.AutoScroll = true;
            this.tabPage_load.Controls.Add(this.button_load_loadFromRecPathFile);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBatchNo);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBarcodeFile);
            this.tabPage_load.Location = new System.Drawing.Point(4, 25);
            this.tabPage_load.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_load.Name = "tabPage_load";
            this.tabPage_load.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_load.Size = new System.Drawing.Size(544, 73);
            this.tabPage_load.TabIndex = 0;
            this.tabPage_load.Text = "装载";
            this.tabPage_load.UseVisualStyleBackColor = true;
            // 
            // button_load_loadFromRecPathFile
            // 
            this.button_load_loadFromRecPathFile.Location = new System.Drawing.Point(8, 40);
            this.button_load_loadFromRecPathFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_load_loadFromRecPathFile.Name = "button_load_loadFromRecPathFile";
            this.button_load_loadFromRecPathFile.Size = new System.Drawing.Size(227, 28);
            this.button_load_loadFromRecPathFile.TabIndex = 1;
            this.button_load_loadFromRecPathFile.Text = "从记录路径文件装载(&F)...";
            this.button_load_loadFromRecPathFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromRecPathFile.Click += new System.EventHandler(this.button_load_loadFromRecPathFile_Click);
            // 
            // button_load_loadFromBatchNo
            // 
            this.button_load_loadFromBatchNo.Location = new System.Drawing.Point(8, 6);
            this.button_load_loadFromBatchNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_load_loadFromBatchNo.Name = "button_load_loadFromBatchNo";
            this.button_load_loadFromBatchNo.Size = new System.Drawing.Size(227, 28);
            this.button_load_loadFromBatchNo.TabIndex = 0;
            this.button_load_loadFromBatchNo.Text = "根据批次号检索装载(&B)...";
            this.button_load_loadFromBatchNo.UseVisualStyleBackColor = true;
            this.button_load_loadFromBatchNo.Click += new System.EventHandler(this.button_load_loadFromBatchNo_Click);
            // 
            // button_load_loadFromBarcodeFile
            // 
            this.button_load_loadFromBarcodeFile.Location = new System.Drawing.Point(241, 40);
            this.button_load_loadFromBarcodeFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_load_loadFromBarcodeFile.Name = "button_load_loadFromBarcodeFile";
            this.button_load_loadFromBarcodeFile.Size = new System.Drawing.Size(227, 28);
            this.button_load_loadFromBarcodeFile.TabIndex = 2;
            this.button_load_loadFromBarcodeFile.Text = "从条码号文件装载(&F)...";
            this.button_load_loadFromBarcodeFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromBarcodeFile.Click += new System.EventHandler(this.button_load_loadFromBarcodeFile_Click);
            // 
            // tabPage_sort
            // 
            this.tabPage_sort.AutoScroll = true;
            this.tabPage_sort.Controls.Add(this.comboBox_sort_sortStyle);
            this.tabPage_sort.Controls.Add(this.label3);
            this.tabPage_sort.Location = new System.Drawing.Point(4, 25);
            this.tabPage_sort.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage_sort.Name = "tabPage_sort";
            this.tabPage_sort.Size = new System.Drawing.Size(544, 73);
            this.tabPage_sort.TabIndex = 3;
            this.tabPage_sort.Text = "排序";
            this.tabPage_sort.UseVisualStyleBackColor = true;
            // 
            // comboBox_sort_sortStyle
            // 
            this.comboBox_sort_sortStyle.FormattingEnabled = true;
            this.comboBox_sort_sortStyle.Items.AddRange(new object[] {
            "<无>",
            "书目记录路径"});
            this.comboBox_sort_sortStyle.Location = new System.Drawing.Point(125, 12);
            this.comboBox_sort_sortStyle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBox_sort_sortStyle.Name = "comboBox_sort_sortStyle";
            this.comboBox_sort_sortStyle.Size = new System.Drawing.Size(244, 23);
            this.comboBox_sort_sortStyle.TabIndex = 23;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 15);
            this.label3.TabIndex = 22;
            this.label3.Text = "排序策略(&S):";
            // 
            // tableLayoutPanel_member
            // 
            this.tableLayoutPanel_member.ColumnCount = 1;
            this.tableLayoutPanel_member.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_member.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel_member.Controls.Add(this.listView_member, 0, 1);
            this.tableLayoutPanel_member.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_member.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_member.Name = "tableLayoutPanel_member";
            this.tableLayoutPanel_member.RowCount = 2;
            this.tableLayoutPanel_member.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_member.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_member.Size = new System.Drawing.Size(552, 111);
            this.tableLayoutPanel_member.TabIndex = 2;
            // 
            // tableLayoutPanel_bind
            // 
            this.tableLayoutPanel_bind.ColumnCount = 1;
            this.tableLayoutPanel_bind.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_bind.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_bind.Controls.Add(this.listView_parent, 0, 1);
            this.tableLayoutPanel_bind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_bind.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_bind.Name = "tableLayoutPanel_bind";
            this.tableLayoutPanel_bind.RowCount = 2;
            this.tableLayoutPanel_bind.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_bind.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_bind.Size = new System.Drawing.Size(552, 103);
            this.tableLayoutPanel_bind.TabIndex = 2;
            // 
            // PrintBindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 388);
            this.Controls.Add(this.splitContainer_inAndOutof);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PrintBindingForm";
            this.Text = "打印装订单";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintBindingForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrintBindingForm_FormClosed);
            this.Load += new System.EventHandler(this.PrintBindingForm_Load);
            this.splitContainer_inAndOutof.Panel1.ResumeLayout(false);
            this.splitContainer_inAndOutof.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_inAndOutof)).EndInit();
            this.splitContainer_inAndOutof.ResumeLayout(false);
            this.tabPage_print.ResumeLayout(false);
            this.tabPage_print.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_load.ResumeLayout(false);
            this.tabPage_sort.ResumeLayout(false);
            this.tabPage_sort.PerformLayout();
            this.tableLayoutPanel_member.ResumeLayout(false);
            this.tableLayoutPanel_member.PerformLayout();
            this.tableLayoutPanel_bind.ResumeLayout(false);
            this.tableLayoutPanel_bind.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_print_option;
        private System.Windows.Forms.SplitContainer splitContainer_inAndOutof;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.GUI.ListViewNF listView_parent;
        private System.Windows.Forms.ImageList imageList_lineType;
        private DigitalPlatform.GUI.ListViewNF listView_member;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_print_print;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_load;
        private System.Windows.Forms.Button button_load_loadFromBatchNo;
        private System.Windows.Forms.Button button_load_loadFromBarcodeFile;
        private System.Windows.Forms.TabPage tabPage_sort;
        private System.Windows.Forms.Button button_load_loadFromRecPathFile;
        private System.Windows.Forms.ComboBox comboBox_sort_sortStyle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox_print_barcodeFix;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_member;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_bind;
    }
}