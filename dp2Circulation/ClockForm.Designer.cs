namespace dp2Circulation
{
    partial class ClockForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClockForm));
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button_set = new System.Windows.Forms.Button();
            this.button_get = new System.Windows.Forms.Button();
            this.button_reset = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_time = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_autoGetServerTime = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_localTime = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(14, 20);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker1.TabIndex = 0;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // button_set
            // 
            this.button_set.Location = new System.Drawing.Point(14, 89);
            this.button_set.Name = "button_set";
            this.button_set.Size = new System.Drawing.Size(113, 23);
            this.button_set.TabIndex = 1;
            this.button_set.Text = "设置(&S)";
            this.button_set.UseVisualStyleBackColor = true;
            this.button_set.Click += new System.EventHandler(this.button_set_Click);
            // 
            // button_get
            // 
            this.button_get.Location = new System.Drawing.Point(14, 46);
            this.button_get.Name = "button_get";
            this.button_get.Size = new System.Drawing.Size(113, 23);
            this.button_get.TabIndex = 2;
            this.button_get.Text = "重新获得(&G)";
            this.button_get.UseVisualStyleBackColor = true;
            this.button_get.Click += new System.EventHandler(this.button_get_Click);
            // 
            // button_reset
            // 
            this.button_reset.Location = new System.Drawing.Point(14, 118);
            this.button_reset.Name = "button_reset";
            this.button_reset.Size = new System.Drawing.Size(113, 23);
            this.button_reset.TabIndex = 3;
            this.button_reset.Text = "复原(&R)";
            this.button_reset.UseVisualStyleBackColor = true;
            this.button_reset.Click += new System.EventHandler(this.button_reset_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 179);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "上述时间值的RFC1123格式:";
            // 
            // textBox_time
            // 
            this.textBox_time.Location = new System.Drawing.Point(14, 194);
            this.textBox_time.Name = "textBox_time";
            this.textBox_time.ReadOnly = true;
            this.textBox_time.Size = new System.Drawing.Size(265, 21);
            this.textBox_time.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBox_autoGetServerTime);
            this.groupBox1.Controls.Add(this.textBox_time);
            this.groupBox1.Controls.Add(this.dateTimePicker1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button_get);
            this.groupBox1.Controls.Add(this.button_reset);
            this.groupBox1.Controls.Add(this.button_set);
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(302, 228);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 服务器时钟";
            // 
            // checkBox_autoGetServerTime
            // 
            this.checkBox_autoGetServerTime.AutoSize = true;
            this.checkBox_autoGetServerTime.Location = new System.Drawing.Point(139, 54);
            this.checkBox_autoGetServerTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_autoGetServerTime.Name = "checkBox_autoGetServerTime";
            this.checkBox_autoGetServerTime.Size = new System.Drawing.Size(102, 16);
            this.checkBox_autoGetServerTime.TabIndex = 6;
            this.checkBox_autoGetServerTime.Text = "不停地获得(&A)";
            this.checkBox_autoGetServerTime.UseVisualStyleBackColor = true;
            this.checkBox_autoGetServerTime.CheckedChanged += new System.EventHandler(this.checkBox_autoGet_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 250);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "本地时钟(&L):";
            // 
            // textBox_localTime
            // 
            this.textBox_localTime.Location = new System.Drawing.Point(10, 265);
            this.textBox_localTime.Name = "textBox_localTime";
            this.textBox_localTime.ReadOnly = true;
            this.textBox_localTime.Size = new System.Drawing.Size(265, 21);
            this.textBox_localTime.TabIndex = 10;
            // 
            // ClockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 299);
            this.Controls.Add(this.textBox_localTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ClockForm";
            this.ShowInTaskbar = false;
            this.Text = "时钟";
            this.Activated += new System.EventHandler(this.ClockForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClockForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClockForm_FormClosed);
            this.Load += new System.EventHandler(this.ClockForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button button_set;
        private System.Windows.Forms.Button button_get;
        private System.Windows.Forms.Button button_reset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_time;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_autoGetServerTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_localTime;
    }
}