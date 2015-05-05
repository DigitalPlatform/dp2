namespace dp2Circulation
{
    partial class LabelLineFormatDialog
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
            this.comboBox_align = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_fontString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_getFont = new System.Windows.Forms.Button();
            this.button_setBarcodeFont = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_startX = new System.Windows.Forms.TextBox();
            this.textBox_startY = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown_offsetX = new DigitalPlatform.CommonControl.UniverseNumericUpDown();
            this.numericUpDown_offsetY = new DigitalPlatform.CommonControl.UniverseNumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_text = new System.Windows.Forms.TabPage();
            this.tabPage_position = new System.Windows.Forms.TabPage();
            this.tabPage_color = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_foreColor = new System.Windows.Forms.TextBox();
            this.button_setForeColor = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_backColor = new System.Windows.Forms.TextBox();
            this.button_setBackColor = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetY)).BeginInit();
            this.tabControl_main.SuspendLayout();
            this.tabPage_text.SuspendLayout();
            this.tabPage_position.SuspendLayout();
            this.tabPage_color.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBox_align
            // 
            this.comboBox_align.FormattingEnabled = true;
            this.comboBox_align.Items.AddRange(new object[] {
            "left",
            "right",
            "center"});
            this.comboBox_align.Location = new System.Drawing.Point(110, 64);
            this.comboBox_align.Name = "comboBox_align";
            this.comboBox_align.Size = new System.Drawing.Size(156, 20);
            this.comboBox_align.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "对齐方式(&A):";
            // 
            // textBox_fontString
            // 
            this.textBox_fontString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_fontString.Location = new System.Drawing.Point(110, 11);
            this.textBox_fontString.Name = "textBox_fontString";
            this.textBox_fontString.Size = new System.Drawing.Size(168, 21);
            this.textBox_fontString.TabIndex = 1;
            this.textBox_fontString.TextChanged += new System.EventHandler(this.textBox_fontString_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "字体(&F):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(229, 255);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(148, 255);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_getFont
            // 
            this.button_getFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getFont.Location = new System.Drawing.Point(233, 35);
            this.button_getFont.Name = "button_getFont";
            this.button_getFont.Size = new System.Drawing.Size(45, 18);
            this.button_getFont.TabIndex = 3;
            this.button_getFont.Text = "...";
            this.button_getFont.UseVisualStyleBackColor = true;
            this.button_getFont.Click += new System.EventHandler(this.button_getFont_Click);
            // 
            // button_setBarcodeFont
            // 
            this.button_setBarcodeFont.Location = new System.Drawing.Point(110, 33);
            this.button_setBarcodeFont.Name = "button_setBarcodeFont";
            this.button_setBarcodeFont.Size = new System.Drawing.Size(105, 23);
            this.button_setBarcodeFont.TabIndex = 2;
            this.button_setBarcodeFont.Text = "设为条码字体";
            this.button_setBarcodeFont.UseVisualStyleBackColor = true;
            this.button_setBarcodeFont.Click += new System.EventHandler(this.button_setBarcodeFont_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "起始位置 X";
            // 
            // textBox_startX
            // 
            this.textBox_startX.Location = new System.Drawing.Point(110, 16);
            this.textBox_startX.Name = "textBox_startX";
            this.textBox_startX.Size = new System.Drawing.Size(100, 21);
            this.textBox_startX.TabIndex = 9;
            // 
            // textBox_startY
            // 
            this.textBox_startY.Location = new System.Drawing.Point(110, 43);
            this.textBox_startY.Name = "textBox_startY";
            this.textBox_startY.Size = new System.Drawing.Size(100, 21);
            this.textBox_startY.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "起始位置 Y";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "偏移 X";
            // 
            // numericUpDown_offsetX
            // 
            this.numericUpDown_offsetX.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.numericUpDown_offsetX.Location = new System.Drawing.Point(110, 80);
            this.numericUpDown_offsetX.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_offsetX.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDown_offsetX.Name = "numericUpDown_offsetX";
            this.numericUpDown_offsetX.Size = new System.Drawing.Size(100, 21);
            this.numericUpDown_offsetX.TabIndex = 13;
            this.numericUpDown_offsetX.UniverseValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDown_offsetY
            // 
            this.numericUpDown_offsetY.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.numericUpDown_offsetY.Location = new System.Drawing.Point(110, 107);
            this.numericUpDown_offsetY.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_offsetY.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numericUpDown_offsetY.Name = "numericUpDown_offsetY";
            this.numericUpDown_offsetY.Size = new System.Drawing.Size(100, 21);
            this.numericUpDown_offsetY.TabIndex = 15;
            this.numericUpDown_offsetY.UniverseValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 109);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "偏移 Y";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_text);
            this.tabControl_main.Controls.Add(this.tabPage_position);
            this.tabControl_main.Controls.Add(this.tabPage_color);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(292, 237);
            this.tabControl_main.TabIndex = 16;
            // 
            // tabPage_text
            // 
            this.tabPage_text.Controls.Add(this.label1);
            this.tabPage_text.Controls.Add(this.textBox_fontString);
            this.tabPage_text.Controls.Add(this.label2);
            this.tabPage_text.Controls.Add(this.comboBox_align);
            this.tabPage_text.Controls.Add(this.button_getFont);
            this.tabPage_text.Controls.Add(this.button_setBarcodeFont);
            this.tabPage_text.Location = new System.Drawing.Point(4, 22);
            this.tabPage_text.Name = "tabPage_text";
            this.tabPage_text.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_text.Size = new System.Drawing.Size(284, 211);
            this.tabPage_text.TabIndex = 0;
            this.tabPage_text.Text = "文字";
            this.tabPage_text.UseVisualStyleBackColor = true;
            // 
            // tabPage_position
            // 
            this.tabPage_position.Controls.Add(this.textBox_startX);
            this.tabPage_position.Controls.Add(this.numericUpDown_offsetY);
            this.tabPage_position.Controls.Add(this.label3);
            this.tabPage_position.Controls.Add(this.label6);
            this.tabPage_position.Controls.Add(this.label4);
            this.tabPage_position.Controls.Add(this.numericUpDown_offsetX);
            this.tabPage_position.Controls.Add(this.textBox_startY);
            this.tabPage_position.Controls.Add(this.label5);
            this.tabPage_position.Location = new System.Drawing.Point(4, 22);
            this.tabPage_position.Name = "tabPage_position";
            this.tabPage_position.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_position.Size = new System.Drawing.Size(257, 211);
            this.tabPage_position.TabIndex = 1;
            this.tabPage_position.Text = "位置";
            this.tabPage_position.UseVisualStyleBackColor = true;
            // 
            // tabPage_color
            // 
            this.tabPage_color.Controls.Add(this.label8);
            this.tabPage_color.Controls.Add(this.textBox_backColor);
            this.tabPage_color.Controls.Add(this.button_setBackColor);
            this.tabPage_color.Controls.Add(this.label7);
            this.tabPage_color.Controls.Add(this.textBox_foreColor);
            this.tabPage_color.Controls.Add(this.button_setForeColor);
            this.tabPage_color.Location = new System.Drawing.Point(4, 22);
            this.tabPage_color.Name = "tabPage_color";
            this.tabPage_color.Size = new System.Drawing.Size(284, 211);
            this.tabPage_color.TabIndex = 2;
            this.tabPage_color.Text = "颜色";
            this.tabPage_color.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 4;
            this.label7.Text = "前景色(&F):";
            // 
            // textBox_foreColor
            // 
            this.textBox_foreColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_foreColor.Location = new System.Drawing.Point(113, 17);
            this.textBox_foreColor.Name = "textBox_foreColor";
            this.textBox_foreColor.Size = new System.Drawing.Size(168, 21);
            this.textBox_foreColor.TabIndex = 5;
            // 
            // button_setForeColor
            // 
            this.button_setForeColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setForeColor.Location = new System.Drawing.Point(236, 41);
            this.button_setForeColor.Name = "button_setForeColor";
            this.button_setForeColor.Size = new System.Drawing.Size(45, 18);
            this.button_setForeColor.TabIndex = 6;
            this.button_setForeColor.Text = "...";
            this.button_setForeColor.UseVisualStyleBackColor = true;
            this.button_setForeColor.Click += new System.EventHandler(this.button_setForeColor_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 78);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 7;
            this.label8.Text = "背景色(&B):";
            // 
            // textBox_backColor
            // 
            this.textBox_backColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_backColor.Location = new System.Drawing.Point(113, 75);
            this.textBox_backColor.Name = "textBox_backColor";
            this.textBox_backColor.Size = new System.Drawing.Size(168, 21);
            this.textBox_backColor.TabIndex = 8;
            // 
            // button_setBackColor
            // 
            this.button_setBackColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setBackColor.Location = new System.Drawing.Point(236, 99);
            this.button_setBackColor.Name = "button_setBackColor";
            this.button_setBackColor.Size = new System.Drawing.Size(45, 18);
            this.button_setBackColor.TabIndex = 9;
            this.button_setBackColor.Text = "...";
            this.button_setBackColor.UseVisualStyleBackColor = true;
            this.button_setBackColor.Click += new System.EventHandler(this.button_setBackColor_Click);
            // 
            // LabelLineFormatDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(316, 290);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "LabelLineFormatDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "行格式";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetY)).EndInit();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_text.ResumeLayout(false);
            this.tabPage_text.PerformLayout();
            this.tabPage_position.ResumeLayout(false);
            this.tabPage_position.PerformLayout();
            this.tabPage_color.ResumeLayout(false);
            this.tabPage_color.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_align;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_fontString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_getFont;
        private System.Windows.Forms.Button button_setBarcodeFont;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_startX;
        private System.Windows.Forms.TextBox textBox_startY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private DigitalPlatform.CommonControl.UniverseNumericUpDown numericUpDown_offsetX;
        private DigitalPlatform.CommonControl.UniverseNumericUpDown numericUpDown_offsetY;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_text;
        private System.Windows.Forms.TabPage tabPage_position;
        private System.Windows.Forms.TabPage tabPage_color;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_backColor;
        private System.Windows.Forms.Button button_setBackColor;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_foreColor;
        private System.Windows.Forms.Button button_setForeColor;
    }
}