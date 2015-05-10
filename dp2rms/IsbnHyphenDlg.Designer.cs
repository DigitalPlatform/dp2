namespace dp2rms
{
    partial class IsbnHyphenDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IsbnHyphenDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_isbn = new System.Windows.Forms.TextBox();
            this.button_addHyphen = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "ISBN";
            // 
            // textBox_isbn
            // 
            this.textBox_isbn.Location = new System.Drawing.Point(15, 29);
            this.textBox_isbn.Name = "textBox_isbn";
            this.textBox_isbn.Size = new System.Drawing.Size(265, 21);
            this.textBox_isbn.TabIndex = 1;
            // 
            // button_addHyphen
            // 
            this.button_addHyphen.Location = new System.Drawing.Point(178, 57);
            this.button_addHyphen.Name = "button_addHyphen";
            this.button_addHyphen.Size = new System.Drawing.Size(102, 23);
            this.button_addHyphen.TabIndex = 2;
            this.button_addHyphen.Text = "加入横杠(&A)";
            this.button_addHyphen.UseVisualStyleBackColor = true;
            this.button_addHyphen.Click += new System.EventHandler(this.button_addHyphen_Click);
            // 
            // IsbnHyphenDlg
            // 
            this.AcceptButton = this.button_addHyphen;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 100);
            this.Controls.Add(this.button_addHyphen);
            this.Controls.Add(this.textBox_isbn);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IsbnHyphenDlg";
            this.ShowInTaskbar = false;
            this.Text = "给ISBN加横杠";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_isbn;
        private System.Windows.Forms.Button button_addHyphen;
    }
}