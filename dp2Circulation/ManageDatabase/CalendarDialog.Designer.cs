namespace dp2Circulation
{
    partial class CalendarDialog
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
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_timeRange = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.calenderControl1 = new DigitalPlatform.CommonDialog.CalendarControl();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_libraryCode = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(379, 323);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(460, 323);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 10;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(386, 94);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.Size = new System.Drawing.Size(149, 115);
            this.textBox_comment.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(305, 97);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "注释(&C):";
            // 
            // textBox_timeRange
            // 
            this.textBox_timeRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_timeRange.Location = new System.Drawing.Point(386, 69);
            this.textBox_timeRange.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_timeRange.Name = "textBox_timeRange";
            this.textBox_timeRange.ReadOnly = true;
            this.textBox_timeRange.Size = new System.Drawing.Size(149, 21);
            this.textBox_timeRange.TabIndex = 5;
            this.textBox_timeRange.Enter += new System.EventHandler(this.textBox_timeRange_Enter);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(305, 72);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "时间范围(&R):";
            // 
            // calenderControl1
            // 
            this.calenderControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.calenderControl1.BackColor = System.Drawing.Color.Gray;
            this.calenderControl1.Changed = false;
            this.calenderControl1.DayCellHeight = 60;
            this.calenderControl1.DayCellWidth = 80;
            new DigitalPlatform.CommonDialog.DayStateDefCollection().Add(dayStateDef1);
            new DigitalPlatform.CommonDialog.DayStateDefCollection().Add(dayStateDef2);
            this.calenderControl1.DocumentOrgX = ((long)(0));
            this.calenderControl1.DocumentOrgY = ((long)(0));
            this.calenderControl1.Location = new System.Drawing.Point(11, 11);
            this.calenderControl1.Margin = new System.Windows.Forms.Padding(2);
            this.calenderControl1.Name = "calenderControl1";
            this.calenderControl1.Size = new System.Drawing.Size(289, 335);
            this.calenderControl1.TabIndex = 8;
            this.calenderControl1.Text = "calenderControl1";
            this.calenderControl1.TimeRange = "";
            this.calenderControl1.Leave += new System.EventHandler(this.calenderControl1_Leave);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(305, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "日历名(&N):";
            // 
            // textBox_name
            // 
            this.textBox_name.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_name.Location = new System.Drawing.Point(386, 43);
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(149, 21);
            this.textBox_name.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(305, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "馆代码(&L):";
            // 
            // comboBox_libraryCode
            // 
            this.comboBox_libraryCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_libraryCode.FormattingEnabled = true;
            this.comboBox_libraryCode.Location = new System.Drawing.Point(386, 17);
            this.comboBox_libraryCode.Name = "comboBox_libraryCode";
            this.comboBox_libraryCode.Size = new System.Drawing.Size(149, 20);
            this.comboBox_libraryCode.TabIndex = 1;
            // 
            // CalendarDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(546, 357);
            this.Controls.Add(this.comboBox_libraryCode);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_name);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_timeRange);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.calenderControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "CalendarDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "日历";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalendarDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CalendarDialog_FormClosed);
            this.Load += new System.EventHandler(this.CalendarDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_timeRange;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.CommonDialog.CalendarControl calenderControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_name;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_libraryCode;
    }
}