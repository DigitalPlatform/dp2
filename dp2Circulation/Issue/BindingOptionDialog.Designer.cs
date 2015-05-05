namespace dp2Circulation
{
    partial class BindingOptionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BindingOptionDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.textBox_general_acceptBatchNo = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_general_bindingBatchNo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_ui = new System.Windows.Forms.TabPage();
            this.checkBox_ui_displayLockedOrderGroup = new System.Windows.Forms.CheckBox();
            this.checkBox_ui_displayOrderInfoXY = new System.Windows.Forms.CheckBox();
            this.comboBox_ui_splitterDirection = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_cellContents = new System.Windows.Forms.TabPage();
            this.button_cellContents_modify = new System.Windows.Forms.Button();
            this.button_cellContents_delete = new System.Windows.Forms.Button();
            this.button_cellContents_new = new System.Windows.Forms.Button();
            this.button_cellContents_moveDown = new System.Windows.Forms.Button();
            this.button_cellContents_moveUp = new System.Windows.Forms.Button();
            this.listView_cellContents_lines = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_caption = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_groupContents = new System.Windows.Forms.TabPage();
            this.button_groupContents_modify = new System.Windows.Forms.Button();
            this.button_groupContents_delete = new System.Windows.Forms.Button();
            this.button_groupContents_new = new System.Windows.Forms.Button();
            this.button_groupContents_moveDown = new System.Windows.Forms.Button();
            this.button_groupContents_moveUp = new System.Windows.Forms.Button();
            this.listView_groupContents_lines = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            this.tabPage_ui.SuspendLayout();
            this.tabPage_cellContents.SuspendLayout();
            this.tabPage_groupContents.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(257, 223);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(196, 223);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_general);
            this.tabControl_main.Controls.Add(this.tabPage_ui);
            this.tabControl_main.Controls.Add(this.tabPage_cellContents);
            this.tabControl_main.Controls.Add(this.tabPage_groupContents);
            this.tabControl_main.Location = new System.Drawing.Point(10, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(304, 208);
            this.tabControl_main.TabIndex = 7;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.textBox_general_acceptBatchNo);
            this.tabPage_general.Controls.Add(this.label3);
            this.tabPage_general.Controls.Add(this.textBox_general_bindingBatchNo);
            this.tabPage_general.Controls.Add(this.label1);
            this.tabPage_general.Location = new System.Drawing.Point(4, 22);
            this.tabPage_general.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_general.Size = new System.Drawing.Size(296, 182);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "一般事项";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // textBox_general_acceptBatchNo
            // 
            this.textBox_general_acceptBatchNo.Location = new System.Drawing.Point(95, 39);
            this.textBox_general_acceptBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_general_acceptBatchNo.Name = "textBox_general_acceptBatchNo";
            this.textBox_general_acceptBatchNo.Size = new System.Drawing.Size(96, 21);
            this.textBox_general_acceptBatchNo.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 42);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "验收批次号(&A):";
            // 
            // textBox_general_bindingBatchNo
            // 
            this.textBox_general_bindingBatchNo.Location = new System.Drawing.Point(95, 14);
            this.textBox_general_bindingBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_general_bindingBatchNo.Name = "textBox_general_bindingBatchNo";
            this.textBox_general_bindingBatchNo.Size = new System.Drawing.Size(96, 21);
            this.textBox_general_bindingBatchNo.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "装订批次号(&B):";
            // 
            // tabPage_ui
            // 
            this.tabPage_ui.Controls.Add(this.checkBox_ui_displayLockedOrderGroup);
            this.tabPage_ui.Controls.Add(this.checkBox_ui_displayOrderInfoXY);
            this.tabPage_ui.Controls.Add(this.comboBox_ui_splitterDirection);
            this.tabPage_ui.Controls.Add(this.label2);
            this.tabPage_ui.Location = new System.Drawing.Point(4, 22);
            this.tabPage_ui.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_ui.Name = "tabPage_ui";
            this.tabPage_ui.Size = new System.Drawing.Size(296, 182);
            this.tabPage_ui.TabIndex = 1;
            this.tabPage_ui.Text = "外观";
            this.tabPage_ui.UseVisualStyleBackColor = true;
            // 
            // checkBox_ui_displayLockedOrderGroup
            // 
            this.checkBox_ui_displayLockedOrderGroup.AutoSize = true;
            this.checkBox_ui_displayLockedOrderGroup.Location = new System.Drawing.Point(5, 79);
            this.checkBox_ui_displayLockedOrderGroup.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_ui_displayLockedOrderGroup.Name = "checkBox_ui_displayLockedOrderGroup";
            this.checkBox_ui_displayLockedOrderGroup.Size = new System.Drawing.Size(270, 16);
            this.checkBox_ui_displayLockedOrderGroup.TabIndex = 3;
            this.checkBox_ui_displayLockedOrderGroup.Text = "显示当前用户管辖分馆范围之外的订购信息(&L)";
            this.checkBox_ui_displayLockedOrderGroup.UseVisualStyleBackColor = true;
            // 
            // checkBox_ui_displayOrderInfoXY
            // 
            this.checkBox_ui_displayOrderInfoXY.AutoSize = true;
            this.checkBox_ui_displayOrderInfoXY.Location = new System.Drawing.Point(5, 47);
            this.checkBox_ui_displayOrderInfoXY.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_ui_displayOrderInfoXY.Name = "checkBox_ui_displayOrderInfoXY";
            this.checkBox_ui_displayOrderInfoXY.Size = new System.Drawing.Size(150, 16);
            this.checkBox_ui_displayOrderInfoXY.TabIndex = 2;
            this.checkBox_ui_displayOrderInfoXY.Text = "显示订购信息坐标值(&O)";
            this.checkBox_ui_displayOrderInfoXY.UseVisualStyleBackColor = true;
            // 
            // comboBox_ui_splitterDirection
            // 
            this.comboBox_ui_splitterDirection.FormattingEnabled = true;
            this.comboBox_ui_splitterDirection.Items.AddRange(new object[] {
            "垂直",
            "水平"});
            this.comboBox_ui_splitterDirection.Location = new System.Drawing.Point(91, 16);
            this.comboBox_ui_splitterDirection.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_ui_splitterDirection.Name = "comboBox_ui_splitterDirection";
            this.comboBox_ui_splitterDirection.Size = new System.Drawing.Size(92, 20);
            this.comboBox_ui_splitterDirection.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "布局方式(&L):";
            // 
            // tabPage_cellContents
            // 
            this.tabPage_cellContents.Controls.Add(this.button_cellContents_modify);
            this.tabPage_cellContents.Controls.Add(this.button_cellContents_delete);
            this.tabPage_cellContents.Controls.Add(this.button_cellContents_new);
            this.tabPage_cellContents.Controls.Add(this.button_cellContents_moveDown);
            this.tabPage_cellContents.Controls.Add(this.button_cellContents_moveUp);
            this.tabPage_cellContents.Controls.Add(this.listView_cellContents_lines);
            this.tabPage_cellContents.Controls.Add(this.label4);
            this.tabPage_cellContents.Location = new System.Drawing.Point(4, 22);
            this.tabPage_cellContents.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_cellContents.Name = "tabPage_cellContents";
            this.tabPage_cellContents.Size = new System.Drawing.Size(296, 182);
            this.tabPage_cellContents.TabIndex = 2;
            this.tabPage_cellContents.Text = "册格子内容";
            this.tabPage_cellContents.UseVisualStyleBackColor = true;
            // 
            // button_cellContents_modify
            // 
            this.button_cellContents_modify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cellContents_modify.Enabled = false;
            this.button_cellContents_modify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cellContents_modify.Location = new System.Drawing.Point(239, 126);
            this.button_cellContents_modify.Margin = new System.Windows.Forms.Padding(2);
            this.button_cellContents_modify.Name = "button_cellContents_modify";
            this.button_cellContents_modify.Size = new System.Drawing.Size(56, 22);
            this.button_cellContents_modify.TabIndex = 9;
            this.button_cellContents_modify.Text = "修改(&M)";
            this.button_cellContents_modify.UseVisualStyleBackColor = true;
            this.button_cellContents_modify.Click += new System.EventHandler(this.button_cellContents_modify_Click);
            // 
            // button_cellContents_delete
            // 
            this.button_cellContents_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cellContents_delete.Enabled = false;
            this.button_cellContents_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cellContents_delete.Location = new System.Drawing.Point(239, 153);
            this.button_cellContents_delete.Margin = new System.Windows.Forms.Padding(2);
            this.button_cellContents_delete.Name = "button_cellContents_delete";
            this.button_cellContents_delete.Size = new System.Drawing.Size(56, 22);
            this.button_cellContents_delete.TabIndex = 10;
            this.button_cellContents_delete.Text = "删除(&R)";
            this.button_cellContents_delete.UseVisualStyleBackColor = true;
            this.button_cellContents_delete.Click += new System.EventHandler(this.button_cellContents_delete_Click);
            // 
            // button_cellContents_new
            // 
            this.button_cellContents_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cellContents_new.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cellContents_new.Location = new System.Drawing.Point(239, 98);
            this.button_cellContents_new.Margin = new System.Windows.Forms.Padding(2);
            this.button_cellContents_new.Name = "button_cellContents_new";
            this.button_cellContents_new.Size = new System.Drawing.Size(56, 22);
            this.button_cellContents_new.TabIndex = 8;
            this.button_cellContents_new.Text = "新增(&N)";
            this.button_cellContents_new.UseVisualStyleBackColor = true;
            this.button_cellContents_new.Click += new System.EventHandler(this.button_cellContents_new_Click);
            // 
            // button_cellContents_moveDown
            // 
            this.button_cellContents_moveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cellContents_moveDown.Enabled = false;
            this.button_cellContents_moveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cellContents_moveDown.Location = new System.Drawing.Point(239, 54);
            this.button_cellContents_moveDown.Margin = new System.Windows.Forms.Padding(2);
            this.button_cellContents_moveDown.Name = "button_cellContents_moveDown";
            this.button_cellContents_moveDown.Size = new System.Drawing.Size(56, 22);
            this.button_cellContents_moveDown.TabIndex = 7;
            this.button_cellContents_moveDown.Text = "下移(&D)";
            this.button_cellContents_moveDown.UseVisualStyleBackColor = true;
            this.button_cellContents_moveDown.Click += new System.EventHandler(this.button_cellContents_moveDown_Click);
            // 
            // button_cellContents_moveUp
            // 
            this.button_cellContents_moveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cellContents_moveUp.Enabled = false;
            this.button_cellContents_moveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cellContents_moveUp.Location = new System.Drawing.Point(239, 26);
            this.button_cellContents_moveUp.Margin = new System.Windows.Forms.Padding(2);
            this.button_cellContents_moveUp.Name = "button_cellContents_moveUp";
            this.button_cellContents_moveUp.Size = new System.Drawing.Size(56, 22);
            this.button_cellContents_moveUp.TabIndex = 6;
            this.button_cellContents_moveUp.Text = "上移(&U)";
            this.button_cellContents_moveUp.UseVisualStyleBackColor = true;
            this.button_cellContents_moveUp.Click += new System.EventHandler(this.button_cellContents_moveUp_Click);
            // 
            // listView_cellContents_lines
            // 
            this.listView_cellContents_lines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_cellContents_lines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_caption});
            this.listView_cellContents_lines.FullRowSelect = true;
            this.listView_cellContents_lines.HideSelection = false;
            this.listView_cellContents_lines.Location = new System.Drawing.Point(3, 26);
            this.listView_cellContents_lines.Margin = new System.Windows.Forms.Padding(2);
            this.listView_cellContents_lines.Name = "listView_cellContents_lines";
            this.listView_cellContents_lines.Size = new System.Drawing.Size(233, 158);
            this.listView_cellContents_lines.TabIndex = 1;
            this.listView_cellContents_lines.UseCompatibleStateImageBehavior = false;
            this.listView_cellContents_lines.View = System.Windows.Forms.View.Details;
            this.listView_cellContents_lines.SelectedIndexChanged += new System.EventHandler(this.listView_cellContents_lines_SelectedIndexChanged);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "名称";
            this.columnHeader_name.Width = 100;
            // 
            // columnHeader_caption
            // 
            this.columnHeader_caption.Text = "标题";
            this.columnHeader_caption.Width = 150;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 11);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "所显示的内容行(&L):";
            // 
            // tabPage_groupContents
            // 
            this.tabPage_groupContents.Controls.Add(this.button_groupContents_modify);
            this.tabPage_groupContents.Controls.Add(this.button_groupContents_delete);
            this.tabPage_groupContents.Controls.Add(this.button_groupContents_new);
            this.tabPage_groupContents.Controls.Add(this.button_groupContents_moveDown);
            this.tabPage_groupContents.Controls.Add(this.button_groupContents_moveUp);
            this.tabPage_groupContents.Controls.Add(this.listView_groupContents_lines);
            this.tabPage_groupContents.Controls.Add(this.label5);
            this.tabPage_groupContents.Location = new System.Drawing.Point(4, 22);
            this.tabPage_groupContents.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_groupContents.Name = "tabPage_groupContents";
            this.tabPage_groupContents.Size = new System.Drawing.Size(296, 182);
            this.tabPage_groupContents.TabIndex = 3;
            this.tabPage_groupContents.Text = "组格子内容";
            this.tabPage_groupContents.UseVisualStyleBackColor = true;
            // 
            // button_groupContents_modify
            // 
            this.button_groupContents_modify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_groupContents_modify.Enabled = false;
            this.button_groupContents_modify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_groupContents_modify.Location = new System.Drawing.Point(239, 126);
            this.button_groupContents_modify.Margin = new System.Windows.Forms.Padding(2);
            this.button_groupContents_modify.Name = "button_groupContents_modify";
            this.button_groupContents_modify.Size = new System.Drawing.Size(56, 22);
            this.button_groupContents_modify.TabIndex = 16;
            this.button_groupContents_modify.Text = "修改(&M)";
            this.button_groupContents_modify.UseVisualStyleBackColor = true;
            this.button_groupContents_modify.Click += new System.EventHandler(this.button_groupContents_modify_Click);
            // 
            // button_groupContents_delete
            // 
            this.button_groupContents_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_groupContents_delete.Enabled = false;
            this.button_groupContents_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_groupContents_delete.Location = new System.Drawing.Point(239, 153);
            this.button_groupContents_delete.Margin = new System.Windows.Forms.Padding(2);
            this.button_groupContents_delete.Name = "button_groupContents_delete";
            this.button_groupContents_delete.Size = new System.Drawing.Size(56, 22);
            this.button_groupContents_delete.TabIndex = 17;
            this.button_groupContents_delete.Text = "删除(&R)";
            this.button_groupContents_delete.UseVisualStyleBackColor = true;
            this.button_groupContents_delete.Click += new System.EventHandler(this.button_groupContents_delete_Click);
            // 
            // button_groupContents_new
            // 
            this.button_groupContents_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_groupContents_new.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_groupContents_new.Location = new System.Drawing.Point(239, 98);
            this.button_groupContents_new.Margin = new System.Windows.Forms.Padding(2);
            this.button_groupContents_new.Name = "button_groupContents_new";
            this.button_groupContents_new.Size = new System.Drawing.Size(56, 22);
            this.button_groupContents_new.TabIndex = 15;
            this.button_groupContents_new.Text = "新增(&N)";
            this.button_groupContents_new.UseVisualStyleBackColor = true;
            this.button_groupContents_new.Click += new System.EventHandler(this.button_groupContents_new_Click);
            // 
            // button_groupContents_moveDown
            // 
            this.button_groupContents_moveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_groupContents_moveDown.Enabled = false;
            this.button_groupContents_moveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_groupContents_moveDown.Location = new System.Drawing.Point(239, 54);
            this.button_groupContents_moveDown.Margin = new System.Windows.Forms.Padding(2);
            this.button_groupContents_moveDown.Name = "button_groupContents_moveDown";
            this.button_groupContents_moveDown.Size = new System.Drawing.Size(56, 22);
            this.button_groupContents_moveDown.TabIndex = 14;
            this.button_groupContents_moveDown.Text = "下移(&D)";
            this.button_groupContents_moveDown.UseVisualStyleBackColor = true;
            this.button_groupContents_moveDown.Click += new System.EventHandler(this.button_groupContents_moveDown_Click);
            // 
            // button_groupContents_moveUp
            // 
            this.button_groupContents_moveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_groupContents_moveUp.Enabled = false;
            this.button_groupContents_moveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_groupContents_moveUp.Location = new System.Drawing.Point(239, 26);
            this.button_groupContents_moveUp.Margin = new System.Windows.Forms.Padding(2);
            this.button_groupContents_moveUp.Name = "button_groupContents_moveUp";
            this.button_groupContents_moveUp.Size = new System.Drawing.Size(56, 22);
            this.button_groupContents_moveUp.TabIndex = 13;
            this.button_groupContents_moveUp.Text = "上移(&U)";
            this.button_groupContents_moveUp.UseVisualStyleBackColor = true;
            this.button_groupContents_moveUp.Click += new System.EventHandler(this.button_groupContents_moveUp_Click);
            // 
            // listView_groupContents_lines
            // 
            this.listView_groupContents_lines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_groupContents_lines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView_groupContents_lines.FullRowSelect = true;
            this.listView_groupContents_lines.HideSelection = false;
            this.listView_groupContents_lines.Location = new System.Drawing.Point(3, 26);
            this.listView_groupContents_lines.Margin = new System.Windows.Forms.Padding(2);
            this.listView_groupContents_lines.Name = "listView_groupContents_lines";
            this.listView_groupContents_lines.Size = new System.Drawing.Size(233, 158);
            this.listView_groupContents_lines.TabIndex = 12;
            this.listView_groupContents_lines.UseCompatibleStateImageBehavior = false;
            this.listView_groupContents_lines.View = System.Windows.Forms.View.Details;
            this.listView_groupContents_lines.SelectedIndexChanged += new System.EventHandler(this.listView_groupContents_lines_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "名称";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "标题";
            this.columnHeader2.Width = 150;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(0, 11);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "所显示的内容行(&L):";
            // 
            // BindingOptionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(322, 255);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BindingOptionDialog";
            this.ShowInTaskbar = false;
            this.Text = "装订选项";
            this.Load += new System.EventHandler(this.BindingOptionDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.tabPage_general.PerformLayout();
            this.tabPage_ui.ResumeLayout(false);
            this.tabPage_ui.PerformLayout();
            this.tabPage_cellContents.ResumeLayout(false);
            this.tabPage_cellContents.PerformLayout();
            this.tabPage_groupContents.ResumeLayout(false);
            this.tabPage_groupContents.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.TextBox textBox_general_bindingBatchNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_ui;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_ui_splitterDirection;
        private System.Windows.Forms.TextBox textBox_general_acceptBatchNo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox_ui_displayOrderInfoXY;
        private System.Windows.Forms.TabPage tabPage_cellContents;
        private System.Windows.Forms.Label label4;
        private DigitalPlatform.GUI.ListViewNF listView_cellContents_lines;
        private System.Windows.Forms.Button button_cellContents_modify;
        private System.Windows.Forms.Button button_cellContents_delete;
        private System.Windows.Forms.Button button_cellContents_new;
        private System.Windows.Forms.Button button_cellContents_moveDown;
        private System.Windows.Forms.Button button_cellContents_moveUp;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_caption;
        private System.Windows.Forms.TabPage tabPage_groupContents;
        private System.Windows.Forms.Button button_groupContents_modify;
        private System.Windows.Forms.Button button_groupContents_delete;
        private System.Windows.Forms.Button button_groupContents_new;
        private System.Windows.Forms.Button button_groupContents_moveDown;
        private System.Windows.Forms.Button button_groupContents_moveUp;
        private DigitalPlatform.GUI.ListViewNF listView_groupContents_lines;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox_ui_displayLockedOrderGroup;
    }
}