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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DateControl));
            this.maskedTextBox_date = new System.Windows.Forms.MaskedTextBox();
            this.button_findDate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // maskedTextBox_date
            // 
            this.maskedTextBox_date.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maskedTextBox_date.Location = new System.Drawing.Point(4, 4);
            this.maskedTextBox_date.Mask = "0000Äê90ÔÂ90ÈÕ";
            this.maskedTextBox_date.Name = "maskedTextBox_date";
            this.maskedTextBox_date.Size = new System.Drawing.Size(112, 18);
            this.maskedTextBox_date.TabIndex = 0;
            this.maskedTextBox_date.ValidatingType = typeof(System.DateTime);
            this.maskedTextBox_date.Validating += new System.ComponentModel.CancelEventHandler(this.maskedTextBox_date_Validating);
            this.maskedTextBox_date.TextChanged += new System.EventHandler(this.maskedTextBox_date_TextChanged);
            // 
            // button_findDate
            // 
            this.button_findDate.AutoSize = true;
            this.button_findDate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_findDate.FlatAppearance.BorderSize = 0;
            this.button_findDate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_findDate.Image = ((System.Drawing.Image)(resources.GetObject("button_findDate.Image")));
            this.button_findDate.Location = new System.Drawing.Point(113, -5);
            this.button_findDate.Name = "button_findDate";
            this.button_findDate.Size = new System.Drawing.Size(21, 27);
            this.button_findDate.TabIndex = 1;
            this.button_findDate.UseVisualStyleBackColor = true;
            this.button_findDate.Click += new System.EventHandler(this.button_findDate_Click);
            // 
            // DateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.button_findDate);
            this.Controls.Add(this.maskedTextBox_date);
            this.Name = "DateControl";
            this.Size = new System.Drawing.Size(136, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MaskedTextBox maskedTextBox_date;
        private System.Windows.Forms.Button button_findDate;
    }
}
