namespace dp2Circulation
{
    partial class PassGateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PassGateForm));
            this.label1 = new System.Windows.Forms.Label();
            this.button_passGate = new System.Windows.Forms.Button();
            this.textBox_readerBarcode = new System.Windows.Forms.TextBox();
            this.webBrowser_readerInfo = new System.Windows.Forms.WebBrowser();
            this.checkBox_displayReaderDetailInfo = new System.Windows.Forms.CheckBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_list = new System.Windows.Forms.TableLayoutPanel();
            this.panel_listCheck = new System.Windows.Forms.Panel();
            this.checkBox_hideReaderName = new System.Windows.Forms.CheckBox();
            this.checkBox_hideBarcode = new System.Windows.Forms.CheckBox();
            this.listView_list = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_barcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel_detail = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_gateName = new System.Windows.Forms.TextBox();
            this.textBox_counter = new System.Windows.Forms.TextBox();
            this.panel_input = new System.Windows.Forms.Panel();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_list.SuspendLayout();
            this.panel_listCheck.SuspendLayout();
            this.tableLayoutPanel_detail.SuspendLayout();
            this.panel_input.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label1.Location = new System.Drawing.Point(-1, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号(&R):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button_passGate
            // 
            this.button_passGate.AutoSize = true;
            this.button_passGate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_passGate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_passGate.Image = ((System.Drawing.Image)(resources.GetObject("button_passGate.Image")));
            this.button_passGate.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button_passGate.Location = new System.Drawing.Point(239, 1);
            this.button_passGate.Name = "button_passGate";
            this.button_passGate.Size = new System.Drawing.Size(75, 24);
            this.button_passGate.TabIndex = 2;
            this.button_passGate.Text = "登记(&P)";
            this.button_passGate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_passGate.UseVisualStyleBackColor = true;
            this.button_passGate.Click += new System.EventHandler(this.button_passGate_Click);
            // 
            // textBox_readerBarcode
            // 
            this.textBox_readerBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerBarcode.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_readerBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_readerBarcode.Location = new System.Drawing.Point(105, 1);
            this.textBox_readerBarcode.MinimumSize = new System.Drawing.Size(53, 4);
            this.textBox_readerBarcode.Name = "textBox_readerBarcode";
            this.textBox_readerBarcode.Size = new System.Drawing.Size(129, 26);
            this.textBox_readerBarcode.TabIndex = 1;
            this.textBox_readerBarcode.Enter += new System.EventHandler(this.textBox_readerBarcode_Enter);
            this.textBox_readerBarcode.Leave += new System.EventHandler(this.textBox_readerBarcode_Leave);
            // 
            // webBrowser_readerInfo
            // 
            this.webBrowser_readerInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser_readerInfo.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_readerInfo.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_readerInfo.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_readerInfo.Name = "webBrowser_readerInfo";
            this.webBrowser_readerInfo.Size = new System.Drawing.Size(493, 91);
            this.webBrowser_readerInfo.TabIndex = 0;
            this.webBrowser_readerInfo.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_readerInfo_DocumentCompleted);
            // 
            // checkBox_displayReaderDetailInfo
            // 
            this.checkBox_displayReaderDetailInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_displayReaderDetailInfo.AutoSize = true;
            this.checkBox_displayReaderDetailInfo.Location = new System.Drawing.Point(2, 93);
            this.checkBox_displayReaderDetailInfo.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_displayReaderDetailInfo.Name = "checkBox_displayReaderDetailInfo";
            this.checkBox_displayReaderDetailInfo.Size = new System.Drawing.Size(150, 16);
            this.checkBox_displayReaderDetailInfo.TabIndex = 1;
            this.checkBox_displayReaderDetailInfo.Text = "要显示读者详细信息(&D)";
            this.checkBox_displayReaderDetailInfo.UseVisualStyleBackColor = true;
            this.checkBox_displayReaderDetailInfo.CheckedChanged += new System.EventHandler(this.checkBox_displayReaderDetailInfo_CheckedChanged);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 58);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_list);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_detail);
            this.splitContainer_main.Size = new System.Drawing.Size(493, 248);
            this.splitContainer_main.SplitterDistance = 131;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 8;
            // 
            // tableLayoutPanel_list
            // 
            this.tableLayoutPanel_list.ColumnCount = 1;
            this.tableLayoutPanel_list.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_list.Controls.Add(this.panel_listCheck, 0, 1);
            this.tableLayoutPanel_list.Controls.Add(this.listView_list, 0, 0);
            this.tableLayoutPanel_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_list.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_list.Name = "tableLayoutPanel_list";
            this.tableLayoutPanel_list.RowCount = 2;
            this.tableLayoutPanel_list.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_list.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_list.Size = new System.Drawing.Size(493, 131);
            this.tableLayoutPanel_list.TabIndex = 4;
            // 
            // panel_listCheck
            // 
            this.panel_listCheck.Controls.Add(this.checkBox_hideReaderName);
            this.panel_listCheck.Controls.Add(this.checkBox_hideBarcode);
            this.panel_listCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_listCheck.Location = new System.Drawing.Point(3, 109);
            this.panel_listCheck.Name = "panel_listCheck";
            this.panel_listCheck.Size = new System.Drawing.Size(487, 19);
            this.panel_listCheck.TabIndex = 3;
            // 
            // checkBox_hideReaderName
            // 
            this.checkBox_hideReaderName.AutoSize = true;
            this.checkBox_hideReaderName.Location = new System.Drawing.Point(0, 1);
            this.checkBox_hideReaderName.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_hideReaderName.Name = "checkBox_hideReaderName";
            this.checkBox_hideReaderName.Size = new System.Drawing.Size(114, 16);
            this.checkBox_hideReaderName.TabIndex = 1;
            this.checkBox_hideReaderName.Text = "隐藏读者姓名(&N)";
            this.checkBox_hideReaderName.UseVisualStyleBackColor = true;
            this.checkBox_hideReaderName.CheckedChanged += new System.EventHandler(this.checkBox_hideReaderName_CheckedChanged);
            // 
            // checkBox_hideBarcode
            // 
            this.checkBox_hideBarcode.AutoSize = true;
            this.checkBox_hideBarcode.Location = new System.Drawing.Point(118, 1);
            this.checkBox_hideBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_hideBarcode.Name = "checkBox_hideBarcode";
            this.checkBox_hideBarcode.Size = new System.Drawing.Size(102, 16);
            this.checkBox_hideBarcode.TabIndex = 2;
            this.checkBox_hideBarcode.Text = "隐藏条码号(&B)";
            this.checkBox_hideBarcode.UseVisualStyleBackColor = true;
            this.checkBox_hideBarcode.CheckedChanged += new System.EventHandler(this.checkBox_hideBarcode_CheckedChanged);
            // 
            // listView_list
            // 
            this.listView_list.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_barcode,
            this.columnHeader_name,
            this.columnHeader_state,
            this.columnHeader_time});
            this.listView_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_list.LargeImageList = this.imageList_itemType;
            this.listView_list.Location = new System.Drawing.Point(0, 0);
            this.listView_list.Margin = new System.Windows.Forms.Padding(0);
            this.listView_list.Name = "listView_list";
            this.listView_list.Size = new System.Drawing.Size(493, 106);
            this.listView_list.SmallImageList = this.imageList_itemType;
            this.listView_list.TabIndex = 6;
            this.listView_list.UseCompatibleStateImageBehavior = false;
            this.listView_list.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "证条码号";
            this.columnHeader_barcode.Width = 100;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "姓名";
            this.columnHeader_name.Width = 300;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "证状态";
            this.columnHeader_state.Width = 100;
            // 
            // columnHeader_time
            // 
            this.columnHeader_time.Text = "入馆时间";
            this.columnHeader_time.Width = 200;
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_itemType.Images.SetKeyName(0, "Help.png");
            this.imageList_itemType.Images.SetKeyName(1, "DocumentHS.png");
            this.imageList_itemType.Images.SetKeyName(2, "DeleteHS.png");
            // 
            // tableLayoutPanel_detail
            // 
            this.tableLayoutPanel_detail.ColumnCount = 1;
            this.tableLayoutPanel_detail.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_detail.Controls.Add(this.webBrowser_readerInfo, 0, 0);
            this.tableLayoutPanel_detail.Controls.Add(this.checkBox_displayReaderDetailInfo, 0, 1);
            this.tableLayoutPanel_detail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_detail.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_detail.Name = "tableLayoutPanel_detail";
            this.tableLayoutPanel_detail.RowCount = 2;
            this.tableLayoutPanel_detail.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_detail.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_detail.Size = new System.Drawing.Size(493, 111);
            this.tableLayoutPanel_detail.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 30);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "门名称(&G):";
            // 
            // textBox_gateName
            // 
            this.textBox_gateName.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_gateName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.textBox_gateName.Location = new System.Drawing.Point(105, 28);
            this.textBox_gateName.MinimumSize = new System.Drawing.Size(53, 4);
            this.textBox_gateName.Name = "textBox_gateName";
            this.textBox_gateName.Size = new System.Drawing.Size(129, 21);
            this.textBox_gateName.TabIndex = 4;
            // 
            // textBox_counter
            // 
            this.textBox_counter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_counter.Font = new System.Drawing.Font("Arial Black", 32F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_counter.Location = new System.Drawing.Point(319, 0);
            this.textBox_counter.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_counter.Multiline = true;
            this.textBox_counter.Name = "textBox_counter";
            this.textBox_counter.ReadOnly = true;
            this.textBox_counter.Size = new System.Drawing.Size(166, 49);
            this.textBox_counter.TabIndex = 6;
            this.textBox_counter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // panel_input
            // 
            this.panel_input.AutoSize = true;
            this.panel_input.Controls.Add(this.textBox_counter);
            this.panel_input.Controls.Add(this.textBox_readerBarcode);
            this.panel_input.Controls.Add(this.button_passGate);
            this.panel_input.Controls.Add(this.textBox_gateName);
            this.panel_input.Controls.Add(this.label1);
            this.panel_input.Controls.Add(this.label2);
            this.panel_input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_input.Location = new System.Drawing.Point(3, 3);
            this.panel_input.Name = "panel_input";
            this.panel_input.Size = new System.Drawing.Size(487, 52);
            this.panel_input.TabIndex = 0;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.panel_input, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_main, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(493, 306);
            this.tableLayoutPanel_main.TabIndex = 14;
            // 
            // PassGateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 306);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PassGateForm";
            this.ShowInTaskbar = false;
            this.Text = "入馆登记窗";
            this.Activated += new System.EventHandler(this.PassGateForm_Activated);
            this.Deactivate += new System.EventHandler(this.PassGateForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PassGateForm_FormClosed);
            this.Load += new System.EventHandler(this.PassGateForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_list.ResumeLayout(false);
            this.panel_listCheck.ResumeLayout(false);
            this.panel_listCheck.PerformLayout();
            this.tableLayoutPanel_detail.ResumeLayout(false);
            this.tableLayoutPanel_detail.PerformLayout();
            this.panel_input.ResumeLayout(false);
            this.panel_input.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_passGate;
        private System.Windows.Forms.TextBox textBox_readerBarcode;
        private System.Windows.Forms.WebBrowser webBrowser_readerInfo;
        private System.Windows.Forms.CheckBox checkBox_displayReaderDetailInfo;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.CheckBox checkBox_hideReaderName;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.CheckBox checkBox_hideBarcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_gateName;
        private System.Windows.Forms.TextBox textBox_counter;
        private System.Windows.Forms.Panel panel_input;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_detail;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_list;
        private System.Windows.Forms.Panel panel_listCheck;
        private DigitalPlatform.GUI.ListViewNF listView_list;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_time;
    }
}