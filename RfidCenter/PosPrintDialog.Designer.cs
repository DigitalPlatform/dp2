namespace RfidCenter
{
    partial class PosPrintDialog
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
            this.textBox_style = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button_print = new System.Windows.Forms.Button();
            this.comboBox_action = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(581, 475);
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
            this.button_cancel.Location = new System.Drawing.Point(707, 475);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(120, 44);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // textBox_style
            // 
            this.textBox_style.Location = new System.Drawing.Point(144, 74);
            this.textBox_style.Name = "textBox_style";
            this.textBox_style.Size = new System.Drawing.Size(398, 31);
            this.textBox_style.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "扩展风格:";
            // 
            // textBox_text
            // 
            this.textBox_text.AcceptsReturn = true;
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.Location = new System.Drawing.Point(144, 131);
            this.textBox_text.Multiline = true;
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_text.Size = new System.Drawing.Size(543, 311);
            this.textBox_text.TabIndex = 21;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 134);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 21);
            this.label11.TabIndex = 20;
            this.label11.Text = "文字:";
            // 
            // button_print
            // 
            this.button_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_print.Location = new System.Drawing.Point(694, 122);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(133, 44);
            this.button_print.TabIndex = 22;
            this.button_print.Text = "立即打印";
            this.button_print.UseVisualStyleBackColor = true;
            // 
            // comboBox_action
            // 
            this.comboBox_action.FormattingEnabled = true;
            this.comboBox_action.Items.AddRange(new object[] {
            "feed",
            "cut",
            "cuthalf",
            "printline",
            "print",
            "getstatus",
            "init"});
            this.comboBox_action.Location = new System.Drawing.Point(144, 17);
            this.comboBox_action.Name = "comboBox_action";
            this.comboBox_action.Size = new System.Drawing.Size(200, 29);
            this.comboBox_action.TabIndex = 26;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 20);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(63, 21);
            this.label13.TabIndex = 25;
            this.label13.Text = "动作:";
            // 
            // PosPrintDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 531);
            this.Controls.Add(this.comboBox_action);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.button_print);
            this.Controls.Add(this.textBox_text);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.textBox_style);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "PosPrintDialog";
            this.Text = "测试小票打印";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.TextBox textBox_style;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_text;
        private System.Windows.Forms.Label label11;
        public System.Windows.Forms.Button button_print;
        private System.Windows.Forms.ComboBox comboBox_action;
        private System.Windows.Forms.Label label13;
    }
}