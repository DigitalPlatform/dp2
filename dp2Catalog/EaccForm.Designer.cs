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
            this.tabPage_createCharsetTable = new System.Windows.Forms.TabPage();
            this.textBox_e2uFilename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_begin = new System.Windows.Forms.Button();
            this.button_findE2uFilename = new System.Windows.Forms.Button();
            this.button_findOriginFilename = new System.Windows.Forms.Button();
            this.textBox_unihanFilenames = new System.Windows.Forms.TextBox();
            this.tabPage_charConvert = new System.Windows.Forms.TabPage();
            this.textBox_unicodeCode = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_eaccCode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_searchU2e = new System.Windows.Forms.Button();
            this.button_searchE2u = new System.Windows.Forms.Button();
            this.tabPage_stringConvert = new System.Windows.Forms.TabPage();
            this.textBox_unicodeString = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_u2eStringConvert = new System.Windows.Forms.Button();
            this.button_e2uStringConvert = new System.Windows.Forms.Button();
            this.textBox_eaccString = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage_windowCodePage = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_codePage = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_codePage_8bitCode = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_codePage_unicodeCode = new System.Windows.Forms.TextBox();
            this.button_codePage_8tou = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_field066value = new System.Windows.Forms.TextBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_createCharsetTable.SuspendLayout();
            this.tabPage_charConvert.SuspendLayout();
            this.tabPage_stringConvert.SuspendLayout();
            this.tabPage_windowCodePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_createCharsetTable);
            this.tabControl_main.Controls.Add(this.tabPage_charConvert);
            this.tabControl_main.Controls.Add(this.tabPage_stringConvert);
            this.tabControl_main.Controls.Add(this.tabPage_windowCodePage);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(466, 351);
            this.tabControl_main.TabIndex = 23;
            // 
            // tabPage_createCharsetTable
            // 
            this.tabPage_createCharsetTable.Controls.Add(this.textBox_e2uFilename);
            this.tabPage_createCharsetTable.Controls.Add(this.label2);
            this.tabPage_createCharsetTable.Controls.Add(this.label1);
            this.tabPage_createCharsetTable.Controls.Add(this.button_begin);
            this.tabPage_createCharsetTable.Controls.Add(this.button_findE2uFilename);
            this.tabPage_createCharsetTable.Controls.Add(this.button_findOriginFilename);
            this.tabPage_createCharsetTable.Controls.Add(this.textBox_unihanFilenames);
            this.tabPage_createCharsetTable.Location = new System.Drawing.Point(4, 24);
            this.tabPage_createCharsetTable.Name = "tabPage_createCharsetTable";
            this.tabPage_createCharsetTable.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_createCharsetTable.Size = new System.Drawing.Size(458, 258);
            this.tabPage_createCharsetTable.TabIndex = 0;
            this.tabPage_createCharsetTable.Text = "创建码表";
            this.tabPage_createCharsetTable.UseVisualStyleBackColor = true;
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
            // tabPage_charConvert
            // 
            this.tabPage_charConvert.Controls.Add(this.textBox_unicodeCode);
            this.tabPage_charConvert.Controls.Add(this.label5);
            this.tabPage_charConvert.Controls.Add(this.textBox_eaccCode);
            this.tabPage_charConvert.Controls.Add(this.label4);
            this.tabPage_charConvert.Controls.Add(this.button_searchU2e);
            this.tabPage_charConvert.Controls.Add(this.button_searchE2u);
            this.tabPage_charConvert.Location = new System.Drawing.Point(4, 24);
            this.tabPage_charConvert.Name = "tabPage_charConvert";
            this.tabPage_charConvert.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_charConvert.Size = new System.Drawing.Size(458, 258);
            this.tabPage_charConvert.TabIndex = 1;
            this.tabPage_charConvert.Text = "编码转换";
            this.tabPage_charConvert.UseVisualStyleBackColor = true;
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
            // tabPage_stringConvert
            // 
            this.tabPage_stringConvert.Controls.Add(this.textBox_field066value);
            this.tabPage_stringConvert.Controls.Add(this.label10);
            this.tabPage_stringConvert.Controls.Add(this.textBox_unicodeString);
            this.tabPage_stringConvert.Controls.Add(this.label7);
            this.tabPage_stringConvert.Controls.Add(this.button_u2eStringConvert);
            this.tabPage_stringConvert.Controls.Add(this.button_e2uStringConvert);
            this.tabPage_stringConvert.Controls.Add(this.textBox_eaccString);
            this.tabPage_stringConvert.Controls.Add(this.label6);
            this.tabPage_stringConvert.Location = new System.Drawing.Point(4, 24);
            this.tabPage_stringConvert.Name = "tabPage_stringConvert";
            this.tabPage_stringConvert.Size = new System.Drawing.Size(458, 323);
            this.tabPage_stringConvert.TabIndex = 2;
            this.tabPage_stringConvert.Text = "字符串转换";
            this.tabPage_stringConvert.UseVisualStyleBackColor = true;
            // 
            // textBox_unicodeString
            // 
            this.textBox_unicodeString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unicodeString.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
            // tabPage_windowCodePage
            // 
            this.tabPage_windowCodePage.Controls.Add(this.button_codePage_8tou);
            this.tabPage_windowCodePage.Controls.Add(this.textBox_codePage_unicodeCode);
            this.tabPage_windowCodePage.Controls.Add(this.label9);
            this.tabPage_windowCodePage.Controls.Add(this.textBox_codePage_8bitCode);
            this.tabPage_windowCodePage.Controls.Add(this.label8);
            this.tabPage_windowCodePage.Controls.Add(this.comboBox_codePage);
            this.tabPage_windowCodePage.Controls.Add(this.label3);
            this.tabPage_windowCodePage.Location = new System.Drawing.Point(4, 24);
            this.tabPage_windowCodePage.Name = "tabPage_windowCodePage";
            this.tabPage_windowCodePage.Size = new System.Drawing.Size(458, 258);
            this.tabPage_windowCodePage.TabIndex = 3;
            this.tabPage_windowCodePage.Text = "Windows代码页";
            this.tabPage_windowCodePage.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "代码页(&C):";
            // 
            // comboBox_codePage
            // 
            this.comboBox_codePage.DropDownHeight = 300;
            this.comboBox_codePage.FormattingEnabled = true;
            this.comboBox_codePage.IntegralHeight = false;
            this.comboBox_codePage.Location = new System.Drawing.Point(121, 14);
            this.comboBox_codePage.Name = "comboBox_codePage";
            this.comboBox_codePage.Size = new System.Drawing.Size(228, 23);
            this.comboBox_codePage.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 61);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(178, 15);
            this.label8.TabIndex = 2;
            this.label8.Text = "8bit码(16进制表示)(&H):";
            // 
            // textBox_codePage_8bitCode
            // 
            this.textBox_codePage_8bitCode.Location = new System.Drawing.Point(121, 80);
            this.textBox_codePage_8bitCode.Name = "textBox_codePage_8bitCode";
            this.textBox_codePage_8bitCode.Size = new System.Drawing.Size(100, 25);
            this.textBox_codePage_8bitCode.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 147);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(202, 15);
            this.label9.TabIndex = 4;
            this.label9.Text = "Unicode码(16进制表示)(&U):";
            // 
            // textBox_codePage_unicodeCode
            // 
            this.textBox_codePage_unicodeCode.Location = new System.Drawing.Point(121, 166);
            this.textBox_codePage_unicodeCode.Name = "textBox_codePage_unicodeCode";
            this.textBox_codePage_unicodeCode.Size = new System.Drawing.Size(100, 25);
            this.textBox_codePage_unicodeCode.TabIndex = 5;
            // 
            // button_codePage_8tou
            // 
            this.button_codePage_8tou.Location = new System.Drawing.Point(121, 112);
            this.button_codePage_8tou.Name = "button_codePage_8tou";
            this.button_codePage_8tou.Size = new System.Drawing.Size(75, 28);
            this.button_codePage_8tou.TabIndex = 6;
            this.button_codePage_8tou.Text = "转换↓";
            this.button_codePage_8tou.UseVisualStyleBackColor = true;
            this.button_codePage_8tou.Click += new System.EventHandler(this.button_codePage_8tou_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 261);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(155, 15);
            this.label10.TabIndex = 35;
            this.label10.Text = "USMARC 066字段内容:";
            // 
            // textBox_field066value
            // 
            this.textBox_field066value.Location = new System.Drawing.Point(177, 258);
            this.textBox_field066value.Name = "textBox_field066value";
            this.textBox_field066value.Size = new System.Drawing.Size(278, 25);
            this.textBox_field066value.TabIndex = 36;
            // 
            // EaccForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 385);
            this.Controls.Add(this.tabControl_main);
            this.Name = "EaccForm";
            this.Text = "EACC编码维护窗";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EaccForm_FormClosed);
            this.Load += new System.EventHandler(this.EaccForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_createCharsetTable.ResumeLayout(false);
            this.tabPage_createCharsetTable.PerformLayout();
            this.tabPage_charConvert.ResumeLayout(false);
            this.tabPage_charConvert.PerformLayout();
            this.tabPage_stringConvert.ResumeLayout(false);
            this.tabPage_stringConvert.PerformLayout();
            this.tabPage_windowCodePage.ResumeLayout(false);
            this.tabPage_windowCodePage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_createCharsetTable;
        private System.Windows.Forms.TextBox textBox_e2uFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.Button button_findE2uFilename;
        private System.Windows.Forms.Button button_findOriginFilename;
        private System.Windows.Forms.TextBox textBox_unihanFilenames;
        private System.Windows.Forms.TabPage tabPage_charConvert;
        private System.Windows.Forms.TextBox textBox_unicodeCode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_eaccCode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_searchU2e;
        private System.Windows.Forms.Button button_searchE2u;
        private System.Windows.Forms.TabPage tabPage_stringConvert;
        private System.Windows.Forms.TextBox textBox_unicodeString;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_u2eStringConvert;
        private System.Windows.Forms.Button button_e2uStringConvert;
        private System.Windows.Forms.TextBox textBox_eaccString;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabPage_windowCodePage;
        private System.Windows.Forms.ComboBox comboBox_codePage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_codePage_8bitCode;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_codePage_unicodeCode;
        private System.Windows.Forms.Button button_codePage_8tou;
        private System.Windows.Forms.TextBox textBox_field066value;
        private System.Windows.Forms.Label label10;
    }
}