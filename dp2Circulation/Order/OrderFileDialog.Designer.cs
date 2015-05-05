namespace dp2Circulation
{
    partial class OrderFileDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderFileDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.listView_list = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_seller = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_outputFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_outputFolder = new System.Windows.Forms.TextBox();
            this.button_findOutputFolder = new System.Windows.Forms.Button();
            this.button_projectManager = new System.Windows.Forms.Button();
            this.toolStrip_list = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_newItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modifyItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_deleteItem = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_openOutputFolder = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel_list = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip_list.SuspendLayout();
            this.tableLayoutPanel_list.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "输出格式(&F):";
            // 
            // listView_list
            // 
            this.listView_list.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_seller,
            this.columnHeader_outputFormat});
            this.listView_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_list.FullRowSelect = true;
            this.listView_list.HideSelection = false;
            this.listView_list.Location = new System.Drawing.Point(0, 0);
            this.listView_list.Margin = new System.Windows.Forms.Padding(0);
            this.listView_list.Name = "listView_list";
            this.listView_list.Size = new System.Drawing.Size(377, 139);
            this.listView_list.TabIndex = 1;
            this.listView_list.UseCompatibleStateImageBehavior = false;
            this.listView_list.View = System.Windows.Forms.View.Details;
            this.listView_list.SelectedIndexChanged += new System.EventHandler(this.listView_list_SelectedIndexChanged);
            this.listView_list.DoubleClick += new System.EventHandler(this.listView_list_DoubleClick);
            this.listView_list.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_list_MouseUp);
            // 
            // columnHeader_seller
            // 
            this.columnHeader_seller.Text = "渠道";
            this.columnHeader_seller.Width = 137;
            // 
            // columnHeader_outputFormat
            // 
            this.columnHeader_outputFormat.Text = "输出格式";
            this.columnHeader_outputFormat.Width = 300;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(331, 212);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(241, 212);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(85, 22);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 187);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "输出目录(&D):";
            // 
            // textBox_outputFolder
            // 
            this.textBox_outputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputFolder.Location = new System.Drawing.Point(88, 185);
            this.textBox_outputFolder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_outputFolder.Name = "textBox_outputFolder";
            this.textBox_outputFolder.Size = new System.Drawing.Size(263, 21);
            this.textBox_outputFolder.TabIndex = 3;
            this.textBox_outputFolder.TextChanged += new System.EventHandler(this.textBox_outputFolder_TextChanged);
            // 
            // button_findOutputFolder
            // 
            this.button_findOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findOutputFolder.Location = new System.Drawing.Point(355, 185);
            this.button_findOutputFolder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_findOutputFolder.Name = "button_findOutputFolder";
            this.button_findOutputFolder.Size = new System.Drawing.Size(32, 22);
            this.button_findOutputFolder.TabIndex = 4;
            this.button_findOutputFolder.Text = "...";
            this.button_findOutputFolder.UseVisualStyleBackColor = true;
            this.button_findOutputFolder.Click += new System.EventHandler(this.button_findOutputFolder_Click);
            // 
            // button_projectManager
            // 
            this.button_projectManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_projectManager.Location = new System.Drawing.Point(9, 212);
            this.button_projectManager.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_projectManager.Name = "button_projectManager";
            this.button_projectManager.Size = new System.Drawing.Size(80, 22);
            this.button_projectManager.TabIndex = 5;
            this.button_projectManager.Text = "方案管理...";
            this.button_projectManager.UseVisualStyleBackColor = true;
            this.button_projectManager.Click += new System.EventHandler(this.button_projectManager_Click);
            // 
            // toolStrip_list
            // 
            this.toolStrip_list.AutoSize = false;
            this.toolStrip_list.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_list.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_list.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_newItem,
            this.toolStripButton_modifyItem,
            this.toolStripSeparator1,
            this.toolStripButton_deleteItem,
            this.toolStripButton_openOutputFolder});
            this.toolStrip_list.Location = new System.Drawing.Point(0, 139);
            this.toolStrip_list.Name = "toolStrip_list";
            this.toolStrip_list.Size = new System.Drawing.Size(377, 20);
            this.toolStrip_list.TabIndex = 8;
            this.toolStrip_list.Text = "toolStrip1";
            // 
            // toolStripButton_newItem
            // 
            this.toolStripButton_newItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_newItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newItem.Image")));
            this.toolStripButton_newItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newItem.Name = "toolStripButton_newItem";
            this.toolStripButton_newItem.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_newItem.Text = "新增";
            this.toolStripButton_newItem.Click += new System.EventHandler(this.toolStripButton_newItem_Click);
            // 
            // toolStripButton_modifyItem
            // 
            this.toolStripButton_modifyItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyItem.Enabled = false;
            this.toolStripButton_modifyItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyItem.Image")));
            this.toolStripButton_modifyItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyItem.Name = "toolStripButton_modifyItem";
            this.toolStripButton_modifyItem.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_modifyItem.Text = "修改";
            this.toolStripButton_modifyItem.Click += new System.EventHandler(this.toolStripButton_modifyItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 20);
            // 
            // toolStripButton_deleteItem
            // 
            this.toolStripButton_deleteItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_deleteItem.Enabled = false;
            this.toolStripButton_deleteItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_deleteItem.Image")));
            this.toolStripButton_deleteItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_deleteItem.Name = "toolStripButton_deleteItem";
            this.toolStripButton_deleteItem.Size = new System.Drawing.Size(36, 17);
            this.toolStripButton_deleteItem.Text = "删除";
            this.toolStripButton_deleteItem.Click += new System.EventHandler(this.toolStripButton_deleteItem_Click);
            // 
            // toolStripButton_openOutputFolder
            // 
            this.toolStripButton_openOutputFolder.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_openOutputFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_openOutputFolder.Enabled = false;
            this.toolStripButton_openOutputFolder.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_openOutputFolder.Image")));
            this.toolStripButton_openOutputFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_openOutputFolder.Name = "toolStripButton_openOutputFolder";
            this.toolStripButton_openOutputFolder.Size = new System.Drawing.Size(23, 17);
            this.toolStripButton_openOutputFolder.Text = "打开输出目录文件夹";
            this.toolStripButton_openOutputFolder.Click += new System.EventHandler(this.toolStripButton_openOutputFolder_Click);
            // 
            // tableLayoutPanel_list
            // 
            this.tableLayoutPanel_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_list.ColumnCount = 1;
            this.tableLayoutPanel_list.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_list.Controls.Add(this.listView_list, 0, 0);
            this.tableLayoutPanel_list.Controls.Add(this.toolStrip_list, 0, 1);
            this.tableLayoutPanel_list.Location = new System.Drawing.Point(10, 22);
            this.tableLayoutPanel_list.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_list.Name = "tableLayoutPanel_list";
            this.tableLayoutPanel_list.RowCount = 2;
            this.tableLayoutPanel_list.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_list.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_list.Size = new System.Drawing.Size(377, 159);
            this.tableLayoutPanel_list.TabIndex = 9;
            // 
            // OrderFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(396, 244);
            this.Controls.Add(this.tableLayoutPanel_list);
            this.Controls.Add(this.button_projectManager);
            this.Controls.Add(this.button_findOutputFolder);
            this.Controls.Add(this.textBox_outputFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "OrderFileDialog";
            this.ShowInTaskbar = false;
            this.Text = "订单输出格式";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrderFileDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OrderFileDialog_FormClosed);
            this.Load += new System.EventHandler(this.OrderFileDialog_Load);
            this.toolStrip_list.ResumeLayout(false);
            this.toolStrip_list.PerformLayout();
            this.tableLayoutPanel_list.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DigitalPlatform.GUI.ListViewNF listView_list;
        private System.Windows.Forms.ColumnHeader columnHeader_seller;
        private System.Windows.Forms.ColumnHeader columnHeader_outputFormat;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_outputFolder;
        private System.Windows.Forms.Button button_findOutputFolder;
        private System.Windows.Forms.Button button_projectManager;
        private System.Windows.Forms.ToolStrip toolStrip_list;
        private System.Windows.Forms.ToolStripButton toolStripButton_newItem;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_deleteItem;
        private System.Windows.Forms.ToolStripButton toolStripButton_openOutputFolder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_list;
    }
}