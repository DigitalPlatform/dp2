namespace DigitalPlatform.CommonControl
{
    partial class DateRangeControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dateTimePicker_start = new System.Windows.Forms.DateTimePicker();
            this.label_start = new System.Windows.Forms.Label();
            this.label_end = new System.Windows.Forms.Label();
            this.dateTimePicker_end = new System.Windows.Forms.DateTimePicker();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // dateTimePicker_start
            // 
            this.dateTimePicker_start.CustomFormat = "yyyy-MM-dd";
            this.dateTimePicker_start.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_start.Location = new System.Drawing.Point(22, 2);
            this.dateTimePicker_start.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dateTimePicker_start.Name = "dateTimePicker_start";
            this.dateTimePicker_start.Size = new System.Drawing.Size(82, 21);
            this.dateTimePicker_start.TabIndex = 1;
            this.dateTimePicker_start.Value = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this.dateTimePicker_start.ValueChanged += new System.EventHandler(this.dateTimePicker_start_ValueChanged);
            this.dateTimePicker_start.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_start_MouseUp);
            // 
            // label_start
            // 
            this.label_start.AutoSize = true;
            this.label_start.Location = new System.Drawing.Point(1, 6);
            this.label_start.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_start.Name = "label_start";
            this.label_start.Size = new System.Drawing.Size(17, 12);
            this.label_start.TabIndex = 0;
            this.label_start.Text = "从";
            this.toolTip1.SetToolTip(this.label_start, "用鼠标右键点此可快速设置日期范围...");
            this.label_start.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_start_MouseUp);
            // 
            // label_end
            // 
            this.label_end.AutoSize = true;
            this.label_end.Location = new System.Drawing.Point(1, 25);
            this.label_end.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_end.Name = "label_end";
            this.label_end.Size = new System.Drawing.Size(17, 12);
            this.label_end.TabIndex = 2;
            this.label_end.Text = "到";
            // 
            // dateTimePicker_end
            // 
            this.dateTimePicker_end.CustomFormat = "yyyy-MM-dd";
            this.dateTimePicker_end.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_end.Location = new System.Drawing.Point(22, 22);
            this.dateTimePicker_end.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dateTimePicker_end.Name = "dateTimePicker_end";
            this.dateTimePicker_end.Size = new System.Drawing.Size(82, 21);
            this.dateTimePicker_end.TabIndex = 3;
            this.dateTimePicker_end.Value = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this.dateTimePicker_end.ValueChanged += new System.EventHandler(this.dateTimePicker_end_ValueChanged);
            this.dateTimePicker_end.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_start_MouseUp);
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "操作提示";
            // 
            // DateRangeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_end);
            this.Controls.Add(this.dateTimePicker_end);
            this.Controls.Add(this.label_start);
            this.Controls.Add(this.dateTimePicker_start);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DateRangeControl";
            this.Size = new System.Drawing.Size(105, 45);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker_start;
        private System.Windows.Forms.Label label_start;
        private System.Windows.Forms.Label label_end;
        private System.Windows.Forms.DateTimePicker dateTimePicker_end;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
