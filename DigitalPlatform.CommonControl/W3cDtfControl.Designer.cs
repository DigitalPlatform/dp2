namespace DigitalPlatform.CommonControl
{
    partial class W3cDtfControl
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
            this.maskedTextBox_date = new System.Windows.Forms.MaskedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.maskedTextBox_timeZone = new System.Windows.Forms.MaskedTextBox();
            this.label_eastWest = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // maskedTextBox_date
            // 
            this.maskedTextBox_date.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maskedTextBox_date.Location = new System.Drawing.Point(0, 0);
            this.maskedTextBox_date.Margin = new System.Windows.Forms.Padding(1);
            this.maskedTextBox_date.Mask = "0000年00月00日  90时00分00.00秒";
            this.maskedTextBox_date.Name = "maskedTextBox_date";
            this.maskedTextBox_date.Size = new System.Drawing.Size(254, 18);
            this.maskedTextBox_date.TabIndex = 1;
            this.maskedTextBox_date.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(261, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "时区:";
            // 
            // maskedTextBox_timeZone
            // 
            this.maskedTextBox_timeZone.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maskedTextBox_timeZone.Location = new System.Drawing.Point(328, 0);
            this.maskedTextBox_timeZone.Margin = new System.Windows.Forms.Padding(1);
            this.maskedTextBox_timeZone.Mask = "90时00分";
            this.maskedTextBox_timeZone.Name = "maskedTextBox_timeZone";
            this.maskedTextBox_timeZone.Size = new System.Drawing.Size(70, 18);
            this.maskedTextBox_timeZone.TabIndex = 3;
            // 
            // label_eastWest
            // 
            this.label_eastWest.AutoSize = true;
            this.label_eastWest.Location = new System.Drawing.Point(309, 0);
            this.label_eastWest.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_eastWest.Name = "label_eastWest";
            this.label_eastWest.Size = new System.Drawing.Size(15, 15);
            this.label_eastWest.TabIndex = 4;
            this.label_eastWest.Text = "+";
            this.label_eastWest.DoubleClick += new System.EventHandler(this.label_eastWest_DoubleClick);
            this.label_eastWest.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_eastWest_MouseUp);
            // 
            // W3cDtfControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.label_eastWest);
            this.Controls.Add(this.maskedTextBox_timeZone);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.maskedTextBox_date);
            this.Name = "W3cDtfControl";
            this.Size = new System.Drawing.Size(399, 19);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MaskedTextBox maskedTextBox_date;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox maskedTextBox_timeZone;
        private System.Windows.Forms.Label label_eastWest;
    }
}
