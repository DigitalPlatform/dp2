namespace dp2Circulation
{
    partial class GetOperLogFilenameDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetOperLogFilenameDlg));
            this.label_end = new System.Windows.Forms.Label();
            this.label_start = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.dateControl_end = new DigitalPlatform.CommonControl.DateControl();
            this.dateControl_start = new DigitalPlatform.CommonControl.DateControl();
            this.SuspendLayout();
            // 
            // label_end
            // 
            this.label_end.AutoSize = true;
            this.label_end.Location = new System.Drawing.Point(9, 43);
            this.label_end.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_end.Name = "label_end";
            this.label_end.Size = new System.Drawing.Size(89, 12);
            this.label_end.TabIndex = 2;
            this.label_end.Text = "日志结束日(&E):";
            this.label_end.Visible = false;
            // 
            // label_start
            // 
            this.label_start.AutoSize = true;
            this.label_start.Location = new System.Drawing.Point(9, 15);
            this.label_start.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_start.Name = "label_start";
            this.label_start.Size = new System.Drawing.Size(89, 12);
            this.label_start.TabIndex = 0;
            this.label_start.Text = "日志起始日(&S):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(176, 86);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(116, 86);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // dateControl_end
            // 
            this.dateControl_end.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_end.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_end.Location = new System.Drawing.Point(116, 41);
            this.dateControl_end.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_end.Name = "dateControl_end";
            this.dateControl_end.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_end.Size = new System.Drawing.Size(115, 22);
            this.dateControl_end.TabIndex = 3;
            this.dateControl_end.Value = new System.DateTime(((long)(0)));
            this.dateControl_end.Visible = false;
            // 
            // dateControl_start
            // 
            this.dateControl_start.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_start.Location = new System.Drawing.Point(116, 15);
            this.dateControl_start.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_start.Name = "dateControl_start";
            this.dateControl_start.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_start.Size = new System.Drawing.Size(115, 22);
            this.dateControl_start.TabIndex = 1;
            this.dateControl_start.Value = new System.DateTime(((long)(0)));
            this.dateControl_start.DateTextChanged += new System.EventHandler(this.dateControl_start_DateTextChanged);
            // 
            // GetOperLogFilenameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(242, 118);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.dateControl_end);
            this.Controls.Add(this.dateControl_start);
            this.Controls.Add(this.label_end);
            this.Controls.Add(this.label_start);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "GetOperLogFilenameDlg";
            this.ShowInTaskbar = false;
            this.Text = "指定日志文件名";
            this.Load += new System.EventHandler(this.GetOperLogFilenameDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DigitalPlatform.CommonControl.DateControl dateControl_end;
        private DigitalPlatform.CommonControl.DateControl dateControl_start;
        private System.Windows.Forms.Label label_end;
        private System.Windows.Forms.Label label_start;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}