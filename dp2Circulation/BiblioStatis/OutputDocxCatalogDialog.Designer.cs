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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_number = new System.Windows.Forms.TabPage();
            this.tabPage_font = new System.Windows.Forms.TabPage();
            this.checkBox_boldTitleArea = new System.Windows.Forms.CheckBox();
            this.button_getAccessNoFont = new System.Windows.Forms.Button();
            this.button_getContentFont = new System.Windows.Forms.Button();
            this.button_getBarcodeFont = new System.Windows.Forms.Button();
            this.button_getNoFont = new System.Windows.Forms.Button();
            this.tabPage_content = new System.Windows.Forms.TabPage();
            this.checkBox_summary_field = new System.Windows.Forms.CheckBox();
            this.checkBox_resource_identifier_area = new System.Windows.Forms.CheckBox();
            this.checkBox_notes_area = new System.Windows.Forms.CheckBox();
            this.checkBox_series_area = new System.Windows.Forms.CheckBox();
            this.checkBox_material_description_area = new System.Windows.Forms.CheckBox();
            this.checkBox_publication_area = new System.Windows.Forms.CheckBox();
            this.checkBox_material_specific_area = new System.Windows.Forms.CheckBox();
            this.checkBox_edition_area = new System.Windows.Forms.CheckBox();
            this.checkBox_title_area = new System.Windows.Forms.CheckBox();
            this.tabPage_size = new System.Windows.Forms.TabPage();
            this.textBox_size_rowSep = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button_getPageNoFontName = new System.Windows.Forms.Button();
            this.textBox_pageNoFontSize = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBox_pageNoFontName = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_pageNumberStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_biblioNoStart)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage_number.SuspendLayout();
            this.tabPage_font.SuspendLayout();
            this.tabPage_content.SuspendLayout();
            this.tabPage_size.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(595, 524);
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
            this.button_Cancel.Location = new System.Drawing.Point(719, 524);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(118, 37);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // numericUpDown_pageNumberStart
            // 
            this.numericUpDown_pageNumberStart.Location = new System.Drawing.Point(191, 63);
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
            this.label1.Location = new System.Drawing.Point(6, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "起始页码(&P):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "书目起始序号(&I):";
            // 
            // numericUpDown_biblioNoStart
            // 
            this.numericUpDown_biblioNoStart.Location = new System.Drawing.Point(191, 17);
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
            this.label3.Location = new System.Drawing.Point(8, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "序号字体:";
            // 
            // textBox_noFontName
            // 
            this.textBox_noFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_noFontName.Location = new System.Drawing.Point(193, 22);
            this.textBox_noFontName.Name = "textBox_noFontName";
            this.textBox_noFontName.Size = new System.Drawing.Size(541, 31);
            this.textBox_noFontName.TabIndex = 7;
            // 
            // textBox_noFontSize
            // 
            this.textBox_noFontSize.Location = new System.Drawing.Point(193, 59);
            this.textBox_noFontSize.Name = "textBox_noFontSize";
            this.textBox_noFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_noFontSize.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "序号字号:";
            // 
            // textBox_barcodeFontSize
            // 
            this.textBox_barcodeFontSize.Location = new System.Drawing.Point(193, 133);
            this.textBox_barcodeFontSize.Name = "textBox_barcodeFontSize";
            this.textBox_barcodeFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_barcodeFontSize.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(147, 21);
            this.label5.TabIndex = 12;
            this.label5.Text = "册条码号字号:";
            // 
            // textBox_barcodeFontName
            // 
            this.textBox_barcodeFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_barcodeFontName.Location = new System.Drawing.Point(193, 96);
            this.textBox_barcodeFontName.Name = "textBox_barcodeFontName";
            this.textBox_barcodeFontName.Size = new System.Drawing.Size(541, 31);
            this.textBox_barcodeFontName.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 99);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(147, 21);
            this.label6.TabIndex = 10;
            this.label6.Text = "册条码号字体:";
            // 
            // textBox_contentFontSize
            // 
            this.textBox_contentFontSize.Location = new System.Drawing.Point(193, 207);
            this.textBox_contentFontSize.Name = "textBox_contentFontSize";
            this.textBox_contentFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_contentFontSize.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 210);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 21);
            this.label7.TabIndex = 16;
            this.label7.Text = "正文字号:";
            // 
            // textBox_contentFontName
            // 
            this.textBox_contentFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_contentFontName.Location = new System.Drawing.Point(193, 170);
            this.textBox_contentFontName.Name = "textBox_contentFontName";
            this.textBox_contentFontName.Size = new System.Drawing.Size(541, 31);
            this.textBox_contentFontName.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 173);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(105, 21);
            this.label8.TabIndex = 14;
            this.label8.Text = "正文字体:";
            // 
            // textBox_accessNoFontSize
            // 
            this.textBox_accessNoFontSize.Location = new System.Drawing.Point(193, 281);
            this.textBox_accessNoFontSize.Name = "textBox_accessNoFontSize";
            this.textBox_accessNoFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_accessNoFontSize.TabIndex = 21;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 284);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(126, 21);
            this.label9.TabIndex = 20;
            this.label9.Text = "索取号字号:";
            // 
            // textBox_accessNoFontName
            // 
            this.textBox_accessNoFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_accessNoFontName.Location = new System.Drawing.Point(193, 244);
            this.textBox_accessNoFontName.Name = "textBox_accessNoFontName";
            this.textBox_accessNoFontName.Size = new System.Drawing.Size(541, 31);
            this.textBox_accessNoFontName.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 247);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(126, 21);
            this.label10.TabIndex = 18;
            this.label10.Text = "索取号字体:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_number);
            this.tabControl1.Controls.Add(this.tabPage_font);
            this.tabControl1.Controls.Add(this.tabPage_content);
            this.tabControl1.Controls.Add(this.tabPage_size);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(825, 506);
            this.tabControl1.TabIndex = 22;
            // 
            // tabPage_number
            // 
            this.tabPage_number.Controls.Add(this.label2);
            this.tabPage_number.Controls.Add(this.numericUpDown_pageNumberStart);
            this.tabPage_number.Controls.Add(this.label1);
            this.tabPage_number.Controls.Add(this.numericUpDown_biblioNoStart);
            this.tabPage_number.Location = new System.Drawing.Point(4, 31);
            this.tabPage_number.Name = "tabPage_number";
            this.tabPage_number.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_number.Size = new System.Drawing.Size(817, 471);
            this.tabPage_number.TabIndex = 0;
            this.tabPage_number.Text = "编号";
            this.tabPage_number.UseVisualStyleBackColor = true;
            // 
            // tabPage_font
            // 
            this.tabPage_font.Controls.Add(this.button_getPageNoFontName);
            this.tabPage_font.Controls.Add(this.textBox_pageNoFontSize);
            this.tabPage_font.Controls.Add(this.label12);
            this.tabPage_font.Controls.Add(this.textBox_pageNoFontName);
            this.tabPage_font.Controls.Add(this.label13);
            this.tabPage_font.Controls.Add(this.checkBox_boldTitleArea);
            this.tabPage_font.Controls.Add(this.button_getAccessNoFont);
            this.tabPage_font.Controls.Add(this.button_getContentFont);
            this.tabPage_font.Controls.Add(this.button_getBarcodeFont);
            this.tabPage_font.Controls.Add(this.button_getNoFont);
            this.tabPage_font.Controls.Add(this.textBox_noFontName);
            this.tabPage_font.Controls.Add(this.textBox_accessNoFontSize);
            this.tabPage_font.Controls.Add(this.label3);
            this.tabPage_font.Controls.Add(this.label9);
            this.tabPage_font.Controls.Add(this.label4);
            this.tabPage_font.Controls.Add(this.textBox_accessNoFontName);
            this.tabPage_font.Controls.Add(this.textBox_noFontSize);
            this.tabPage_font.Controls.Add(this.label10);
            this.tabPage_font.Controls.Add(this.label6);
            this.tabPage_font.Controls.Add(this.textBox_contentFontSize);
            this.tabPage_font.Controls.Add(this.textBox_barcodeFontName);
            this.tabPage_font.Controls.Add(this.label7);
            this.tabPage_font.Controls.Add(this.label5);
            this.tabPage_font.Controls.Add(this.textBox_contentFontName);
            this.tabPage_font.Controls.Add(this.textBox_barcodeFontSize);
            this.tabPage_font.Controls.Add(this.label8);
            this.tabPage_font.Location = new System.Drawing.Point(4, 31);
            this.tabPage_font.Name = "tabPage_font";
            this.tabPage_font.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_font.Size = new System.Drawing.Size(817, 471);
            this.tabPage_font.TabIndex = 1;
            this.tabPage_font.Text = "字体";
            this.tabPage_font.UseVisualStyleBackColor = true;
            // 
            // checkBox_boldTitleArea
            // 
            this.checkBox_boldTitleArea.AutoSize = true;
            this.checkBox_boldTitleArea.Location = new System.Drawing.Point(12, 416);
            this.checkBox_boldTitleArea.Name = "checkBox_boldTitleArea";
            this.checkBox_boldTitleArea.Size = new System.Drawing.Size(267, 25);
            this.checkBox_boldTitleArea.TabIndex = 26;
            this.checkBox_boldTitleArea.Text = "粗体显示题名与责任者项";
            this.checkBox_boldTitleArea.UseVisualStyleBackColor = true;
            // 
            // button_getAccessNoFont
            // 
            this.button_getAccessNoFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getAccessNoFont.Location = new System.Drawing.Point(736, 244);
            this.button_getAccessNoFont.Name = "button_getAccessNoFont";
            this.button_getAccessNoFont.Size = new System.Drawing.Size(58, 31);
            this.button_getAccessNoFont.TabIndex = 25;
            this.button_getAccessNoFont.Text = "...";
            this.button_getAccessNoFont.UseVisualStyleBackColor = true;
            this.button_getAccessNoFont.Click += new System.EventHandler(this.button_getAccessNoFont_Click);
            // 
            // button_getContentFont
            // 
            this.button_getContentFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getContentFont.Location = new System.Drawing.Point(736, 170);
            this.button_getContentFont.Name = "button_getContentFont";
            this.button_getContentFont.Size = new System.Drawing.Size(58, 31);
            this.button_getContentFont.TabIndex = 24;
            this.button_getContentFont.Text = "...";
            this.button_getContentFont.UseVisualStyleBackColor = true;
            this.button_getContentFont.Click += new System.EventHandler(this.button_getContentFont_Click);
            // 
            // button_getBarcodeFont
            // 
            this.button_getBarcodeFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getBarcodeFont.Location = new System.Drawing.Point(736, 96);
            this.button_getBarcodeFont.Name = "button_getBarcodeFont";
            this.button_getBarcodeFont.Size = new System.Drawing.Size(58, 31);
            this.button_getBarcodeFont.TabIndex = 23;
            this.button_getBarcodeFont.Text = "...";
            this.button_getBarcodeFont.UseVisualStyleBackColor = true;
            this.button_getBarcodeFont.Click += new System.EventHandler(this.button_getBarcodeFont_Click);
            // 
            // button_getNoFont
            // 
            this.button_getNoFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getNoFont.Location = new System.Drawing.Point(736, 22);
            this.button_getNoFont.Name = "button_getNoFont";
            this.button_getNoFont.Size = new System.Drawing.Size(58, 31);
            this.button_getNoFont.TabIndex = 22;
            this.button_getNoFont.Text = "...";
            this.button_getNoFont.UseVisualStyleBackColor = true;
            this.button_getNoFont.Click += new System.EventHandler(this.button_getNoFont_Click);
            // 
            // tabPage_content
            // 
            this.tabPage_content.Controls.Add(this.checkBox_summary_field);
            this.tabPage_content.Controls.Add(this.checkBox_resource_identifier_area);
            this.tabPage_content.Controls.Add(this.checkBox_notes_area);
            this.tabPage_content.Controls.Add(this.checkBox_series_area);
            this.tabPage_content.Controls.Add(this.checkBox_material_description_area);
            this.tabPage_content.Controls.Add(this.checkBox_publication_area);
            this.tabPage_content.Controls.Add(this.checkBox_material_specific_area);
            this.tabPage_content.Controls.Add(this.checkBox_edition_area);
            this.tabPage_content.Controls.Add(this.checkBox_title_area);
            this.tabPage_content.Location = new System.Drawing.Point(4, 31);
            this.tabPage_content.Name = "tabPage_content";
            this.tabPage_content.Size = new System.Drawing.Size(817, 471);
            this.tabPage_content.TabIndex = 2;
            this.tabPage_content.Text = "内容";
            this.tabPage_content.UseVisualStyleBackColor = true;
            // 
            // checkBox_summary_field
            // 
            this.checkBox_summary_field.AutoSize = true;
            this.checkBox_summary_field.Location = new System.Drawing.Point(13, 316);
            this.checkBox_summary_field.Name = "checkBox_summary_field";
            this.checkBox_summary_field.Size = new System.Drawing.Size(274, 25);
            this.checkBox_summary_field.TabIndex = 8;
            this.checkBox_summary_field.Text = "内容提要 summary_field";
            this.checkBox_summary_field.UseVisualStyleBackColor = true;
            // 
            // checkBox_resource_identifier_area
            // 
            this.checkBox_resource_identifier_area.AutoSize = true;
            this.checkBox_resource_identifier_area.Location = new System.Drawing.Point(13, 265);
            this.checkBox_resource_identifier_area.Name = "checkBox_resource_identifier_area";
            this.checkBox_resource_identifier_area.Size = new System.Drawing.Size(395, 25);
            this.checkBox_resource_identifier_area.TabIndex = 7;
            this.checkBox_resource_identifier_area.Text = "获得方式 resource_identifier_area";
            this.checkBox_resource_identifier_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_notes_area
            // 
            this.checkBox_notes_area.AutoSize = true;
            this.checkBox_notes_area.Location = new System.Drawing.Point(13, 234);
            this.checkBox_notes_area.Name = "checkBox_notes_area";
            this.checkBox_notes_area.Size = new System.Drawing.Size(199, 25);
            this.checkBox_notes_area.TabIndex = 6;
            this.checkBox_notes_area.Text = "附注 notes_area";
            this.checkBox_notes_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_series_area
            // 
            this.checkBox_series_area.AutoSize = true;
            this.checkBox_series_area.Location = new System.Drawing.Point(13, 203);
            this.checkBox_series_area.Name = "checkBox_series_area";
            this.checkBox_series_area.Size = new System.Drawing.Size(210, 25);
            this.checkBox_series_area.TabIndex = 5;
            this.checkBox_series_area.Text = "丛编 series_area";
            this.checkBox_series_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_material_description_area
            // 
            this.checkBox_material_description_area.AutoSize = true;
            this.checkBox_material_description_area.Location = new System.Drawing.Point(13, 172);
            this.checkBox_material_description_area.Name = "checkBox_material_description_area";
            this.checkBox_material_description_area.Size = new System.Drawing.Size(406, 25);
            this.checkBox_material_description_area.TabIndex = 4;
            this.checkBox_material_description_area.Text = "载体形态 material_description_area";
            this.checkBox_material_description_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_publication_area
            // 
            this.checkBox_publication_area.AutoSize = true;
            this.checkBox_publication_area.Location = new System.Drawing.Point(13, 141);
            this.checkBox_publication_area.Name = "checkBox_publication_area";
            this.checkBox_publication_area.Size = new System.Drawing.Size(307, 25);
            this.checkBox_publication_area.TabIndex = 3;
            this.checkBox_publication_area.Text = "出版发行 publication_area";
            this.checkBox_publication_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_material_specific_area
            // 
            this.checkBox_material_specific_area.AutoSize = true;
            this.checkBox_material_specific_area.Location = new System.Drawing.Point(13, 110);
            this.checkBox_material_specific_area.Name = "checkBox_material_specific_area";
            this.checkBox_material_specific_area.Size = new System.Drawing.Size(415, 25);
            this.checkBox_material_specific_area.TabIndex = 2;
            this.checkBox_material_specific_area.Text = "资料特殊细节 material_specific_area";
            this.checkBox_material_specific_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_edition_area
            // 
            this.checkBox_edition_area.AutoSize = true;
            this.checkBox_edition_area.Location = new System.Drawing.Point(13, 79);
            this.checkBox_edition_area.Name = "checkBox_edition_area";
            this.checkBox_edition_area.Size = new System.Drawing.Size(221, 25);
            this.checkBox_edition_area.TabIndex = 1;
            this.checkBox_edition_area.Text = "版本 edition_area";
            this.checkBox_edition_area.UseVisualStyleBackColor = true;
            // 
            // checkBox_title_area
            // 
            this.checkBox_title_area.AutoSize = true;
            this.checkBox_title_area.Location = new System.Drawing.Point(13, 48);
            this.checkBox_title_area.Name = "checkBox_title_area";
            this.checkBox_title_area.Size = new System.Drawing.Size(283, 25);
            this.checkBox_title_area.TabIndex = 0;
            this.checkBox_title_area.Text = "题名与责任者 title_area";
            this.checkBox_title_area.UseVisualStyleBackColor = true;
            // 
            // tabPage_size
            // 
            this.tabPage_size.Controls.Add(this.textBox_size_rowSep);
            this.tabPage_size.Controls.Add(this.label11);
            this.tabPage_size.Location = new System.Drawing.Point(4, 31);
            this.tabPage_size.Name = "tabPage_size";
            this.tabPage_size.Size = new System.Drawing.Size(817, 471);
            this.tabPage_size.TabIndex = 3;
            this.tabPage_size.Text = "尺寸";
            this.tabPage_size.UseVisualStyleBackColor = true;
            // 
            // textBox_size_rowSep
            // 
            this.textBox_size_rowSep.Location = new System.Drawing.Point(195, 39);
            this.textBox_size_rowSep.Name = "textBox_size_rowSep";
            this.textBox_size_rowSep.Size = new System.Drawing.Size(192, 31);
            this.textBox_size_rowSep.TabIndex = 23;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(10, 42);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(126, 21);
            this.label11.TabIndex = 22;
            this.label11.Text = "表格行间距:";
            // 
            // button_getPageNoFontName
            // 
            this.button_getPageNoFontName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getPageNoFontName.Location = new System.Drawing.Point(736, 329);
            this.button_getPageNoFontName.Name = "button_getPageNoFontName";
            this.button_getPageNoFontName.Size = new System.Drawing.Size(58, 31);
            this.button_getPageNoFontName.TabIndex = 31;
            this.button_getPageNoFontName.Text = "...";
            this.button_getPageNoFontName.UseVisualStyleBackColor = true;
            this.button_getPageNoFontName.Click += new System.EventHandler(this.button_getPageNoFontName_Click);
            // 
            // textBox_pageNoFontSize
            // 
            this.textBox_pageNoFontSize.Location = new System.Drawing.Point(193, 366);
            this.textBox_pageNoFontSize.Name = "textBox_pageNoFontSize";
            this.textBox_pageNoFontSize.Size = new System.Drawing.Size(192, 31);
            this.textBox_pageNoFontSize.TabIndex = 30;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 369);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 21);
            this.label12.TabIndex = 29;
            this.label12.Text = "页码字号:";
            // 
            // textBox_pageNoFontName
            // 
            this.textBox_pageNoFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_pageNoFontName.Location = new System.Drawing.Point(193, 329);
            this.textBox_pageNoFontName.Name = "textBox_pageNoFontName";
            this.textBox_pageNoFontName.Size = new System.Drawing.Size(541, 31);
            this.textBox_pageNoFontName.TabIndex = 28;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 332);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(105, 21);
            this.label13.TabIndex = 27;
            this.label13.Text = "页码字体:";
            // 
            // OutputDocxCatalogDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(849, 573);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "OutputDocxCatalogDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "输出 docx 书本式目录";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_pageNumberStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_biblioNoStart)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage_number.ResumeLayout(false);
            this.tabPage_number.PerformLayout();
            this.tabPage_font.ResumeLayout(false);
            this.tabPage_font.PerformLayout();
            this.tabPage_content.ResumeLayout(false);
            this.tabPage_content.PerformLayout();
            this.tabPage_size.ResumeLayout(false);
            this.tabPage_size.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_number;
        private System.Windows.Forms.TabPage tabPage_font;
        private System.Windows.Forms.TabPage tabPage_content;
        private System.Windows.Forms.CheckBox checkBox_resource_identifier_area;
        private System.Windows.Forms.CheckBox checkBox_notes_area;
        private System.Windows.Forms.CheckBox checkBox_series_area;
        private System.Windows.Forms.CheckBox checkBox_material_description_area;
        private System.Windows.Forms.CheckBox checkBox_publication_area;
        private System.Windows.Forms.CheckBox checkBox_material_specific_area;
        private System.Windows.Forms.CheckBox checkBox_edition_area;
        private System.Windows.Forms.CheckBox checkBox_title_area;
        private System.Windows.Forms.CheckBox checkBox_summary_field;
        private System.Windows.Forms.Button button_getNoFont;
        private System.Windows.Forms.Button button_getAccessNoFont;
        private System.Windows.Forms.Button button_getContentFont;
        private System.Windows.Forms.Button button_getBarcodeFont;
        private System.Windows.Forms.CheckBox checkBox_boldTitleArea;
        private System.Windows.Forms.TabPage tabPage_size;
        private System.Windows.Forms.TextBox textBox_size_rowSep;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button_getPageNoFontName;
        private System.Windows.Forms.TextBox textBox_pageNoFontSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBox_pageNoFontName;
        private System.Windows.Forms.Label label13;
    }
}