namespace DigitalPlatform.CommonControl
{
    partial class DateControl
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
            this.SuspendLayout();
            // 
            // maskedTextBox_date
            // 
            this.maskedTextBox_date.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maskedTextBox_date.Location = new System.Drawing.Point(3, 4);
            this.maskedTextBox_date.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBox_date.Mask = "0000Äê90ÔÂ90ÈÕ";
            this.maskedTextBox_date.Name = "maskedTextBox_date";
            this.maskedTextBox_date.Size = new System.Drawing.Size(84, 14);
            this.maskedTextBox_date.TabIndex = 0;
            this.maskedTextBox_date.ValidatingType = typeof(System.DateTime);
            this.maskedTextBox_date.TextChanged += new System.EventHandler(this.maskedTextBox_date_TextChanged);
            this.maskedTextBox_date.Validating += new System.ComponentModel.CancelEventHandler(this.maskedTextBox_date_Validating);
            // 
            // DateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.maskedTextBox_date);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DateControl";
            this.Size = new System.Drawing.Size(102, 22);
            this.FontChanged += new System.EventHandler(this.DateControl_FontChanged);
            this.PaddingChanged += new System.EventHandler(this.DateControl_PaddingChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DateControl_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DateControl_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MaskedTextBox maskedTextBox_date;
    }
}
