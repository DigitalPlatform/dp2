namespace dp2Circulation
{
    partial class CalendarForm
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
            DigitalPlatform.CommonDialog.DayStateDef dayStateDef1 = new DigitalPlatform.CommonDialog.DayStateDef();
            DigitalPlatform.CommonDialog.DayStateDef dayStateDef2 = new DigitalPlatform.CommonDialog.DayStateDef();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalendarForm));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_calendarName = new System.Windows.Forms.ComboBox();
            this.button_load = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_timeRange = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button_save = new System.Windows.Forms.Button();
            this.calenderControl1 = new DigitalPlatform.CommonDialog.CalendarControl();
            this.button_create = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "日历名(&N):";
            // 
            // comboBox_calendarName
            // 
            this.comboBox_calendarName.FormattingEnabled = true;
            this.comboBox_calendarName.Location = new System.Drawing.Point(120, 13);
            this.comboBox_calendarName.Name = "comboBox_calendarName";
            this.comboBox_calendarName.Size = new System.Drawing.Size(260, 23);
            this.comboBox_calendarName.TabIndex = 1;
            this.comboBox_calendarName.SelectionChangeCommitted += new System.EventHandler(this.comboBox_calendarName_SelectionChangeCommitted);
            this.comboBox_calendarName.Enter += new System.EventHandler(this.comboBox_calendarName_Enter);
            this.comboBox_calendarName.DropDownClosed += new System.EventHandler(this.comboBox_calendarName_DropDownClosed);
            this.comboBox_calendarName.DropDown += new System.EventHandler(this.comboBox_calendarName_DropDown);
            // 
            // button_load
            // 
            this.button_load.Location = new System.Drawing.Point(387, 13);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(102, 28);
            this.button_load.TabIndex = 2;
            this.button_load.Text = "装载(&L)";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "时间范围 (&R):";
            // 
            // textBox_timeRange
            // 
            this.textBox_timeRange.Location = new System.Drawing.Point(120, 67);
            this.textBox_timeRange.Name = "textBox_timeRange";
            this.textBox_timeRange.ReadOnly = true;
            this.textBox_timeRange.Size = new System.Drawing.Size(260, 25);
            this.textBox_timeRange.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "注释(&C):";
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(120, 98);
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.Size = new System.Drawing.Size(460, 25);
            this.textBox_comment.TabIndex = 6;
            this.textBox_comment.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(12, 51);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(568, 2);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // button_save
            // 
            this.button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_save.Enabled = false;
            this.button_save.Location = new System.Drawing.Point(495, 388);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(85, 28);
            this.button_save.TabIndex = 9;
            this.button_save.Text = "保存(&S)";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // calenderControl1
            // 
            this.calenderControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.calenderControl1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.calenderControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.calenderControl1.Changed = false;
            new DigitalPlatform.CommonDialog.DayStateDefCollection().Add(dayStateDef1);
            new DigitalPlatform.CommonDialog.DayStateDefCollection().Add(dayStateDef2);
            this.calenderControl1.DocumentOrgX = ((long)(0));
            this.calenderControl1.DocumentOrgY = ((long)(0));
            this.calenderControl1.Location = new System.Drawing.Point(12, 129);
            this.calenderControl1.Name = "calenderControl1";
            this.calenderControl1.Size = new System.Drawing.Size(568, 245);
            this.calenderControl1.TabIndex = 7;
            this.calenderControl1.Text = "calenderControl1";
            this.calenderControl1.TimeRange = "";
            this.calenderControl1.Enter += new System.EventHandler(this.calenderControl1_Enter);
            this.calenderControl1.BoxStateChanged += new System.EventHandler(this.calenderControl1_BoxStateChanged);
            // 
            // button_create
            // 
            this.button_create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_create.Location = new System.Drawing.Point(404, 388);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(85, 28);
            this.button_create.TabIndex = 8;
            this.button_create.Text = "创建(&C)";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_delete.Location = new System.Drawing.Point(12, 388);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(85, 28);
            this.button_delete.TabIndex = 10;
            this.button_delete.Text = "删除(&D)";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // CalendarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 428);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_timeRange);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.calenderControl1);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.comboBox_calendarName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CalendarForm";
            this.ShowInTaskbar = false;
            this.Text = "日历";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CalendarForm_FormClosed);
            this.Activated += new System.EventHandler(this.CalendarForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalendarForm_FormClosing);
            this.Load += new System.EventHandler(this.CalendarForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_calendarName;
        private System.Windows.Forms.Button button_load;
        private DigitalPlatform.CommonDialog.CalendarControl calenderControl1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_timeRange;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.Button button_delete;
    }
}