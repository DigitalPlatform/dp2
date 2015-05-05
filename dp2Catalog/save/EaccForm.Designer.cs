namespace dp2Catalog
{
    partial class EaccForm
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBox_u2eFilename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_e2uFilename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_begin = new System.Windows.Forms.Button();
            this.button_findU2eFilename = new System.Windows.Forms.Button();
            this.button_findE2uFilename = new System.Windows.Forms.Button();
            this.button_findOriginFilename = new System.Windows.Forms.Button();
            this.textBox_unihanFilenames = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox_unicodeCode = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_eaccCode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_searchU2e = new System.Windows.Forms.Button();
            this.button_searchE2u = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textBox_unicodeString = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_u2eStringConvert = new System.Windows.Forms.Button();
            this.button_e2uStringConvert = new System.Windows.Forms.Button();
            this.textBox_eaccString = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage1);
            this.tabControl_main.Controls.Add(this.tabPage2);
            this.tabControl_main.Controls.Add(this.tabPage3);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(466, 286);
            this.tabControl_main.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.textBox_u2eFilename);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.textBox_e2uFilename);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.button_begin);
            this.tabPage1.Controls.Add(this.button_findU2eFilename);
            this.tabPage1.Controls.Add(this.button_findE2uFilename);
            this.tabPage1.Controls.Add(this.button_findOriginFilename);
            this.tabPage1.Controls.Add(this.textBox_unihanFilenames);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(458, 258);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "创建码表";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBox_u2eFilename
            // 
            this.textBox_u2eFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_u2eFilename.Location = new System.Drawing.Point(138, 179);
            this.textBox_u2eFilename.Name = "textBox_u2eFilename";
            this.textBox_u2eFilename.Size = new System.Drawing.Size(262, 25);
            this.textBox_u2eFilename.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 186);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 15);
            this.label3.TabIndex = 16;
            this.label3.Text = "u2e码表文件(&U):";
            // 
            // textBox_e2uFilename
            // 
            this.textBox_e2uFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_e2uFilename.Location = new System.Drawing.Point(138, 148);
            this.textBox_e2uFilename.Name = "textBox_e2uFilename";
            this.textBox_e2uFilename.Size = new System.Drawing.Size(262, 25);
            this.textBox_e2uFilename.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "e2u码表文件(&E):";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 103);
            this.label1.TabIndex = 10;
            this.label1.Text = "源文件(&O):\r\n(一行一个)";
            // 
            // button_begin
            // 
            this.button_begin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_begin.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_begin.Location = new System.Drawing.Point(354, 224);
            this.button_begin.Name = "button_begin";
            this.button_begin.Size = new System.Drawing.Size(98, 28);
            this.button_begin.TabIndex = 19;
            this.button_begin.Text = "开始(&B)";
            this.button_begin.UseVisualStyleBackColor = true;
            this.button_begin.Click += new System.EventHandler(this.button_begin_Click);
            // 
            // button_findU2eFilename
            // 
            this.button_findU2eFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findU2eFilename.Location = new System.Drawing.Point(406, 179);
            this.button_findU2eFilename.Name = "button_findU2eFilename";
            this.button_findU2eFilename.Size = new System.Drawing.Size(46, 28);
            this.button_findU2eFilename.TabIndex = 18;
            this.button_findU2eFilename.Text = "...";
            this.button_findU2eFilename.UseVisualStyleBackColor = true;
            // 
            // button_findE2uFilename
            // 
            this.button_findE2uFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findE2uFilename.Location = new System.Drawing.Point(406, 148);
            this.button_findE2uFilename.Name = "button_findE2uFilename";
            this.button_findE2uFilename.Size = new System.Drawing.Size(46, 28);
            this.button_findE2uFilename.TabIndex = 15;
            this.button_findE2uFilename.Text = "...";
            this.button_findE2uFilename.UseVisualStyleBackColor = true;
            // 
            // button_findOriginFilename
            // 
            this.button_findOriginFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findOriginFilename.Location = new System.Drawing.Point(406, 19);
            this.button_findOriginFilename.Name = "button_findOriginFilename";
            this.button_findOriginFilename.Size = new System.Drawing.Size(46, 28);
            this.button_findOriginFilename.TabIndex = 12;
            this.button_findOriginFilename.Text = "...";
            this.button_findOriginFilename.UseVisualStyleBackColor = true;
            // 
            // textBox_unihanFilenames
            // 
            this.textBox_unihanFilenames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unihanFilenames.Location = new System.Drawing.Point(138, 19);
            this.textBox_unihanFilenames.Multiline = true;
            this.textBox_unihanFilenames.Name = "textBox_unihanFilenames";
            this.textBox_unihanFilenames.Size = new System.Drawing.Size(262, 110);
            this.textBox_unihanFilenames.TabIndex = 11;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox_unicodeCode);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.textBox_eaccCode);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.button_searchU2e);
            this.tabPage2.Controls.Add(this.button_searchE2u);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(458, 258);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "编码转换";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox_unicodeCode
            // 
            this.textBox_unicodeCode.Location = new System.Drawing.Point(308, 58);
            this.textBox_unicodeCode.Name = "textBox_unicodeCode";
            this.textBox_unicodeCode.Size = new System.Drawing.Size(144, 25);
            this.textBox_unicodeCode.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(305, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 15);
            this.label5.TabIndex = 21;
            this.label5.Text = "Unicode编码(&U):";
            // 
            // textBox_eaccCode
            // 
            this.textBox_eaccCode.Location = new System.Drawing.Point(6, 58);
            this.textBox_eaccCode.Name = "textBox_eaccCode";
            this.textBox_eaccCode.Size = new System.Drawing.Size(148, 25);
            this.textBox_eaccCode.TabIndex = 20;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 15);
            this.label4.TabIndex = 19;
            this.label4.Text = "EACC编码(&E):";
            // 
            // button_searchU2e
            // 
            this.button_searchU2e.Location = new System.Drawing.Point(160, 58);
            this.button_searchU2e.Name = "button_searchU2e";
            this.button_searchU2e.Size = new System.Drawing.Size(142, 28);
            this.button_searchU2e.TabIndex = 18;
            this.button_searchU2e.Text = "<-- 检索";
            this.button_searchU2e.UseVisualStyleBackColor = true;
            this.button_searchU2e.Click += new System.EventHandler(this.button_searchU2e_Click);
            // 
            // button_searchE2u
            // 
            this.button_searchE2u.Location = new System.Drawing.Point(160, 24);
            this.button_searchE2u.Name = "button_searchE2u";
            this.button_searchE2u.Size = new System.Drawing.Size(142, 28);
            this.button_searchE2u.TabIndex = 17;
            this.button_searchE2u.Text = "检索 -->";
            this.button_searchE2u.UseVisualStyleBackColor = true;
            this.button_searchE2u.Click += new System.EventHandler(this.button_searchE2u_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textBox_unicodeString);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.button_u2eStringConvert);
            this.tabPage3.Controls.Add(this.button_e2uStringConvert);
            this.tabPage3.Controls.Add(this.textBox_eaccString);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(458, 258);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "字符串转换";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textBox_unicodeString
            // 
            this.textBox_unicodeString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unicodeString.Location = new System.Drawing.Point(3, 150);
            this.textBox_unicodeString.Multiline = true;
            this.textBox_unicodeString.Name = "textBox_unicodeString";
            this.textBox_unicodeString.Size = new System.Drawing.Size(452, 95);
            this.textBox_unicodeString.TabIndex = 34;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 132);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 15);
            this.label7.TabIndex = 33;
            this.label7.Text = "Unicode字符串:";
            // 
            // button_u2eStringConvert
            // 
            this.button_u2eStringConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_u2eStringConvert.Location = new System.Drawing.Point(313, 101);
            this.button_u2eStringConvert.Name = "button_u2eStringConvert";
            this.button_u2eStringConvert.Size = new System.Drawing.Size(142, 28);
            this.button_u2eStringConvert.TabIndex = 32;
            this.button_u2eStringConvert.Text = "Unicode-->EACC";
            this.button_u2eStringConvert.UseVisualStyleBackColor = true;
            this.button_u2eStringConvert.Click += new System.EventHandler(this.button_u2eStringConvert_Click);
            // 
            // button_e2uStringConvert
            // 
            this.button_e2uStringConvert.Location = new System.Drawing.Point(3, 101);
            this.button_e2uStringConvert.Name = "button_e2uStringConvert";
            this.button_e2uStringConvert.Size = new System.Drawing.Size(142, 28);
            this.button_e2uStringConvert.TabIndex = 31;
            this.button_e2uStringConvert.Text = "EACC-->Unicode";
            this.button_e2uStringConvert.UseVisualStyleBackColor = true;
            this.button_e2uStringConvert.Click += new System.EventHandler(this.button_e2uStringConvert_Click);
            // 
            // textBox_eaccString
            // 
            this.textBox_eaccString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_eaccString.Location = new System.Drawing.Point(3, 27);
            this.textBox_eaccString.Multiline = true;
            this.textBox_eaccString.Name = "textBox_eaccString";
            this.textBox_eaccString.Size = new System.Drawing.Size(452, 68);
            this.textBox_eaccString.TabIndex = 30;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(0, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 15);
            this.label6.TabIndex = 29;
            this.label6.Text = "EACC字符串:";
            // 
            // EaccForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 320);
            this.Controls.Add(this.tabControl_main);
            this.Name = "EaccForm";
            this.Text = "EACC编码维护窗";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EaccForm_FormClosed);
            this.Load += new System.EventHandler(this.EaccForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textBox_u2eFilename;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_e2uFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.Button button_findU2eFilename;
        private System.Windows.Forms.Button button_findE2uFilename;
        private System.Windows.Forms.Button button_findOriginFilename;
        private System.Windows.Forms.TextBox textBox_unihanFilenames;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox_unicodeCode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_eaccCode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_searchU2e;
        private System.Windows.Forms.Button button_searchE2u;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox textBox_unicodeString;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_u2eStringConvert;
        private System.Windows.Forms.Button button_e2uStringConvert;
        private System.Windows.Forms.TextBox textBox_eaccString;
        private System.Windows.Forms.Label label6;
    }
}