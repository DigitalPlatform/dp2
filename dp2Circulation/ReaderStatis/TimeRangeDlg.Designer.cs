namespace dp2Circulation
{
    partial class TimeRangeDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimeRangeDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.dateControl_start = new DigitalPlatform.CommonControl.DateControl();
            this.dateControl_end = new DigitalPlatform.CommonControl.DateControl();
            this.label2 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "起点(&S):";
            // 
            // dateControl_start
            // 
            this.dateControl_start.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_start.Location = new System.Drawing.Point(78, 11);
            this.dateControl_start.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_start.Name = "dateControl_start";
            this.dateControl_start.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_start.Size = new System.Drawing.Size(118, 22);
            this.dateControl_start.TabIndex = 1;
            this.dateControl_start.Value = new System.DateTime(((long)(0)));
            // 
            // dateControl_end
            // 
            this.dateControl_end.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_end.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_end.Location = new System.Drawing.Point(78, 37);
            this.dateControl_end.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_end.Name = "dateControl_end";
            this.dateControl_end.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_end.Size = new System.Drawing.Size(118, 22);
            this.dateControl_end.TabIndex = 3;
            this.dateControl_end.Value = new System.DateTime(((long)(0)));
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 42);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "终点(&E):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(218, 10);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(218, 37);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // TimeRangeDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(283, 70);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.dateControl_end);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dateControl_start);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TimeRangeDlg";
            this.ShowInTaskbar = false;
            this.Text = "TimeRangeDlg";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.DateControl dateControl_start;
        private DigitalPlatform.CommonControl.DateControl dateControl_end;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}