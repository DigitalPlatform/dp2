namespace dp2Circulation.Print
{
    partial class TextAndHtmlControl
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
            this.DisposeFreeControls();

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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_text = new System.Windows.Forms.TabPage();
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.tabPage_html = new System.Windows.Forms.TabPage();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.tabControl1.SuspendLayout();
            this.tabPage_text.SuspendLayout();
            this.tabPage_html.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabPage_text);
            this.tabControl1.Controls.Add(this.tabPage_html);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(279, 190);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_text
            // 
            this.tabPage_text.Controls.Add(this.textBox_text);
            this.tabPage_text.Location = new System.Drawing.Point(4, 25);
            this.tabPage_text.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_text.Name = "tabPage_text";
            this.tabPage_text.Size = new System.Drawing.Size(271, 161);
            this.tabPage_text.TabIndex = 0;
            this.tabPage_text.Text = "纯文本";
            this.tabPage_text.UseVisualStyleBackColor = true;
            // 
            // textBox_text
            // 
            this.textBox_text.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_text.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_text.Location = new System.Drawing.Point(0, 0);
            this.textBox_text.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_text.MaxLength = 0;
            this.textBox_text.Multiline = true;
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_text.Size = new System.Drawing.Size(271, 161);
            this.textBox_text.TabIndex = 0;
            // 
            // tabPage_html
            // 
            this.tabPage_html.Controls.Add(this.webBrowser1);
            this.tabPage_html.Location = new System.Drawing.Point(4, 25);
            this.tabPage_html.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_html.Name = "tabPage_html";
            this.tabPage_html.Size = new System.Drawing.Size(271, 161);
            this.tabPage_html.TabIndex = 1;
            this.tabPage_html.Text = "HTML效果";
            this.tabPage_html.UseVisualStyleBackColor = true;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(271, 161);
            this.webBrowser1.TabIndex = 0;
            // 
            // TextAndHtmlControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "TextAndHtmlControl";
            this.Size = new System.Drawing.Size(279, 190);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_text.ResumeLayout(false);
            this.tabPage_text.PerformLayout();
            this.tabPage_html.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_text;
        private System.Windows.Forms.TabPage tabPage_html;
        private System.Windows.Forms.TextBox textBox_text;
        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}
