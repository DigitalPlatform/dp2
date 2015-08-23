namespace dp2Circulation
{
    partial class InventoryForm
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.tabComboBox_inputBatchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_scan = new System.Windows.Forms.TabPage();
            this.tabPage_inventoryList = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_list = new System.Windows.Forms.TableLayoutPanel();
            this.listView_inventoryList_records = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel_list_searchPanel = new System.Windows.Forms.Panel();
            this.button_inventoryList_getBatchNos = new System.Windows.Forms.Button();
            this.button_inventoryList_search = new System.Windows.Forms.Button();
            this.textBox_inventoryList_batchNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_baseList = new System.Windows.Forms.TabPage();
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            this.tabPage_inventoryList.SuspendLayout();
            this.tableLayoutPanel_list.SuspendLayout();
            this.panel_list_searchPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_start);
            this.tabControl_main.Controls.Add(this.tabPage_scan);
            this.tabControl_main.Controls.Add(this.tabPage_inventoryList);
            this.tabControl_main.Controls.Add(this.tabPage_baseList);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(430, 278);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_start
            // 
            this.tabPage_start.Controls.Add(this.tabComboBox_inputBatchNo);
            this.tabPage_start.Controls.Add(this.label1);
            this.tabPage_start.Location = new System.Drawing.Point(4, 22);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_start.Size = new System.Drawing.Size(422, 252);
            this.tabPage_start.TabIndex = 0;
            this.tabPage_start.Text = "开始";
            this.tabPage_start.UseVisualStyleBackColor = true;
            // 
            // tabComboBox_inputBatchNo
            // 
            this.tabComboBox_inputBatchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_inputBatchNo.FormattingEnabled = true;
            this.tabComboBox_inputBatchNo.LeftFontStyle = System.Drawing.FontStyle.Bold;
            this.tabComboBox_inputBatchNo.Location = new System.Drawing.Point(94, 16);
            this.tabComboBox_inputBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.tabComboBox_inputBatchNo.Name = "tabComboBox_inputBatchNo";
            this.tabComboBox_inputBatchNo.RightFontStyle = System.Drawing.FontStyle.Italic;
            this.tabComboBox_inputBatchNo.Size = new System.Drawing.Size(140, 22);
            this.tabComboBox_inputBatchNo.TabIndex = 6;
            this.tabComboBox_inputBatchNo.TextChanged += new System.EventHandler(this.tabComboBox_inputBatchNo_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "批次号(&B):";
            // 
            // tabPage_scan
            // 
            this.tabPage_scan.Location = new System.Drawing.Point(4, 22);
            this.tabPage_scan.Name = "tabPage_scan";
            this.tabPage_scan.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_scan.Size = new System.Drawing.Size(422, 252);
            this.tabPage_scan.TabIndex = 1;
            this.tabPage_scan.Text = "扫入";
            this.tabPage_scan.UseVisualStyleBackColor = true;
            // 
            // tabPage_inventoryList
            // 
            this.tabPage_inventoryList.Controls.Add(this.tableLayoutPanel_list);
            this.tabPage_inventoryList.Location = new System.Drawing.Point(4, 22);
            this.tabPage_inventoryList.Name = "tabPage_inventoryList";
            this.tabPage_inventoryList.Size = new System.Drawing.Size(422, 252);
            this.tabPage_inventoryList.TabIndex = 2;
            this.tabPage_inventoryList.Text = "盘点集";
            this.tabPage_inventoryList.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_list
            // 
            this.tableLayoutPanel_list.ColumnCount = 1;
            this.tableLayoutPanel_list.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_list.Controls.Add(this.listView_inventoryList_records, 0, 1);
            this.tableLayoutPanel_list.Controls.Add(this.panel_list_searchPanel, 0, 0);
            this.tableLayoutPanel_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_list.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_list.Name = "tableLayoutPanel_list";
            this.tableLayoutPanel_list.RowCount = 2;
            this.tableLayoutPanel_list.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_list.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_list.Size = new System.Drawing.Size(422, 252);
            this.tableLayoutPanel_list.TabIndex = 12;
            // 
            // listView_inventoryList_records
            // 
            this.listView_inventoryList_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1});
            this.listView_inventoryList_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_inventoryList_records.FullRowSelect = true;
            this.listView_inventoryList_records.HideSelection = false;
            this.listView_inventoryList_records.Location = new System.Drawing.Point(0, 73);
            this.listView_inventoryList_records.Margin = new System.Windows.Forms.Padding(0);
            this.listView_inventoryList_records.Name = "listView_inventoryList_records";
            this.listView_inventoryList_records.Size = new System.Drawing.Size(422, 179);
            this.listView_inventoryList_records.TabIndex = 11;
            this.listView_inventoryList_records.UseCompatibleStateImageBehavior = false;
            this.listView_inventoryList_records.View = System.Windows.Forms.View.Details;
            this.listView_inventoryList_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_records_ColumnClick);
            this.listView_inventoryList_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 150;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 200;
            // 
            // panel_list_searchPanel
            // 
            this.panel_list_searchPanel.Controls.Add(this.button_inventoryList_getBatchNos);
            this.panel_list_searchPanel.Controls.Add(this.button_inventoryList_search);
            this.panel_list_searchPanel.Controls.Add(this.textBox_inventoryList_batchNo);
            this.panel_list_searchPanel.Controls.Add(this.label2);
            this.panel_list_searchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_list_searchPanel.Location = new System.Drawing.Point(0, 0);
            this.panel_list_searchPanel.Margin = new System.Windows.Forms.Padding(0);
            this.panel_list_searchPanel.Name = "panel_list_searchPanel";
            this.panel_list_searchPanel.Size = new System.Drawing.Size(422, 73);
            this.panel_list_searchPanel.TabIndex = 12;
            // 
            // button_inventoryList_getBatchNos
            // 
            this.button_inventoryList_getBatchNos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_inventoryList_getBatchNos.Location = new System.Drawing.Point(347, 47);
            this.button_inventoryList_getBatchNos.Name = "button_inventoryList_getBatchNos";
            this.button_inventoryList_getBatchNos.Size = new System.Drawing.Size(75, 23);
            this.button_inventoryList_getBatchNos.TabIndex = 3;
            this.button_inventoryList_getBatchNos.Text = "选取批次号";
            this.button_inventoryList_getBatchNos.UseVisualStyleBackColor = true;
            this.button_inventoryList_getBatchNos.Click += new System.EventHandler(this.button_list_getBatchNos_Click);
            // 
            // button_inventoryList_search
            // 
            this.button_inventoryList_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_inventoryList_search.Location = new System.Drawing.Point(347, 19);
            this.button_inventoryList_search.Name = "button_inventoryList_search";
            this.button_inventoryList_search.Size = new System.Drawing.Size(75, 23);
            this.button_inventoryList_search.TabIndex = 2;
            this.button_inventoryList_search.Text = "检索";
            this.button_inventoryList_search.UseVisualStyleBackColor = true;
            this.button_inventoryList_search.Click += new System.EventHandler(this.button_list_search_Click);
            // 
            // textBox_inventoryList_batchNo
            // 
            this.textBox_inventoryList_batchNo.AcceptsReturn = true;
            this.textBox_inventoryList_batchNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_inventoryList_batchNo.Location = new System.Drawing.Point(0, 19);
            this.textBox_inventoryList_batchNo.Multiline = true;
            this.textBox_inventoryList_batchNo.Name = "textBox_inventoryList_batchNo";
            this.textBox_inventoryList_batchNo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_inventoryList_batchNo.Size = new System.Drawing.Size(346, 54);
            this.textBox_inventoryList_batchNo.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-2, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "批次号:";
            // 
            // tabPage_baseList
            // 
            this.tabPage_baseList.Location = new System.Drawing.Point(4, 22);
            this.tabPage_baseList.Name = "tabPage_baseList";
            this.tabPage_baseList.Size = new System.Drawing.Size(422, 252);
            this.tabPage_baseList.TabIndex = 3;
            this.tabPage_baseList.Text = "基准集";
            this.tabPage_baseList.UseVisualStyleBackColor = true;
            // 
            // InventoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 278);
            this.Controls.Add(this.tabControl_main);
            this.Name = "InventoryForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "盘点";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InventoryForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InventoryForm_FormClosed);
            this.Load += new System.EventHandler(this.InventoryForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.tabPage_start.PerformLayout();
            this.tabPage_inventoryList.ResumeLayout(false);
            this.tableLayoutPanel_list.ResumeLayout(false);
            this.panel_list_searchPanel.ResumeLayout(false);
            this.panel_list_searchPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.TabPage tabPage_scan;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_inputBatchNo;
        private System.Windows.Forms.TabPage tabPage_inventoryList;
        private DigitalPlatform.GUI.ListViewNF listView_inventoryList_records;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_list;
        private System.Windows.Forms.Panel panel_list_searchPanel;
        private System.Windows.Forms.Button button_inventoryList_getBatchNos;
        private System.Windows.Forms.Button button_inventoryList_search;
        private System.Windows.Forms.TextBox textBox_inventoryList_batchNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage_baseList;
    }
}