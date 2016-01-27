namespace dp2Circulation
{
    partial class HtmlPrintForm
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

            this.eventPrintComplete.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlPrintForm));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_print = new System.Windows.Forms.Button();
            this.button_prevPage = new System.Windows.Forms.Button();
            this.button_nextPage = new System.Windows.Forms.Button();
            this.label_pageInfo = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_printRange = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_copies = new System.Windows.Forms.TextBox();
            this.button_firstPage = new System.Windows.Forms.Button();
            this.button_lastPage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(9, 8);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(393, 217);
            this.webBrowser1.TabIndex = 0;
            // 
            // button_print
            // 
            this.button_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_print.Location = new System.Drawing.Point(9, 261);
            this.button_print.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(56, 22);
            this.button_print.TabIndex = 6;
            this.button_print.Text = "´òÓ¡(&P)";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // button_prevPage
            // 
            this.button_prevPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_prevPage.Location = new System.Drawing.Point(321, 230);
            this.button_prevPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_prevPage.Name = "button_prevPage";
            this.button_prevPage.Size = new System.Drawing.Size(23, 22);
            this.button_prevPage.TabIndex = 3;
            this.button_prevPage.Text = "<";
            this.button_prevPage.UseVisualStyleBackColor = true;
            this.button_prevPage.Click += new System.EventHandler(this.button_prevPage_Click);
            // 
            // button_nextPage
            // 
            this.button_nextPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_nextPage.Location = new System.Drawing.Point(344, 230);
            this.button_nextPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_nextPage.Name = "button_nextPage";
            this.button_nextPage.Size = new System.Drawing.Size(23, 22);
            this.button_nextPage.TabIndex = 4;
            this.button_nextPage.Text = ">";
            this.button_nextPage.UseVisualStyleBackColor = true;
            this.button_nextPage.Click += new System.EventHandler(this.button_nextPage_Click);
            // 
            // label_pageInfo
            // 
            this.label_pageInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_pageInfo.Location = new System.Drawing.Point(7, 230);
            this.label_pageInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_pageInfo.Name = "label_pageInfo";
            this.label_pageInfo.Size = new System.Drawing.Size(275, 18);
            this.label_pageInfo.TabIndex = 1;
            this.label_pageInfo.Text = "label1";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(70, 266);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Ò³Âë·¶Î§(&R):";
            // 
            // textBox_printRange
            // 
            this.textBox_printRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_printRange.Location = new System.Drawing.Point(150, 264);
            this.textBox_printRange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_printRange.Name = "textBox_printRange";
            this.textBox_printRange.Size = new System.Drawing.Size(148, 21);
            this.textBox_printRange.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(302, 266);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "·ÝÊý(&C):";
            // 
            // textBox_copies
            // 
            this.textBox_copies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_copies.Location = new System.Drawing.Point(358, 264);
            this.textBox_copies.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_copies.Name = "textBox_copies";
            this.textBox_copies.Size = new System.Drawing.Size(45, 21);
            this.textBox_copies.TabIndex = 10;
            this.textBox_copies.Text = "1";
            // 
            // button_firstPage
            // 
            this.button_firstPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_firstPage.Location = new System.Drawing.Point(286, 230);
            this.button_firstPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_firstPage.Name = "button_firstPage";
            this.button_firstPage.Size = new System.Drawing.Size(26, 22);
            this.button_firstPage.TabIndex = 2;
            this.button_firstPage.Text = "|<";
            this.button_firstPage.UseVisualStyleBackColor = true;
            this.button_firstPage.Click += new System.EventHandler(this.button_firstPage_Click);
            // 
            // button_lastPage
            // 
            this.button_lastPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_lastPage.Location = new System.Drawing.Point(376, 230);
            this.button_lastPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_lastPage.Name = "button_lastPage";
            this.button_lastPage.Size = new System.Drawing.Size(26, 22);
            this.button_lastPage.TabIndex = 5;
            this.button_lastPage.Text = ">|";
            this.button_lastPage.UseVisualStyleBackColor = true;
            this.button_lastPage.Click += new System.EventHandler(this.button_lastPage_Click);
            // 
            // HtmlPrintForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 290);
            this.Controls.Add(this.button_lastPage);
            this.Controls.Add(this.button_firstPage);
            this.Controls.Add(this.textBox_copies);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_printRange);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_pageInfo);
            this.Controls.Add(this.button_nextPage);
            this.Controls.Add(this.button_prevPage);
            this.Controls.Add(this.button_print);
            this.Controls.Add(this.webBrowser1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "HtmlPrintForm";
            this.ShowInTaskbar = false;
            this.Text = "HtmlPrintForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HtmlPrintForm_FormClosed);
            this.Load += new System.EventHandler(this.HtmlPrintForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.Button button_prevPage;
        private System.Windows.Forms.Button button_nextPage;
        private System.Windows.Forms.Label label_pageInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_printRange;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_copies;
        private System.Windows.Forms.Button button_firstPage;
        private System.Windows.Forms.Button button_lastPage;
    }
}