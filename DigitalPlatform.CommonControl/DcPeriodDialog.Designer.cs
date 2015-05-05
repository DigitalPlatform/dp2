namespace DigitalPlatform.CommonControl
{
    partial class DcPeriodDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_scheme = new System.Windows.Forms.ComboBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.w3cDtfControl_end = new DigitalPlatform.CommonControl.W3cDtfControl();
            this.w3cDtfControl_start = new DigitalPlatform.CommonControl.W3cDtfControl();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称(name=):";
            // 
            // textBox_name
            // 
            this.textBox_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_name.Location = new System.Drawing.Point(12, 31);
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(402, 25);
            this.textBox_name.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(139, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "起始时间(start=):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "结束时间(end=):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 162);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "类型(scheme=):";
            // 
            // comboBox_scheme
            // 
            this.comboBox_scheme.Enabled = false;
            this.comboBox_scheme.FormattingEnabled = true;
            this.comboBox_scheme.Items.AddRange(new object[] {
            "W3C-DTF"});
            this.comboBox_scheme.Location = new System.Drawing.Point(12, 180);
            this.comboBox_scheme.Name = "comboBox_scheme";
            this.comboBox_scheme.Size = new System.Drawing.Size(250, 23);
            this.comboBox_scheme.TabIndex = 7;
            this.comboBox_scheme.Text = "W3C-DTF";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(339, 232);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(258, 232);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // w3cDtfControl_end
            // 
            this.w3cDtfControl_end.AutoSize = true;
            this.w3cDtfControl_end.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.w3cDtfControl_end.Location = new System.Drawing.Point(12, 131);
            this.w3cDtfControl_end.Name = "w3cDtfControl_end";
            this.w3cDtfControl_end.Size = new System.Drawing.Size(403, 24);
            this.w3cDtfControl_end.TabIndex = 5;
            this.w3cDtfControl_end.ValueString = "";
            // 
            // w3cDtfControl_start
            // 
            this.w3cDtfControl_start.AutoSize = true;
            this.w3cDtfControl_start.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.w3cDtfControl_start.Location = new System.Drawing.Point(12, 82);
            this.w3cDtfControl_start.Name = "w3cDtfControl_start";
            this.w3cDtfControl_start.Size = new System.Drawing.Size(403, 24);
            this.w3cDtfControl_start.TabIndex = 3;
            this.w3cDtfControl_start.ValueString = "";
            // 
            // DcPeriodDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 272);
            this.ControlBox = false;
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_scheme);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.w3cDtfControl_end);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.w3cDtfControl_start);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_name);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DcPeriodDialog";
            this.ShowInTaskbar = false;
            this.Text = "DC时间范围";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_name;
        private System.Windows.Forms.Label label2;
        private W3cDtfControl w3cDtfControl_start;
        private W3cDtfControl w3cDtfControl_end;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_scheme;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}