namespace dp2Circulation
{
    partial class PrintAcceptForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintAcceptForm));
            this.tabControl_items = new System.Windows.Forms.TabControl();
            this.tabPage_originItems = new System.Windows.Forms.TabPage();
            this.listView_origin = new DigitalPlatform.GUI.ListViewNF();
            this.tabPage_mergedItems = new System.Windows.Forms.TabPage();
            this.listView_merged = new DigitalPlatform.GUI.ListViewNF();
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_load = new System.Windows.Forms.TabPage();
            this.button_load_loadFromOrderBatchNo = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_load_type = new System.Windows.Forms.ComboBox();
            this.button_load_loadFromBatchNo = new System.Windows.Forms.Button();
            this.button_load_loadFromRecPathFile = new System.Windows.Forms.Button();
            this.tabPage_saveChange = new System.Windows.Forms.TabPage();
            this.textBox_saveChange_info = new System.Windows.Forms.TextBox();
            this.button_saveChange_saveChange = new System.Windows.Forms.Button();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print_exchangeRateOption = new System.Windows.Forms.Button();
            this.button_print_exchangeRateStatis = new System.Windows.Forms.Button();
            this.contextMenuStrip_printExchangeRate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_printExchangeRate_outputExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.button_print_originOption = new System.Windows.Forms.Button();
            this.button_print_printOriginList = new System.Windows.Forms.Button();
            this.button_print_Option = new System.Windows.Forms.Button();
            this.button_print_printAcceptList = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.tabControl_items.SuspendLayout();
            this.tabPage_originItems.SuspendLayout();
            this.tabPage_mergedItems.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_load.SuspendLayout();
            this.tabPage_saveChange.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.contextMenuStrip_printExchangeRate.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_items
            // 
            this.tabControl_items.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_items.Controls.Add(this.tabPage_originItems);
            this.tabControl_items.Controls.Add(this.tabPage_mergedItems);
            this.tabControl_items.Location = new System.Drawing.Point(0, 297);
            this.tabControl_items.Name = "tabControl_items";
            this.tabControl_items.SelectedIndex = 0;
            this.tabControl_items.Size = new System.Drawing.Size(621, 127);
            this.tabControl_items.TabIndex = 8;
            // 
            // tabPage_originItems
            // 
            this.tabPage_originItems.Controls.Add(this.listView_origin);
            this.tabPage_originItems.Location = new System.Drawing.Point(4, 28);
            this.tabPage_originItems.Name = "tabPage_originItems";
            this.tabPage_originItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_originItems.Size = new System.Drawing.Size(613, 95);
            this.tabPage_originItems.TabIndex = 0;
            this.tabPage_originItems.Text = "原始数据";
            this.tabPage_originItems.UseVisualStyleBackColor = true;
            // 
            // listView_origin
            // 
            this.listView_origin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_origin.FullRowSelect = true;
            this.listView_origin.HideSelection = false;
            this.listView_origin.Location = new System.Drawing.Point(3, 3);
            this.listView_origin.Name = "listView_origin";
            this.listView_origin.Size = new System.Drawing.Size(607, 89);
            this.listView_origin.TabIndex = 4;
            this.listView_origin.UseCompatibleStateImageBehavior = false;
            this.listView_origin.View = System.Windows.Forms.View.Details;
            this.listView_origin.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_origin_ColumnClick);
            this.listView_origin.SelectedIndexChanged += new System.EventHandler(this.listView_origin_SelectedIndexChanged);
            this.listView_origin.DoubleClick += new System.EventHandler(this.listView_origin_DoubleClick);
            this.listView_origin.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_origin_MouseUp);
            // 
            // tabPage_mergedItems
            // 
            this.tabPage_mergedItems.Controls.Add(this.listView_merged);
            this.tabPage_mergedItems.Location = new System.Drawing.Point(4, 28);
            this.tabPage_mergedItems.Name = "tabPage_mergedItems";
            this.tabPage_mergedItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_mergedItems.Size = new System.Drawing.Size(613, 95);
            this.tabPage_mergedItems.TabIndex = 1;
            this.tabPage_mergedItems.Text = "合并后";
            this.tabPage_mergedItems.UseVisualStyleBackColor = true;
            // 
            // listView_merged
            // 
            this.listView_merged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_merged.FullRowSelect = true;
            this.listView_merged.HideSelection = false;
            this.listView_merged.Location = new System.Drawing.Point(3, 3);
            this.listView_merged.Name = "listView_merged";
            this.listView_merged.Size = new System.Drawing.Size(607, 89);
            this.listView_merged.TabIndex = 5;
            this.listView_merged.UseCompatibleStateImageBehavior = false;
            this.listView_merged.View = System.Windows.Forms.View.Details;
            this.listView_merged.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_merged_ColumnClick);
            this.listView_merged.SelectedIndexChanged += new System.EventHandler(this.listView_merged_SelectedIndexChanged);
            this.listView_merged.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_merged_MouseUp);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(496, 256);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(124, 33);
            this.button_next.TabIndex = 7;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_load);
            this.tabControl_main.Controls.Add(this.tabPage_saveChange);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(0, 15);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(621, 236);
            this.tabControl_main.TabIndex = 6;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_load
            // 
            this.tabPage_load.Controls.Add(this.button_load_loadFromOrderBatchNo);
            this.tabPage_load.Controls.Add(this.label3);
            this.tabPage_load.Controls.Add(this.comboBox_load_type);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBatchNo);
            this.tabPage_load.Controls.Add(this.button_load_loadFromRecPathFile);
            this.tabPage_load.Location = new System.Drawing.Point(4, 28);
            this.tabPage_load.Name = "tabPage_load";
            this.tabPage_load.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_load.Size = new System.Drawing.Size(613, 204);
            this.tabPage_load.TabIndex = 0;
            this.tabPage_load.Text = "装载";
            this.tabPage_load.UseVisualStyleBackColor = true;
            // 
            // button_load_loadFromOrderBatchNo
            // 
            this.button_load_loadFromOrderBatchNo.Location = new System.Drawing.Point(178, 142);
            this.button_load_loadFromOrderBatchNo.Name = "button_load_loadFromOrderBatchNo";
            this.button_load_loadFromOrderBatchNo.Size = new System.Drawing.Size(315, 33);
            this.button_load_loadFromOrderBatchNo.TabIndex = 4;
            this.button_load_loadFromOrderBatchNo.Text = "根据订购批次号检索装载(&O)...";
            this.button_load_loadFromOrderBatchNo.UseVisualStyleBackColor = true;
            this.button_load_loadFromOrderBatchNo.Click += new System.EventHandler(this.button_load_loadFromOrderBatchNo_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "出版物类型(&T):";
            // 
            // comboBox_load_type
            // 
            this.comboBox_load_type.FormattingEnabled = true;
            this.comboBox_load_type.Items.AddRange(new object[] {
            "图书",
            "连续出版物"});
            this.comboBox_load_type.Location = new System.Drawing.Point(178, 10);
            this.comboBox_load_type.Name = "comboBox_load_type";
            this.comboBox_load_type.Size = new System.Drawing.Size(172, 26);
            this.comboBox_load_type.TabIndex = 1;
            this.comboBox_load_type.Text = "图书";
            this.comboBox_load_type.SelectedIndexChanged += new System.EventHandler(this.comboBox_load_type_SelectedIndexChanged);
            // 
            // button_load_loadFromBatchNo
            // 
            this.button_load_loadFromBatchNo.Location = new System.Drawing.Point(178, 104);
            this.button_load_loadFromBatchNo.Name = "button_load_loadFromBatchNo";
            this.button_load_loadFromBatchNo.Size = new System.Drawing.Size(315, 33);
            this.button_load_loadFromBatchNo.TabIndex = 3;
            this.button_load_loadFromBatchNo.Text = "根据验收批次号检索装载(&B)...";
            this.button_load_loadFromBatchNo.UseVisualStyleBackColor = true;
            this.button_load_loadFromBatchNo.Click += new System.EventHandler(this.button_load_loadFromAcceptBatchNo_Click);
            // 
            // button_load_loadFromRecPathFile
            // 
            this.button_load_loadFromRecPathFile.Location = new System.Drawing.Point(178, 63);
            this.button_load_loadFromRecPathFile.Name = "button_load_loadFromRecPathFile";
            this.button_load_loadFromRecPathFile.Size = new System.Drawing.Size(315, 33);
            this.button_load_loadFromRecPathFile.TabIndex = 2;
            this.button_load_loadFromRecPathFile.Text = "从(册)记录路径文件装载(&F)...";
            this.button_load_loadFromRecPathFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromRecPathFile.Click += new System.EventHandler(this.button_load_loadFromRecPathFile_Click);
            // 
            // tabPage_saveChange
            // 
            this.tabPage_saveChange.Controls.Add(this.textBox_saveChange_info);
            this.tabPage_saveChange.Controls.Add(this.button_saveChange_saveChange);
            this.tabPage_saveChange.Location = new System.Drawing.Point(4, 28);
            this.tabPage_saveChange.Name = "tabPage_saveChange";
            this.tabPage_saveChange.Size = new System.Drawing.Size(613, 204);
            this.tabPage_saveChange.TabIndex = 3;
            this.tabPage_saveChange.Text = "保存修改";
            this.tabPage_saveChange.UseVisualStyleBackColor = true;
            // 
            // textBox_saveChange_info
            // 
            this.textBox_saveChange_info.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_saveChange_info.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_saveChange_info.Location = new System.Drawing.Point(3, 4);
            this.textBox_saveChange_info.Multiline = true;
            this.textBox_saveChange_info.Name = "textBox_saveChange_info";
            this.textBox_saveChange_info.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_saveChange_info.Size = new System.Drawing.Size(577, 142);
            this.textBox_saveChange_info.TabIndex = 3;
            // 
            // button_saveChange_saveChange
            // 
            this.button_saveChange_saveChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveChange_saveChange.Location = new System.Drawing.Point(326, 165);
            this.button_saveChange_saveChange.Name = "button_saveChange_saveChange";
            this.button_saveChange_saveChange.Size = new System.Drawing.Size(255, 33);
            this.button_saveChange_saveChange.TabIndex = 2;
            this.button_saveChange_saveChange.Text = "保存对原始数据的修改(&S)";
            this.button_saveChange_saveChange.UseVisualStyleBackColor = true;
            this.button_saveChange_saveChange.Click += new System.EventHandler(this.button_saveChange_saveChange_Click);
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_print_exchangeRateOption);
            this.tabPage_print.Controls.Add(this.button_print_exchangeRateStatis);
            this.tabPage_print.Controls.Add(this.button_print_originOption);
            this.tabPage_print.Controls.Add(this.button_print_printOriginList);
            this.tabPage_print.Controls.Add(this.button_print_Option);
            this.tabPage_print.Controls.Add(this.button_print_printAcceptList);
            this.tabPage_print.Location = new System.Drawing.Point(4, 28);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(613, 204);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = "打印";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print_exchangeRateOption
            // 
            this.button_print_exchangeRateOption.Location = new System.Drawing.Point(243, 87);
            this.button_print_exchangeRateOption.Name = "button_print_exchangeRateOption";
            this.button_print_exchangeRateOption.Size = new System.Drawing.Size(228, 33);
            this.button_print_exchangeRateOption.TabIndex = 6;
            this.button_print_exchangeRateOption.Text = "汇率表选项(&X)...";
            this.button_print_exchangeRateOption.UseVisualStyleBackColor = true;
            this.button_print_exchangeRateOption.Click += new System.EventHandler(this.button_print_exchangeRateOption_Click);
            // 
            // button_print_exchangeRateStatis
            // 
            this.button_print_exchangeRateStatis.ContextMenuStrip = this.contextMenuStrip_printExchangeRate;
            this.button_print_exchangeRateStatis.Location = new System.Drawing.Point(8, 87);
            this.button_print_exchangeRateStatis.Name = "button_print_exchangeRateStatis";
            this.button_print_exchangeRateStatis.Size = new System.Drawing.Size(228, 33);
            this.button_print_exchangeRateStatis.TabIndex = 5;
            this.button_print_exchangeRateStatis.Text = "汇率统计(&E)...";
            this.button_print_exchangeRateStatis.UseVisualStyleBackColor = true;
            this.button_print_exchangeRateStatis.Click += new System.EventHandler(this.button_print_exchangeRateStatis_Click);
            // 
            // contextMenuStrip_printExchangeRate
            // 
            this.contextMenuStrip_printExchangeRate.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_printExchangeRate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_printExchangeRate_outputExcel});
            this.contextMenuStrip_printExchangeRate.Name = "contextMenuStrip_printOrder";
            this.contextMenuStrip_printExchangeRate.Size = new System.Drawing.Size(206, 32);
            // 
            // toolStripMenuItem_printExchangeRate_outputExcel
            // 
            this.toolStripMenuItem_printExchangeRate_outputExcel.Name = "toolStripMenuItem_printExchangeRate_outputExcel";
            this.toolStripMenuItem_printExchangeRate_outputExcel.Size = new System.Drawing.Size(205, 28);
            this.toolStripMenuItem_printExchangeRate_outputExcel.Text = "输出 Excel 文件";
            this.toolStripMenuItem_printExchangeRate_outputExcel.Click += new System.EventHandler(this.toolStripMenuItem_printExchangeRate_outputExcel_Click);
            // 
            // button_print_originOption
            // 
            this.button_print_originOption.Location = new System.Drawing.Point(243, 48);
            this.button_print_originOption.Name = "button_print_originOption";
            this.button_print_originOption.Size = new System.Drawing.Size(228, 33);
            this.button_print_originOption.TabIndex = 4;
            this.button_print_originOption.Text = "原始数据选项(&T)...";
            this.button_print_originOption.UseVisualStyleBackColor = true;
            this.button_print_originOption.Click += new System.EventHandler(this.button_print_originOption_Click);
            // 
            // button_print_printOriginList
            // 
            this.button_print_printOriginList.Location = new System.Drawing.Point(8, 48);
            this.button_print_printOriginList.Name = "button_print_printOriginList";
            this.button_print_printOriginList.Size = new System.Drawing.Size(228, 33);
            this.button_print_printOriginList.TabIndex = 3;
            this.button_print_printOriginList.Text = "打印原始数据(&R)...";
            this.button_print_printOriginList.UseVisualStyleBackColor = true;
            this.button_print_printOriginList.Click += new System.EventHandler(this.button_print_printOriginList_Click);
            // 
            // button_print_Option
            // 
            this.button_print_Option.Location = new System.Drawing.Point(243, 8);
            this.button_print_Option.Name = "button_print_Option";
            this.button_print_Option.Size = new System.Drawing.Size(228, 33);
            this.button_print_Option.TabIndex = 2;
            this.button_print_Option.Text = "验收单选项(&O)...";
            this.button_print_Option.UseVisualStyleBackColor = true;
            this.button_print_Option.Click += new System.EventHandler(this.button_print_Option_Click);
            // 
            // button_print_printAcceptList
            // 
            this.button_print_printAcceptList.Location = new System.Drawing.Point(8, 8);
            this.button_print_printAcceptList.Name = "button_print_printAcceptList";
            this.button_print_printAcceptList.Size = new System.Drawing.Size(228, 33);
            this.button_print_printAcceptList.TabIndex = 0;
            this.button_print_printAcceptList.Text = "打印验收单(&P)...";
            this.button_print_printAcceptList.UseVisualStyleBackColor = true;
            this.button_print_printAcceptList.Click += new System.EventHandler(this.button_print_printAcceptList_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_message.AutoSize = true;
            this.label_message.Location = new System.Drawing.Point(4, 424);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(0, 18);
            this.label_message.TabIndex = 9;
            // 
            // PrintAcceptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 465);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.tabControl_items);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrintAcceptForm";
            this.ShowInTaskbar = false;
            this.Text = "打印验收单";
            this.Activated += new System.EventHandler(this.PrintAcceptForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintAcceptForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrintAcceptForm_FormClosed);
            this.Load += new System.EventHandler(this.PrintAcceptForm_Load);
            this.tabControl_items.ResumeLayout(false);
            this.tabPage_originItems.ResumeLayout(false);
            this.tabPage_mergedItems.ResumeLayout(false);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_load.ResumeLayout(false);
            this.tabPage_load.PerformLayout();
            this.tabPage_saveChange.ResumeLayout(false);
            this.tabPage_saveChange.PerformLayout();
            this.tabPage_print.ResumeLayout(false);
            this.contextMenuStrip_printExchangeRate.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_items;
        private System.Windows.Forms.TabPage tabPage_originItems;
        private DigitalPlatform.GUI.ListViewNF listView_origin;
        private System.Windows.Forms.TabPage tabPage_mergedItems;
        private DigitalPlatform.GUI.ListViewNF listView_merged;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_load;
        private System.Windows.Forms.Button button_load_loadFromBatchNo;
        private System.Windows.Forms.Button button_load_loadFromRecPathFile;
        private System.Windows.Forms.TabPage tabPage_saveChange;
        private System.Windows.Forms.TextBox textBox_saveChange_info;
        private System.Windows.Forms.Button button_saveChange_saveChange;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_print_originOption;
        private System.Windows.Forms.Button button_print_printOriginList;
        private System.Windows.Forms.Button button_print_Option;
        private System.Windows.Forms.Button button_print_printAcceptList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_load_type;
        private System.Windows.Forms.Button button_load_loadFromOrderBatchNo;
        private System.Windows.Forms.Button button_print_exchangeRateStatis;
        private System.Windows.Forms.Button button_print_exchangeRateOption;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_printExchangeRate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_printExchangeRate_outputExcel;
        private System.Windows.Forms.Label label_message;
    }
}