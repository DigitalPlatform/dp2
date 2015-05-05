namespace DigitalPlatform.CommonControl
{
    partial class DoubleComboBox
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
            this.ComboBox = new System.Windows.Forms.ComboBox();
            this.TextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ComboBox
            // 
            this.ComboBox.FormattingEnabled = true;
            this.ComboBox.Location = new System.Drawing.Point(0, 0);
            this.ComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.ComboBox.Name = "ComboBox";
            this.ComboBox.Size = new System.Drawing.Size(146, 20);
            this.ComboBox.TabIndex = 0;
            this.ComboBox.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            this.ComboBox.SizeChanged += new System.EventHandler(this.ComboBox_SizeChanged);
            this.ComboBox.TextChanged += new System.EventHandler(this.ComboBox_TextChanged);
            // 
            // TextBox
            // 
            this.TextBox.Location = new System.Drawing.Point(0, 20);
            this.TextBox.Margin = new System.Windows.Forms.Padding(0);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(146, 21);
            this.TextBox.TabIndex = 1;
            this.TextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // DoubleComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.TextBox);
            this.Controls.Add(this.ComboBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DoubleComboBox";
            this.Size = new System.Drawing.Size(145, 41);
            this.SizeChanged += new System.EventHandler(this.DoubleComboBox_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox TextBox;
        public System.Windows.Forms.ComboBox ComboBox;
    }
}
