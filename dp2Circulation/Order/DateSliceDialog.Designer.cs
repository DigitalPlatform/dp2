namespace dp2Circulation
{
    partial class DateSliceDialog
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
            this.comboBox_quickSetTimeRange = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dateControl_end = new DigitalPlatform.CommonControl.DateControl();
            this.dateControl_start = new DigitalPlatform.CommonControl.DateControl();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_slice = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox_quickSetTimeRange
            // 
            this.comboBox_quickSetTimeRange.FormattingEnabled = true;
            this.comboBox_quickSetTimeRange.Items.AddRange(new object[] {
            "订购日至今日",
            "今日(一日)",
            "本周",
            "本月",
            "本年",
            "最近 7 天",
            "最近 30 天",
            "最近 31 天",
            "最近 365 天",
            "最近 2 年",
            "最近 3 年",
            "最近 10 年"});
            this.comboBox_quickSetTimeRange.Location = new System.Drawing.Point(147, 77);
            this.comboBox_quickSetTimeRange.Name = "comboBox_quickSetTimeRange";
            this.comboBox_quickSetTimeRange.Size = new System.Drawing.Size(111, 20);
            this.comboBox_quickSetTimeRange.TabIndex = 16;
            this.comboBox_quickSetTimeRange.SelectedIndexChanged += new System.EventHandler(this.comboBox_quickSetTimeRange_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 12);
            this.label6.TabIndex = 15;
            this.label6.Text = "快速设定时间范围(&Q):";
            // 
            // dateControl_end
            // 
            this.dateControl_end.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_end.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_end.Location = new System.Drawing.Point(147, 48);
            this.dateControl_end.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_end.Name = "dateControl_end";
            this.dateControl_end.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_end.Size = new System.Drawing.Size(111, 22);
            this.dateControl_end.TabIndex = 14;
            this.dateControl_end.Value = new System.DateTime(((long)(0)));
            // 
            // dateControl_start
            // 
            this.dateControl_start.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_start.Location = new System.Drawing.Point(148, 20);
            this.dateControl_start.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_start.Name = "dateControl_start";
            this.dateControl_start.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_start.Size = new System.Drawing.Size(110, 22);
            this.dateControl_start.TabIndex = 13;
            this.dateControl_start.Value = new System.DateTime(((long)(0)));
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 12;
            this.label2.Text = "结束日(&E):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "起始日(&S):";
            // 
            // comboBox_slice
            // 
            this.comboBox_slice.FormattingEnabled = true;
            this.comboBox_slice.Items.AddRange(new object[] {
            "<不切片>",
            "日",
            "月",
            "年"});
            this.comboBox_slice.Location = new System.Drawing.Point(148, 121);
            this.comboBox_slice.Name = "comboBox_slice";
            this.comboBox_slice.Size = new System.Drawing.Size(111, 20);
            this.comboBox_slice.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 17;
            this.label3.Text = "切片(&S):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(216, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 19;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(297, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 20;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // DateSliceDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(384, 264);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_slice);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_quickSetTimeRange);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dateControl_end);
            this.Controls.Add(this.dateControl_start);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "DateSliceDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "时间切片";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_quickSetTimeRange;
        private System.Windows.Forms.Label label6;
        private DigitalPlatform.CommonControl.DateControl dateControl_end;
        private DigitalPlatform.CommonControl.DateControl dateControl_start;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_slice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}