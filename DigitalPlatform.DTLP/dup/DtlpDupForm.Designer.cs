namespace DigitalPlatform.DTLP
{
    partial class DtlpDupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DtlpDupForm));
            this.button_viewMarcRecord = new System.Windows.Forms.Button();
            this.comboBox_projectName = new System.Windows.Forms.ComboBox();
            this.label_dupMessage = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_recordPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listView_browse = new System.Windows.Forms.ListView();
            this.columnHeader_path = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_sum = new System.Windows.Forms.ColumnHeader();
            this.label_message = new System.Windows.Forms.Label();
            this.button_search = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_viewMarcRecord
            // 
            this.button_viewMarcRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_viewMarcRecord.Enabled = false;
            this.button_viewMarcRecord.Location = new System.Drawing.Point(443, 45);
            this.button_viewMarcRecord.Name = "button_viewMarcRecord";
            this.button_viewMarcRecord.Size = new System.Drawing.Size(100, 27);
            this.button_viewMarcRecord.TabIndex = 5;
            this.button_viewMarcRecord.Text = "MARC...";
            this.button_viewMarcRecord.UseVisualStyleBackColor = true;
            // 
            // comboBox_projectName
            // 
            this.comboBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_projectName.FormattingEnabled = true;
            this.comboBox_projectName.Location = new System.Drawing.Point(157, 12);
            this.comboBox_projectName.Name = "comboBox_projectName";
            this.comboBox_projectName.Size = new System.Drawing.Size(279, 23);
            this.comboBox_projectName.TabIndex = 1;
            this.comboBox_projectName.DropDown += new System.EventHandler(this.comboBox_projectName_DropDown);
            // 
            // label_dupMessage
            // 
            this.label_dupMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_dupMessage.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_dupMessage.Location = new System.Drawing.Point(9, 331);
            this.label_dupMessage.Name = "label_dupMessage";
            this.label_dupMessage.Size = new System.Drawing.Size(534, 30);
            this.label_dupMessage.TabIndex = 7;
            this.label_dupMessage.Text = "尚未查重...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "查重方案(&P):";
            // 
            // textBox_recordPath
            // 
            this.textBox_recordPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recordPath.Location = new System.Drawing.Point(157, 43);
            this.textBox_recordPath.Name = "textBox_recordPath";
            this.textBox_recordPath.Size = new System.Drawing.Size(279, 25);
            this.textBox_recordPath.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
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
            this.listView_browse.Location = new System.Drawing.Point(12, 78);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(531, 250);
            this.listView_browse.TabIndex = 6;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_browse_ColumnClick);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "记录路径";
            this.columnHeader_path.Width = 260;
            // 
            // columnHeader_sum
            // 
            this.columnHeader_sum.Text = "权值和";
            this.columnHeader_sum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_sum.Width = 100;
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(11, 370);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(532, 30);
            this.label_message.TabIndex = 8;
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(443, 12);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(100, 27);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "查重(&S)";
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // DtlpDupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 411);
            this.Controls.Add(this.button_viewMarcRecord);
            this.Controls.Add(this.comboBox_projectName);
            this.Controls.Add(this.label_dupMessage);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_recordPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_browse);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_search);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DtlpDupForm";
            this.ShowInTaskbar = false;
            this.Text = "DTLP查重";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DtlpDupForm_FormClosed);
            this.Load += new System.EventHandler(this.DupForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_viewMarcRecord;
        private System.Windows.Forms.ComboBox comboBox_projectName;
        private System.Windows.Forms.Label label_dupMessage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_recordPath;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ListView listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_sum;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_search;
    }
}