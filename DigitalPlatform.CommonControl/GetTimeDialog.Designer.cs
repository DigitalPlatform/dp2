namespace DigitalPlatform.CommonControl
{
    partial class GetTimeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetTimeDialog));
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_clearHMS = new System.Windows.Forms.Button();
            this.button_clearHMS_2 = new System.Windows.Forms.Button();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.button_setMin = new System.Windows.Forms.Button();
            this.button_setMax2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(12, 12);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker1.TabIndex = 0;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(132, 166);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(213, 166);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_clearHMS
            // 
            this.button_clearHMS.Location = new System.Drawing.Point(12, 39);
            this.button_clearHMS.Name = "button_clearHMS";
            this.button_clearHMS.Size = new System.Drawing.Size(145, 23);
            this.button_clearHMS.TabIndex = 1;
            this.button_clearHMS.Text = "清除时分秒部分";
            this.button_clearHMS.UseVisualStyleBackColor = true;
            this.button_clearHMS.Click += new System.EventHandler(this.button_clearHMS_Click);
            // 
            // button_clearHMS_2
            // 
            this.button_clearHMS_2.Location = new System.Drawing.Point(12, 113);
            this.button_clearHMS_2.Name = "button_clearHMS_2";
            this.button_clearHMS_2.Size = new System.Drawing.Size(145, 23);
            this.button_clearHMS_2.TabIndex = 4;
            this.button_clearHMS_2.Text = "清除时分秒部分";
            this.button_clearHMS_2.UseVisualStyleBackColor = true;
            this.button_clearHMS_2.Visible = false;
            this.button_clearHMS_2.Click += new System.EventHandler(this.button_clearHMS_2_Click);
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker2.Location = new System.Drawing.Point(12, 86);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker2.TabIndex = 3;
            this.dateTimePicker2.Visible = false;
            // 
            // button_setMin
            // 
            this.button_setMin.Location = new System.Drawing.Point(164, 39);
            this.button_setMin.Name = "button_setMin";
            this.button_setMin.Size = new System.Drawing.Size(75, 23);
            this.button_setMin.TabIndex = 2;
            this.button_setMin.Text = "极小值";
            this.button_setMin.UseVisualStyleBackColor = true;
            this.button_setMin.Click += new System.EventHandler(this.button_setMin_Click);
            // 
            // button_setMax2
            // 
            this.button_setMax2.Location = new System.Drawing.Point(163, 113);
            this.button_setMax2.Name = "button_setMax2";
            this.button_setMax2.Size = new System.Drawing.Size(75, 23);
            this.button_setMax2.TabIndex = 5;
            this.button_setMax2.Text = "极大值";
            this.button_setMax2.UseVisualStyleBackColor = true;
            this.button_setMax2.Visible = false;
            this.button_setMax2.Click += new System.EventHandler(this.button_setMax2_Click);
            // 
            // GetTimeDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(300, 201);
            this.Controls.Add(this.button_setMax2);
            this.Controls.Add(this.button_setMin);
            this.Controls.Add(this.button_clearHMS_2);
            this.Controls.Add(this.dateTimePicker2);
            this.Controls.Add(this.button_clearHMS);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.dateTimePicker1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GetTimeDialog";
            this.ShowInTaskbar = false;
            this.Text = "指定时间";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_clearHMS;
        private System.Windows.Forms.Button button_clearHMS_2;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Button button_setMin;
        private System.Windows.Forms.Button button_setMax2;
    }
}