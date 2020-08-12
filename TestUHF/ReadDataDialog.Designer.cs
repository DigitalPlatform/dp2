namespace TestUHF
{
    partial class ReadDataDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_read = new System.Windows.Forms.Button();
            this.combobox_memoryBank = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button_write = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.combobox_wordCount = new System.Windows.Forms.ComboBox();
            this.combobox_startWord = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textbox_data = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.combobox_epcList = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_read);
            this.groupBox1.Controls.Add(this.combobox_memoryBank);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.button_write);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.combobox_wordCount);
            this.groupBox1.Controls.Add(this.combobox_startWord);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textbox_data);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(15, 61);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox1.Size = new System.Drawing.Size(1054, 304);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Memory Access";
            // 
            // button_read
            // 
            this.button_read.Location = new System.Drawing.Point(567, 231);
            this.button_read.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_read.Name = "button_read";
            this.button_read.Size = new System.Drawing.Size(270, 52);
            this.button_read.TabIndex = 15;
            this.button_read.Text = "Read";
            this.button_read.UseVisualStyleBackColor = true;
            this.button_read.Click += new System.EventHandler(this.button_read_Click);
            // 
            // combobox_memoryBank
            // 
            this.combobox_memoryBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_memoryBank.FormattingEnabled = true;
            this.combobox_memoryBank.Items.AddRange(new object[] {
            "00:RFU",
            "01:EPC",
            "02:TID",
            "03:USER"});
            this.combobox_memoryBank.Location = new System.Drawing.Point(167, 44);
            this.combobox_memoryBank.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.combobox_memoryBank.Name = "combobox_memoryBank";
            this.combobox_memoryBank.Size = new System.Drawing.Size(219, 29);
            this.combobox_memoryBank.TabIndex = 14;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(22, 49);
            this.label10.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(142, 21);
            this.label10.TabIndex = 13;
            this.label10.Text = "Memory Bank:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(748, 49);
            this.label9.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 21);
            this.label9.TabIndex = 12;
            this.label9.Text = "Hex";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(592, 44);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(142, 31);
            this.textBox1.TabIndex = 11;
            this.textBox1.Text = "00000000";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(451, 49);
            this.label8.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(131, 21);
            this.label8.TabIndex = 10;
            this.label8.Text = "Access Pwd:";
            // 
            // button_write
            // 
            this.button_write.Location = new System.Drawing.Point(150, 231);
            this.button_write.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_write.Name = "button_write";
            this.button_write.Size = new System.Drawing.Size(270, 52);
            this.button_write.TabIndex = 9;
            this.button_write.Text = "Write";
            this.button_write.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(959, 175);
            this.label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 21);
            this.label7.TabIndex = 8;
            this.label7.Text = "Hex";
            // 
            // combobox_wordCount
            // 
            this.combobox_wordCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_wordCount.FormattingEnabled = true;
            this.combobox_wordCount.Location = new System.Drawing.Point(495, 94);
            this.combobox_wordCount.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.combobox_wordCount.Name = "combobox_wordCount";
            this.combobox_wordCount.Size = new System.Drawing.Size(140, 29);
            this.combobox_wordCount.TabIndex = 7;
            // 
            // combobox_startWord
            // 
            this.combobox_startWord.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_startWord.FormattingEnabled = true;
            this.combobox_startWord.Location = new System.Drawing.Point(167, 98);
            this.combobox_startWord.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.combobox_startWord.Name = "combobox_startWord";
            this.combobox_startWord.Size = new System.Drawing.Size(140, 29);
            this.combobox_startWord.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(363, 103);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(131, 21);
            this.label6.TabIndex = 5;
            this.label6.Text = "Word Count:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 105);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 21);
            this.label5.TabIndex = 4;
            this.label5.Text = "Start Word:";
            // 
            // textbox_data
            // 
            this.textbox_data.Location = new System.Drawing.Point(169, 166);
            this.textbox_data.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textbox_data.Name = "textbox_data";
            this.textbox_data.Size = new System.Drawing.Size(776, 31);
            this.textbox_data.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(57, 175);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 21);
            this.label4.TabIndex = 2;
            this.label4.Text = "New Data:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 21);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 21);
            this.label3.TabIndex = 17;
            this.label3.Text = "EPCs:";
            // 
            // combobox_epcList
            // 
            this.combobox_epcList.FormattingEnabled = true;
            this.combobox_epcList.Location = new System.Drawing.Point(92, 14);
            this.combobox_epcList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.combobox_epcList.Name = "combobox_epcList";
            this.combobox_epcList.Size = new System.Drawing.Size(974, 29);
            this.combobox_epcList.TabIndex = 16;
            // 
            // ReadDataDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1102, 390);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.combobox_epcList);
            this.Name = "ReadDataDialog";
            this.Text = "ReadDataDialog";
            this.Load += new System.EventHandler(this.ReadDataDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_read;
        private System.Windows.Forms.ComboBox combobox_memoryBank;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_write;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox combobox_wordCount;
        private System.Windows.Forms.ComboBox combobox_startWord;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textbox_data;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox combobox_epcList;
    }
}