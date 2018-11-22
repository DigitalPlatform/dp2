namespace dp2Circulation
{
    partial class DupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DupForm));
            this.label_dupMessage = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_recordPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listView_browse = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_sum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_dupItemType = new System.Windows.Forms.ImageList(this.components);
            this.label_message = new System.Windows.Forms.Label();
            this.button_search = new System.Windows.Forms.Button();
            this.comboBox_projectName = new System.Windows.Forms.ComboBox();
            this.button_viewXmlRecord = new System.Windows.Forms.Button();
            this.checkBox_includeLowCols = new System.Windows.Forms.CheckBox();
            this.checkBox_returnAllRecords = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBox_returnSearchDetail = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_dupMessage
            // 
            this.label_dupMessage.AutoSize = true;
            this.label_dupMessage.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_dupMessage.Location = new System.Drawing.Point(3, 425);
            this.label_dupMessage.Name = "label_dupMessage";
            this.label_dupMessage.Size = new System.Drawing.Size(114, 18);
            this.label_dupMessage.TabIndex = 1;
            this.label_dupMessage.Text = "尚未查重...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "查重方案(&P):";
            // 
            // textBox_recordPath
            // 
            this.textBox_recordPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recordPath.Location = new System.Drawing.Point(142, -2);
            this.textBox_recordPath.Name = "textBox_recordPath";
            this.textBox_recordPath.Size = new System.Drawing.Size(268, 28);
            this.textBox_recordPath.TabIndex = 1;
            this.textBox_recordPath.TextChanged += new System.EventHandler(this.textBox_recordPath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "源记录路径(&P):";
            // 
            // listView_browse
            // 
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_sum});
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Left;
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.LargeImageList = this.imageList_dupItemType;
            this.listView_browse.Location = new System.Drawing.Point(0, 72);
            this.listView_browse.Margin = new System.Windows.Forms.Padding(0);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(451, 303);
            this.listView_browse.SmallImageList = this.imageList_dupItemType;
            this.listView_browse.TabIndex = 0;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_browse_ColumnClick);
            this.listView_browse.SelectedIndexChanged += new System.EventHandler(this.listView_browse_SelectedIndexChanged);
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_browse_MouseUp);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "记录路径";
            this.columnHeader_path.Width = 120;
            // 
            // columnHeader_sum
            // 
            this.columnHeader_sum.Text = "权值和";
            this.columnHeader_sum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_sum.Width = 70;
            // 
            // imageList_dupItemType
            // 
            this.imageList_dupItemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_dupItemType.ImageStream")));
            this.imageList_dupItemType.TransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.imageList_dupItemType.Images.SetKeyName(0, "undup_type.bmp");
            this.imageList_dupItemType.Images.SetKeyName(1, "dup_type.bmp");
            // 
            // label_message
            // 
            this.label_message.AutoSize = true;
            this.label_message.Location = new System.Drawing.Point(3, 443);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(17, 18);
            this.label_message.TabIndex = 2;
            this.label_message.Text = " ";
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(418, 0);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(112, 33);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "查重(&S)";
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // comboBox_projectName
            // 
            this.comboBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_projectName.FormattingEnabled = true;
            this.comboBox_projectName.Location = new System.Drawing.Point(142, 3);
            this.comboBox_projectName.Name = "comboBox_projectName";
            this.comboBox_projectName.Size = new System.Drawing.Size(268, 26);
            this.comboBox_projectName.TabIndex = 1;
            this.comboBox_projectName.DropDown += new System.EventHandler(this.comboBox_projectName_DropDown);
            // 
            // button_viewXmlRecord
            // 
            this.button_viewXmlRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_viewXmlRecord.Location = new System.Drawing.Point(418, 0);
            this.button_viewXmlRecord.Name = "button_viewXmlRecord";
            this.button_viewXmlRecord.Size = new System.Drawing.Size(112, 33);
            this.button_viewXmlRecord.TabIndex = 2;
            this.button_viewXmlRecord.Text = "XML...";
            this.button_viewXmlRecord.UseVisualStyleBackColor = true;
            this.button_viewXmlRecord.Click += new System.EventHandler(this.button_viewXmlRecord_Click);
            // 
            // checkBox_includeLowCols
            // 
            this.checkBox_includeLowCols.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_includeLowCols.AutoSize = true;
            this.checkBox_includeLowCols.Location = new System.Drawing.Point(4, 4);
            this.checkBox_includeLowCols.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_includeLowCols.Name = "checkBox_includeLowCols";
            this.checkBox_includeLowCols.Size = new System.Drawing.Size(295, 22);
            this.checkBox_includeLowCols.TabIndex = 0;
            this.checkBox_includeLowCols.Text = "返回低于阈值的记录的浏览列(&B)";
            this.checkBox_includeLowCols.UseVisualStyleBackColor = true;
            // 
            // checkBox_returnAllRecords
            // 
            this.checkBox_returnAllRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_returnAllRecords.AutoSize = true;
            this.checkBox_returnAllRecords.Location = new System.Drawing.Point(307, 4);
            this.checkBox_returnAllRecords.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_returnAllRecords.Name = "checkBox_returnAllRecords";
            this.checkBox_returnAllRecords.Size = new System.Drawing.Size(205, 22);
            this.checkBox_returnAllRecords.TabIndex = 1;
            this.checkBox_returnAllRecords.Text = "返回全部命中记录(&A)";
            this.checkBox_returnAllRecords.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_dupMessage, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listView_browse, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label_message, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(534, 464);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboBox_projectName);
            this.panel1.Controls.Add(this.button_search);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(848, 36);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.textBox_recordPath);
            this.panel2.Controls.Add(this.button_viewXmlRecord);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 36);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(848, 36);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.Control;
            this.panel3.Controls.Add(this.flowLayoutPanel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 375);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(848, 50);
            this.panel3.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.checkBox_includeLowCols);
            this.flowLayoutPanel1.Controls.Add(this.checkBox_returnAllRecords);
            this.flowLayoutPanel1.Controls.Add(this.checkBox_returnSearchDetail);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(848, 50);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // checkBox_returnSearchDetail
            // 
            this.checkBox_returnSearchDetail.AutoSize = true;
            this.checkBox_returnSearchDetail.Location = new System.Drawing.Point(519, 3);
            this.checkBox_returnSearchDetail.Name = "checkBox_returnSearchDetail";
            this.checkBox_returnSearchDetail.Size = new System.Drawing.Size(169, 22);
            this.checkBox_returnSearchDetail.TabIndex = 2;
            this.checkBox_returnSearchDetail.Text = "返回检索详情(&S)";
            this.checkBox_returnSearchDetail.UseVisualStyleBackColor = true;
            // 
            // DupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 464);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DupForm";
            this.ShowInTaskbar = false;
            this.Text = "DupForm";
            this.Activated += new System.EventHandler(this.DupForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DupForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DupForm_FormClosed);
            this.Load += new System.EventHandler(this.DupForm_Load);
            this.SizeChanged += new System.EventHandler(this.DupForm_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_dupMessage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_recordPath;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.GUI.ListViewNF listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_sum;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.ComboBox comboBox_projectName;
        private System.Windows.Forms.Button button_viewXmlRecord;
        private System.Windows.Forms.ImageList imageList_dupItemType;
        private System.Windows.Forms.CheckBox checkBox_includeLowCols;
        private System.Windows.Forms.CheckBox checkBox_returnAllRecords;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBox_returnSearchDetail;
    }
}