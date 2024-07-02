namespace dp2Circulation.SearchForms
{
    partial class ExportDatabaseDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_dbName = new System.Windows.Forms.ComboBox();
            this.checkBox_delete = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDown_endNo = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_startNo = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_noEventLog = new System.Windows.Forms.CheckBox();
            this.checkBox_compressTailNo = new System.Windows.Forms.CheckBox();
            this.tabControl_source = new System.Windows.Forms.TabControl();
            this.tabPage_selected = new System.Windows.Forms.TabPage();
            this.tabPage_database = new System.Windows.Forms.TabPage();
            this.label_selected_message = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_endNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_startNo)).BeginInit();
            this.tabControl_source.SuspendLayout();
            this.tabPage_selected.SuspendLayout();
            this.tabPage_database.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(649, 397);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(504, 397);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "书目库名(&N):";
            // 
            // comboBox_dbName
            // 
            this.comboBox_dbName.FormattingEnabled = true;
            this.comboBox_dbName.Location = new System.Drawing.Point(204, 19);
            this.comboBox_dbName.Name = "comboBox_dbName";
            this.comboBox_dbName.Size = new System.Drawing.Size(351, 29);
            this.comboBox_dbName.TabIndex = 1;
            // 
            // checkBox_delete
            // 
            this.checkBox_delete.AutoSize = true;
            this.checkBox_delete.Location = new System.Drawing.Point(16, 351);
            this.checkBox_delete.Name = "checkBox_delete";
            this.checkBox_delete.Size = new System.Drawing.Size(258, 25);
            this.checkBox_delete.TabIndex = 4;
            this.checkBox_delete.Text = "导出的同时删除记录(&D)";
            this.checkBox_delete.UseVisualStyleBackColor = true;
            this.checkBox_delete.CheckedChanged += new System.EventHandler(this.checkBox_delete_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDown_endNo);
            this.groupBox1.Controls.Add(this.numericUpDown_startNo);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(23, 65);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(532, 150);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " ID 范围 ";
            // 
            // numericUpDown_endNo
            // 
            this.numericUpDown_endNo.Location = new System.Drawing.Point(181, 92);
            this.numericUpDown_endNo.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            this.numericUpDown_endNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_endNo.Name = "numericUpDown_endNo";
            this.numericUpDown_endNo.Size = new System.Drawing.Size(311, 31);
            this.numericUpDown_endNo.TabIndex = 3;
            this.numericUpDown_endNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_endNo.Value = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            // 
            // numericUpDown_startNo
            // 
            this.numericUpDown_startNo.Location = new System.Drawing.Point(181, 46);
            this.numericUpDown_startNo.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            this.numericUpDown_startNo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_startNo.Name = "numericUpDown_startNo";
            this.numericUpDown_startNo.Size = new System.Drawing.Size(311, 31);
            this.numericUpDown_startNo.TabIndex = 1;
            this.numericUpDown_startNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_startNo.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 21);
            this.label3.TabIndex = 2;
            this.label3.Text = "结束 ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 21);
            this.label2.TabIndex = 0;
            this.label2.Text = "起始 ID:";
            // 
            // checkBox_noEventLog
            // 
            this.checkBox_noEventLog.AutoSize = true;
            this.checkBox_noEventLog.Location = new System.Drawing.Point(16, 293);
            this.checkBox_noEventLog.Name = "checkBox_noEventLog";
            this.checkBox_noEventLog.Size = new System.Drawing.Size(216, 25);
            this.checkBox_noEventLog.TabIndex = 3;
            this.checkBox_noEventLog.Text = "不产生操作日志(&E)";
            this.checkBox_noEventLog.UseVisualStyleBackColor = true;
            this.checkBox_noEventLog.Visible = false;
            // 
            // checkBox_compressTailNo
            // 
            this.checkBox_compressTailNo.AutoSize = true;
            this.checkBox_compressTailNo.Location = new System.Drawing.Point(16, 382);
            this.checkBox_compressTailNo.Name = "checkBox_compressTailNo";
            this.checkBox_compressTailNo.Size = new System.Drawing.Size(153, 25);
            this.checkBox_compressTailNo.TabIndex = 5;
            this.checkBox_compressTailNo.Text = "压缩尾号(&C)";
            this.checkBox_compressTailNo.UseVisualStyleBackColor = true;
            // 
            // tabControl_source
            // 
            this.tabControl_source.Controls.Add(this.tabPage_selected);
            this.tabControl_source.Controls.Add(this.tabPage_database);
            this.tabControl_source.Location = new System.Drawing.Point(12, 12);
            this.tabControl_source.Name = "tabControl_source";
            this.tabControl_source.SelectedIndex = 0;
            this.tabControl_source.Size = new System.Drawing.Size(776, 275);
            this.tabControl_source.TabIndex = 8;
            // 
            // tabPage_selected
            // 
            this.tabPage_selected.Controls.Add(this.label_selected_message);
            this.tabPage_selected.Location = new System.Drawing.Point(4, 31);
            this.tabPage_selected.Name = "tabPage_selected";
            this.tabPage_selected.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_selected.Size = new System.Drawing.Size(768, 240);
            this.tabPage_selected.TabIndex = 0;
            this.tabPage_selected.Text = "按选定范围";
            this.tabPage_selected.UseVisualStyleBackColor = true;
            // 
            // tabPage_database
            // 
            this.tabPage_database.Controls.Add(this.groupBox1);
            this.tabPage_database.Controls.Add(this.label1);
            this.tabPage_database.Controls.Add(this.comboBox_dbName);
            this.tabPage_database.Location = new System.Drawing.Point(4, 31);
            this.tabPage_database.Name = "tabPage_database";
            this.tabPage_database.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_database.Size = new System.Drawing.Size(768, 240);
            this.tabPage_database.TabIndex = 1;
            this.tabPage_database.Text = "按书目库";
            this.tabPage_database.UseVisualStyleBackColor = true;
            // 
            // label_selected_message
            // 
            this.label_selected_message.AutoSize = true;
            this.label_selected_message.Location = new System.Drawing.Point(16, 22);
            this.label_selected_message.Name = "label_selected_message";
            this.label_selected_message.Size = new System.Drawing.Size(0, 21);
            this.label_selected_message.TabIndex = 0;
            // 
            // ExportDatabaseDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl_source);
            this.Controls.Add(this.checkBox_compressTailNo);
            this.Controls.Add(this.checkBox_noEventLog);
            this.Controls.Add(this.checkBox_delete);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ExportDatabaseDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "导出整个书目库内的记录";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExportDatabaseDialog_FormClosed);
            this.Load += new System.EventHandler(this.ExportDatabaseDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_endNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_startNo)).EndInit();
            this.tabControl_source.ResumeLayout(false);
            this.tabPage_selected.ResumeLayout(false);
            this.tabPage_selected.PerformLayout();
            this.tabPage_database.ResumeLayout(false);
            this.tabPage_database.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_dbName;
        private System.Windows.Forms.CheckBox checkBox_delete;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_startNo;
        private System.Windows.Forms.NumericUpDown numericUpDown_endNo;
        private System.Windows.Forms.CheckBox checkBox_noEventLog;
        private System.Windows.Forms.CheckBox checkBox_compressTailNo;
        private System.Windows.Forms.TabControl tabControl_source;
        private System.Windows.Forms.TabPage tabPage_selected;
        private System.Windows.Forms.TabPage tabPage_database;
        private System.Windows.Forms.Label label_selected_message;
    }
}