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
            this.SuspendLayout();
            // 
            // label_dupMessage
            // 
            this.label_dupMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_dupMessage.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_dupMessage.Location = new System.Drawing.Point(7, 241);
            this.label_dupMessage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_dupMessage.Name = "label_dupMessage";
            this.label_dupMessage.Size = new System.Drawing.Size(399, 24);
            this.label_dupMessage.TabIndex = 8;
            this.label_dupMessage.Text = "尚未查重...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "查重方案(&P):";
            // 
            // textBox_recordPath
            // 
            this.textBox_recordPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recordPath.Location = new System.Drawing.Point(117, 34);
            this.textBox_recordPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_recordPath.Name = "textBox_recordPath";
            this.textBox_recordPath.Size = new System.Drawing.Size(210, 21);
            this.textBox_recordPath.TabIndex = 4;
            this.textBox_recordPath.TextChanged += new System.EventHandler(this.textBox_recordPath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "源记录路径(&P):";
            // 
            // listView_browse
            // 
            this.listView_browse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_sum});
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.LargeImageList = this.imageList_dupItemType;
            this.listView_browse.Location = new System.Drawing.Point(9, 60);
            this.listView_browse.Margin = new System.Windows.Forms.Padding(2);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(398, 157);
            this.listView_browse.SmallImageList = this.imageList_dupItemType;
            this.listView_browse.TabIndex = 6;
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
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(7, 272);
            this.label_message.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(399, 24);
            this.label_message.TabIndex = 9;
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(331, 7);
            this.button_search.Margin = new System.Windows.Forms.Padding(2);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(75, 22);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "查重(&S)";
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // comboBox_projectName
            // 
            this.comboBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_projectName.FormattingEnabled = true;
            this.comboBox_projectName.Location = new System.Drawing.Point(117, 10);
            this.comboBox_projectName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_projectName.Name = "comboBox_projectName";
            this.comboBox_projectName.Size = new System.Drawing.Size(210, 20);
            this.comboBox_projectName.TabIndex = 1;
            this.comboBox_projectName.DropDown += new System.EventHandler(this.comboBox_projectName_DropDown);
            // 
            // button_viewXmlRecord
            // 
            this.button_viewXmlRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_viewXmlRecord.Location = new System.Drawing.Point(331, 33);
            this.button_viewXmlRecord.Margin = new System.Windows.Forms.Padding(2);
            this.button_viewXmlRecord.Name = "button_viewXmlRecord";
            this.button_viewXmlRecord.Size = new System.Drawing.Size(75, 22);
            this.button_viewXmlRecord.TabIndex = 5;
            this.button_viewXmlRecord.Text = "XML...";
            this.button_viewXmlRecord.UseVisualStyleBackColor = true;
            this.button_viewXmlRecord.Click += new System.EventHandler(this.button_viewXmlRecord_Click);
            // 
            // checkBox_includeLowCols
            // 
            this.checkBox_includeLowCols.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_includeLowCols.AutoSize = true;
            this.checkBox_includeLowCols.Location = new System.Drawing.Point(9, 222);
            this.checkBox_includeLowCols.Name = "checkBox_includeLowCols";
            this.checkBox_includeLowCols.Size = new System.Drawing.Size(198, 16);
            this.checkBox_includeLowCols.TabIndex = 6;
            this.checkBox_includeLowCols.Text = "返回低于阈值的记录的浏览列(&B)";
            this.checkBox_includeLowCols.UseVisualStyleBackColor = true;
            // 
            // checkBox_returnAllRecords
            // 
            this.checkBox_returnAllRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_returnAllRecords.AutoSize = true;
            this.checkBox_returnAllRecords.Location = new System.Drawing.Point(225, 222);
            this.checkBox_returnAllRecords.Name = "checkBox_returnAllRecords";
            this.checkBox_returnAllRecords.Size = new System.Drawing.Size(138, 16);
            this.checkBox_returnAllRecords.TabIndex = 7;
            this.checkBox_returnAllRecords.Text = "返回全部命中记录(&A)";
            this.checkBox_returnAllRecords.UseVisualStyleBackColor = true;
            // 
            // DupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 303);
            this.Controls.Add(this.checkBox_returnAllRecords);
            this.Controls.Add(this.checkBox_includeLowCols);
            this.Controls.Add(this.button_viewXmlRecord);
            this.Controls.Add(this.comboBox_projectName);
            this.Controls.Add(this.label_dupMessage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_recordPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_browse);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_search);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DupForm";
            this.ShowInTaskbar = false;
            this.Text = "DupForm";
            this.Activated += new System.EventHandler(this.DupForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DupForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DupForm_FormClosed);
            this.Load += new System.EventHandler(this.DupForm_Load);
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
    }
}