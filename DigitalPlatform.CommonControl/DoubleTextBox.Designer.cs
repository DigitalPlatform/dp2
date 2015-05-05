namespace DigitalPlatform.CommonControl
{
    partial class DoubleTextBox
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
            this.TextBox = new System.Windows.Forms.TextBox();
            this.SecondTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TextBox
            // 
            this.TextBox.Location = new System.Drawing.Point(0, 0);
            this.TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(140, 21);
            this.TextBox.TabIndex = 0;
            this.TextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // SecondTextBox
            // 
            this.SecondTextBox.Location = new System.Drawing.Point(0, 25);
            this.SecondTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SecondTextBox.Name = "SecondTextBox";
            this.SecondTextBox.Size = new System.Drawing.Size(140, 21);
            this.SecondTextBox.TabIndex = 1;
            this.SecondTextBox.TextChanged += new System.EventHandler(this.SecondTextBox_TextChanged);
            // 
            // DoubleTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SecondTextBox);
            this.Controls.Add(this.TextBox);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DoubleTextBox";
            this.Size = new System.Drawing.Size(140, 47);
            this.SizeChanged += new System.EventHandler(this.DoubleTextBox_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox TextBox;
        public System.Windows.Forms.TextBox SecondTextBox;
    }
}
