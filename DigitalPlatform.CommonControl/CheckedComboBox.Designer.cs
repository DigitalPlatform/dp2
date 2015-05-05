namespace DigitalPlatform.CommonControl
{
    partial class CheckedComboBox
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
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox_text
            // 
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_text.Location = new System.Drawing.Point(2, 3);
            this.textBox_text.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.Size = new System.Drawing.Size(166, 14);
            this.textBox_text.TabIndex = 0;
            this.textBox_text.TextChanged += new System.EventHandler(this.textBox_text_TextChanged);
            this.textBox_text.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_text_KeyDown);
            this.textBox_text.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_text_KeyPress);
            // 
            // CheckedComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.textBox_text);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CheckedComboBox";
            this.Size = new System.Drawing.Size(184, 22);
            this.FontChanged += new System.EventHandler(this.CheckedComboBox_FontChanged);
            this.PaddingChanged += new System.EventHandler(this.CheckedComboBox_PaddingChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CheckedComboBox_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CheckedComboBox_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CheckedComboBox_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_text;
    }
}
