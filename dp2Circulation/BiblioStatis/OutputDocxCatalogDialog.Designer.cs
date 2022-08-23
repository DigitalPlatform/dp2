namespace dp2Circulation
{
    partial class OutputDocxCatalogDialog
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
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.numericUpDown_pageNumberStart = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_biblioNoStart = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_noFontName = new System.Windows.Forms.TextBox();
            this.textBox_noFontSize = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_barcodeFontSize = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_barcodeFontName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_contentFontSize = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_contentFontName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_accessNoFontSize = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_accessNoFontName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_pageNumberStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_biblioNoStart)).BeginInit();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(546, 509);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(118, 37);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(670, 509);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(118, 37);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // numericUpDown_pageNumberStart
            // 
            this.numericUpDown_pageNumberStart.Location = new System.Drawing.Point(197, 96);
            this.numericUpDown_pageNumberStart.Name = "numericUpDown_pageNumberStart";
            this.numericUpDown_pageNumberStart.Size = new System.Drawing.Size(177, 31);
            this.numericUpDown_pageNumberStart.TabIndex = 2;
            this.numericUpDown_pageNumberStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_pageNumberStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "起始页码(&P):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "书目起始序号(&I):";
            // 
            // numericUpDown_biblioNoStart
            // 
            this.numericUpDown_biblioNoStart.Location = new System.Drawing.Point(197, 50);
            this.numericUpDown_biblioNoStart.Name = "numericUpDown_biblioNoStart";
            this.numericUpDown_biblioNoStart.Size = new System.Drawing.Size(177, 31);
            this.numericUpDown_biblioNoStart.TabIndex = 5;
            this.numericUpDown_biblioNoStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_biblioNoStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "序号字体:";
            // 
            // textBox_noFontName
            // 
            this.textBox_noFontName.Location = new System.Drawing.Point(197, 163);
            this.textBox_noFontName.Name = "textBox_noFontName";
            this.textBox_noFontName.Size = new System.Drawing.Size(399, 31);
            this.textBox_noFontName.TabIndex = 7;
            // 
            // textBox_noFontSize
            // 
            this.textBox_noFontSize.Location = new System.Drawing.Point(197, 200);
            this.textBox_noFontSize.Name = "textBox_noFontSize";
            this.textBox_noFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_noFontSize.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 203);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "序号字号:";
            // 
            // textBox_barcodeFontSize
            // 
            this.textBox_barcodeFontSize.Location = new System.Drawing.Point(197, 274);
            this.textBox_barcodeFontSize.Name = "textBox_barcodeFontSize";
            this.textBox_barcodeFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_barcodeFontSize.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 277);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(147, 21);
            this.label5.TabIndex = 12;
            this.label5.Text = "册条码号字号:";
            // 
            // textBox_barcodeFontName
            // 
            this.textBox_barcodeFontName.Location = new System.Drawing.Point(197, 237);
            this.textBox_barcodeFontName.Name = "textBox_barcodeFontName";
            this.textBox_barcodeFontName.Size = new System.Drawing.Size(399, 31);
            this.textBox_barcodeFontName.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(147, 21);
            this.label6.TabIndex = 10;
            this.label6.Text = "册条码号字体:";
            // 
            // textBox_contentFontSize
            // 
            this.textBox_contentFontSize.Location = new System.Drawing.Point(197, 348);
            this.textBox_contentFontSize.Name = "textBox_contentFontSize";
            this.textBox_contentFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_contentFontSize.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 351);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 21);
            this.label7.TabIndex = 16;
            this.label7.Text = "正文字号:";
            // 
            // textBox_contentFontName
            // 
            this.textBox_contentFontName.Location = new System.Drawing.Point(197, 311);
            this.textBox_contentFontName.Name = "textBox_contentFontName";
            this.textBox_contentFontName.Size = new System.Drawing.Size(399, 31);
            this.textBox_contentFontName.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 314);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 21);
            this.label8.TabIndex = 14;
            this.label8.Text = "正文字体:";
            // 
            // textBox_accessNoFontSize
            // 
            this.textBox_accessNoFontSize.Location = new System.Drawing.Point(197, 422);
            this.textBox_accessNoFontSize.Name = "textBox_accessNoFontSize";
            this.textBox_accessNoFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_accessNoFontSize.TabIndex = 21;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 425);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(126, 21);
            this.label9.TabIndex = 20;
            this.label9.Text = "索取号字号:";
            // 
            // textBox_accessNoFontName
            // 
            this.textBox_accessNoFontName.Location = new System.Drawing.Point(197, 385);
            this.textBox_accessNoFontName.Name = "textBox_accessNoFontName";
            this.textBox_accessNoFontName.Size = new System.Drawing.Size(399, 31);
            this.textBox_accessNoFontName.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 388);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(126, 21);
            this.label10.TabIndex = 18;
            this.label10.Text = "索取号字体:";
            // 
            // OutputDocxCatalogDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(800, 558);
            this.Controls.Add(this.textBox_accessNoFontSize);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox_accessNoFontName);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textBox_contentFontSize);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_contentFontName);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox_barcodeFontSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_barcodeFontName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_noFontSize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_noFontName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown_biblioNoStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_pageNumberStart);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "OutputDocxCatalogDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "输出 docx 书本式目录";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_pageNumberStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_biblioNoStart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.NumericUpDown numericUpDown_pageNumberStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_biblioNoStart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_noFontName;
        private System.Windows.Forms.TextBox textBox_noFontSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_barcodeFontSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_barcodeFontName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_contentFontSize;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_contentFontName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_accessNoFontSize;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_accessNoFontName;
        private System.Windows.Forms.Label label10;
    }
}