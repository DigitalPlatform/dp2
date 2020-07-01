namespace RfidCenter
{
    partial class LedDisplayDialog
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
            this.button_cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_ledName = new System.Windows.Forms.TextBox();
            this.textBox_x = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_y = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_style = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_fontSize = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_effect = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_moveSpeed = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_duration = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button_display = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBox_horzAlign = new System.Windows.Forms.ComboBox();
            this.comboBox_vertAlign = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(596, 645);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(120, 44);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(722, 645);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(120, 44);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "驱动板名:";
            // 
            // textBox_ledName
            // 
            this.textBox_ledName.Location = new System.Drawing.Point(144, 12);
            this.textBox_ledName.Name = "textBox_ledName";
            this.textBox_ledName.Size = new System.Drawing.Size(398, 31);
            this.textBox_ledName.TabIndex = 3;
            this.textBox_ledName.Text = "*";
            // 
            // textBox_x
            // 
            this.textBox_x.Location = new System.Drawing.Point(144, 49);
            this.textBox_x.Name = "textBox_x";
            this.textBox_x.Size = new System.Drawing.Size(133, 31);
            this.textBox_x.TabIndex = 5;
            this.textBox_x.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "起点 X:";
            // 
            // textBox_y
            // 
            this.textBox_y.Location = new System.Drawing.Point(144, 86);
            this.textBox_y.Name = "textBox_y";
            this.textBox_y.Size = new System.Drawing.Size(133, 31);
            this.textBox_y.TabIndex = 7;
            this.textBox_y.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "起点 Y:";
            // 
            // textBox_style
            // 
            this.textBox_style.Location = new System.Drawing.Point(144, 387);
            this.textBox_style.Name = "textBox_style";
            this.textBox_style.Size = new System.Drawing.Size(398, 31);
            this.textBox_style.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 390);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "扩展风格:";
            // 
            // textBox_fontSize
            // 
            this.textBox_fontSize.Location = new System.Drawing.Point(144, 143);
            this.textBox_fontSize.Name = "textBox_fontSize";
            this.textBox_fontSize.Size = new System.Drawing.Size(133, 31);
            this.textBox_fontSize.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 21);
            this.label5.TabIndex = 10;
            this.label5.Text = "字体尺寸:";
            // 
            // textBox_effect
            // 
            this.textBox_effect.Location = new System.Drawing.Point(144, 180);
            this.textBox_effect.Name = "textBox_effect";
            this.textBox_effect.Size = new System.Drawing.Size(558, 31);
            this.textBox_effect.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 183);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 21);
            this.label6.TabIndex = 12;
            this.label6.Text = "特效:";
            // 
            // textBox_moveSpeed
            // 
            this.textBox_moveSpeed.Location = new System.Drawing.Point(144, 217);
            this.textBox_moveSpeed.Name = "textBox_moveSpeed";
            this.textBox_moveSpeed.Size = new System.Drawing.Size(133, 31);
            this.textBox_moveSpeed.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 220);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 21);
            this.label7.TabIndex = 14;
            this.label7.Text = "移动速度:";
            // 
            // textBox_duration
            // 
            this.textBox_duration.Location = new System.Drawing.Point(144, 254);
            this.textBox_duration.Name = "textBox_duration";
            this.textBox_duration.Size = new System.Drawing.Size(133, 31);
            this.textBox_duration.TabIndex = 17;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 257);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 21);
            this.label8.TabIndex = 16;
            this.label8.Text = "停留时间:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(283, 220);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(162, 21);
            this.label9.TabIndex = 18;
            this.label9.Text = "0~99 (默认 70)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(283, 257);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(323, 21);
            this.label10.TabIndex = 19;
            this.label10.Text = "0.0~999.0 (默认 单元数*1.0)秒";
            // 
            // textBox_text
            // 
            this.textBox_text.AcceptsReturn = true;
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.Location = new System.Drawing.Point(144, 444);
            this.textBox_text.Multiline = true;
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_text.Size = new System.Drawing.Size(558, 195);
            this.textBox_text.TabIndex = 21;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 447);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 21);
            this.label11.TabIndex = 20;
            this.label11.Text = "文字:";
            // 
            // button_display
            // 
            this.button_display.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_display.Location = new System.Drawing.Point(709, 435);
            this.button_display.Name = "button_display";
            this.button_display.Size = new System.Drawing.Size(133, 44);
            this.button_display.TabIndex = 22;
            this.button_display.Text = "立即显示";
            this.button_display.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 298);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 21);
            this.label12.TabIndex = 23;
            this.label12.Text = "水平对齐:";
            // 
            // comboBox_horzAlign
            // 
            this.comboBox_horzAlign.FormattingEnabled = true;
            this.comboBox_horzAlign.Items.AddRange(new object[] {
            "left",
            "center",
            "right"});
            this.comboBox_horzAlign.Location = new System.Drawing.Point(144, 295);
            this.comboBox_horzAlign.Name = "comboBox_horzAlign";
            this.comboBox_horzAlign.Size = new System.Drawing.Size(200, 29);
            this.comboBox_horzAlign.TabIndex = 24;
            // 
            // comboBox_vertAlign
            // 
            this.comboBox_vertAlign.FormattingEnabled = true;
            this.comboBox_vertAlign.Items.AddRange(new object[] {
            "top",
            "center",
            "bottom"});
            this.comboBox_vertAlign.Location = new System.Drawing.Point(144, 330);
            this.comboBox_vertAlign.Name = "comboBox_vertAlign";
            this.comboBox_vertAlign.Size = new System.Drawing.Size(200, 29);
            this.comboBox_vertAlign.TabIndex = 26;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 333);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(105, 21);
            this.label13.TabIndex = 25;
            this.label13.Text = "垂直对齐:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(708, 183);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(140, 21);
            this.label14.TabIndex = 27;
            this.label14.Text = "(默认 still)";
            // 
            // LedDisplayDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 701);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.comboBox_vertAlign);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.comboBox_horzAlign);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.button_display);
            this.Controls.Add(this.textBox_text);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox_duration);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox_moveSpeed);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_effect);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_fontSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_style);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_y);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_x);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_ledName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "LedDisplayDialog";
            this.Text = "LedDisplayDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_ledName;
        private System.Windows.Forms.TextBox textBox_x;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_y;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_style;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_fontSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_effect;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_moveSpeed;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_duration;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_text;
        private System.Windows.Forms.Label label11;
        public System.Windows.Forms.Button button_display;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBox_horzAlign;
        private System.Windows.Forms.ComboBox comboBox_vertAlign;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
    }
}