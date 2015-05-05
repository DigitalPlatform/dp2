namespace dp2Catalog
{
    partial class DtlpLogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DtlpLogForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.listView_records = new System.Windows.Forms.ListView();
            this.columnHeader_index = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_position = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_operType = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_path = new System.Windows.Forms.ColumnHeader();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_logFileName = new System.Windows.Forms.TextBox();
            this.button_load = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.splitContainer_oneRecord = new System.Windows.Forms.SplitContainer();
            this.panel_worksheet = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_worksheet = new System.Windows.Forms.TextBox();
            this.splitContainer_down = new System.Windows.Forms.SplitContainer();
            this.panel_marc = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.marcEditor_record = new DigitalPlatform.Marc.MarcEditor();
            this.panel_desciption = new System.Windows.Forms.Panel();
            this.textBox_description = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_loop = new System.Windows.Forms.CheckBox();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.splitContainer_oneRecord.Panel1.SuspendLayout();
            this.splitContainer_oneRecord.Panel2.SuspendLayout();
            this.splitContainer_oneRecord.SuspendLayout();
            this.panel_worksheet.SuspendLayout();
            this.splitContainer_down.Panel1.SuspendLayout();
            this.splitContainer_down.Panel2.SuspendLayout();
            this.splitContainer_down.SuspendLayout();
            this.panel_marc.SuspendLayout();
            this.panel_desciption.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器地址(&S):";
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Location = new System.Drawing.Point(159, 12);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(230, 25);
            this.textBox_serverAddr.TabIndex = 1;
            // 
            // listView_records
            // 
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_position,
            this.columnHeader_operType,
            this.columnHeader_path});
            this.listView_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(0, 0);
            this.listView_records.Name = "listView_records";
            this.listView_records.Size = new System.Drawing.Size(205, 317);
            this.listView_records.TabIndex = 2;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "序号";
            // 
            // columnHeader_position
            // 
            this.columnHeader_position.Text = "偏移";
            this.columnHeader_position.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_position.Width = 120;
            // 
            // columnHeader_operType
            // 
            this.columnHeader_operType.Text = "操作类型";
            this.columnHeader_operType.Width = 100;
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "操作路径";
            this.columnHeader_path.Width = 300;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "日志文件名(&L):";
            // 
            // textBox_logFileName
            // 
            this.textBox_logFileName.Location = new System.Drawing.Point(159, 44);
            this.textBox_logFileName.Name = "textBox_logFileName";
            this.textBox_logFileName.Size = new System.Drawing.Size(230, 25);
            this.textBox_logFileName.TabIndex = 4;
            // 
            // button_load
            // 
            this.button_load.Location = new System.Drawing.Point(395, 44);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(101, 27);
            this.button_load.TabIndex = 5;
            this.button_load.Text = "装载(&L)";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(12, 77);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.listView_records);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.splitContainer_oneRecord);
            this.splitContainer_main.Size = new System.Drawing.Size(615, 317);
            this.splitContainer_main.SplitterDistance = 205;
            this.splitContainer_main.TabIndex = 6;
            // 
            // splitContainer_oneRecord
            // 
            this.splitContainer_oneRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_oneRecord.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_oneRecord.Name = "splitContainer_oneRecord";
            this.splitContainer_oneRecord.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_oneRecord.Panel1
            // 
            this.splitContainer_oneRecord.Panel1.Controls.Add(this.panel_worksheet);
            // 
            // splitContainer_oneRecord.Panel2
            // 
            this.splitContainer_oneRecord.Panel2.Controls.Add(this.splitContainer_down);
            this.splitContainer_oneRecord.Size = new System.Drawing.Size(406, 317);
            this.splitContainer_oneRecord.SplitterDistance = 117;
            this.splitContainer_oneRecord.TabIndex = 1;
            // 
            // panel_worksheet
            // 
            this.panel_worksheet.Controls.Add(this.label3);
            this.panel_worksheet.Controls.Add(this.textBox_worksheet);
            this.panel_worksheet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_worksheet.Location = new System.Drawing.Point(0, 0);
            this.panel_worksheet.Name = "panel_worksheet";
            this.panel_worksheet.Size = new System.Drawing.Size(406, 117);
            this.panel_worksheet.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "原始数据:";
            // 
            // textBox_worksheet
            // 
            this.textBox_worksheet.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_worksheet.Location = new System.Drawing.Point(0, 18);
            this.textBox_worksheet.Multiline = true;
            this.textBox_worksheet.Name = "textBox_worksheet";
            this.textBox_worksheet.ReadOnly = true;
            this.textBox_worksheet.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_worksheet.Size = new System.Drawing.Size(403, 99);
            this.textBox_worksheet.TabIndex = 0;
            // 
            // splitContainer_down
            // 
            this.splitContainer_down.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_down.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_down.Name = "splitContainer_down";
            this.splitContainer_down.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_down.Panel1
            // 
            this.splitContainer_down.Panel1.Controls.Add(this.panel_marc);
            // 
            // splitContainer_down.Panel2
            // 
            this.splitContainer_down.Panel2.Controls.Add(this.panel_desciption);
            this.splitContainer_down.Size = new System.Drawing.Size(406, 196);
            this.splitContainer_down.SplitterDistance = 130;
            this.splitContainer_down.TabIndex = 1;
            // 
            // panel_marc
            // 
            this.panel_marc.Controls.Add(this.label4);
            this.panel_marc.Controls.Add(this.marcEditor_record);
            this.panel_marc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_marc.Location = new System.Drawing.Point(0, 0);
            this.panel_marc.Name = "panel_marc";
            this.panel_marc.Size = new System.Drawing.Size(406, 130);
            this.panel_marc.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-3, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "MARC:";
            // 
            // marcEditor_record
            // 
            this.marcEditor_record.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.marcEditor_record.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.marcEditor_record.Changed = true;
            this.marcEditor_record.ContentBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor_record.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.marcEditor_record.DocumentOrgX = 0;
            this.marcEditor_record.DocumentOrgY = 0;
            this.marcEditor_record.FieldNameCaptionWidth = 60;
            this.marcEditor_record.FocusedField = null;
            this.marcEditor_record.FocusedFieldIndex = -1;
            this.marcEditor_record.HorzGridColor = System.Drawing.Color.LightGray;
            this.marcEditor_record.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor_record.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.marcEditor_record.IndicatorTextColor = System.Drawing.Color.Green;
            this.marcEditor_record.Location = new System.Drawing.Point(0, 22);
            this.marcEditor_record.Marc = "????????????????????????";
            this.marcEditor_record.MarcDefDom = null;
            this.marcEditor_record.Name = "marcEditor_record";
            this.marcEditor_record.NameBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor_record.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.marcEditor_record.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.marcEditor_record.NameTextColor = System.Drawing.Color.Blue;
            this.marcEditor_record.SelectionStart = 0;
            this.marcEditor_record.Size = new System.Drawing.Size(403, 108);
            this.marcEditor_record.TabIndex = 0;
            this.marcEditor_record.Text = "marcEditor1";
            this.marcEditor_record.VertGridColor = System.Drawing.Color.LightGray;
            // 
            // panel_desciption
            // 
            this.panel_desciption.Controls.Add(this.textBox_description);
            this.panel_desciption.Controls.Add(this.label5);
            this.panel_desciption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_desciption.Location = new System.Drawing.Point(0, 0);
            this.panel_desciption.Name = "panel_desciption";
            this.panel_desciption.Size = new System.Drawing.Size(406, 62);
            this.panel_desciption.TabIndex = 0;
            // 
            // textBox_description
            // 
            this.textBox_description.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_description.Location = new System.Drawing.Point(0, 18);
            this.textBox_description.Multiline = true;
            this.textBox_description.Name = "textBox_description";
            this.textBox_description.ReadOnly = true;
            this.textBox_description.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_description.Size = new System.Drawing.Size(403, 44);
            this.textBox_description.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "解释:";
            // 
            // checkBox_loop
            // 
            this.checkBox_loop.AutoSize = true;
            this.checkBox_loop.Location = new System.Drawing.Point(502, 49);
            this.checkBox_loop.Name = "checkBox_loop";
            this.checkBox_loop.Size = new System.Drawing.Size(110, 19);
            this.checkBox_loop.TabIndex = 7;
            this.checkBox_loop.Text = "持续获取(&L)";
            this.checkBox_loop.UseVisualStyleBackColor = true;
            // 
            // DtlpLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 406);
            this.Controls.Add(this.checkBox_loop);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.textBox_logFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_serverAddr);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DtlpLogForm";
            this.ShowInTaskbar = false;
            this.Text = "DTLP日志窗";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DtlpLogForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DtlpLogForm_FormClosing);
            this.Load += new System.EventHandler(this.DtlpLogForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_oneRecord.Panel1.ResumeLayout(false);
            this.splitContainer_oneRecord.Panel2.ResumeLayout(false);
            this.splitContainer_oneRecord.ResumeLayout(false);
            this.panel_worksheet.ResumeLayout(false);
            this.panel_worksheet.PerformLayout();
            this.splitContainer_down.Panel1.ResumeLayout(false);
            this.splitContainer_down.Panel2.ResumeLayout(false);
            this.splitContainer_down.ResumeLayout(false);
            this.panel_marc.ResumeLayout(false);
            this.panel_marc.PerformLayout();
            this.panel_desciption.ResumeLayout(false);
            this.panel_desciption.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_serverAddr;
        private System.Windows.Forms.ListView listView_records;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_logFileName;
        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_position;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TextBox textBox_worksheet;
        private System.Windows.Forms.SplitContainer splitContainer_oneRecord;
        private DigitalPlatform.Marc.MarcEditor marcEditor_record;
        private System.Windows.Forms.Panel panel_worksheet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer_down;
        private System.Windows.Forms.Panel panel_marc;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel_desciption;
        private System.Windows.Forms.TextBox textBox_description;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader columnHeader_operType;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.CheckBox checkBox_loop;
    }
}