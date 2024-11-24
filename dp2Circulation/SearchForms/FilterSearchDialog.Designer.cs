namespace dp2Circulation.SearchForms
{
    partial class FilterSearchDialog
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
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_biblioDbName = new System.Windows.Forms.Label();
            this.checkedComboBox_biblioDbNames = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label_matchStyle = new System.Windows.Forms.Label();
            this.comboBox_matchStyle = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_location = new System.Windows.Forms.ComboBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.textBox_from = new System.Windows.Forms.TextBox();
            this.checkBox_dontUseBatch = new System.Windows.Forms.CheckBox();
            this.checkBox_detect = new System.Windows.Forms.CheckBox();
            this.textBox_combinations = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.numericUpDown_maxBiblioResultCount = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxBiblioResultCount)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_queryWord.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.textBox_queryWord.Location = new System.Drawing.Point(168, 16);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(7);
            this.textBox_queryWord.MaxLength = 0;
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.ReadOnly = true;
            this.textBox_queryWord.Size = new System.Drawing.Size(313, 31);
            this.textBox_queryWord.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label1.Location = new System.Drawing.Point(16, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "检索词(&W):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_biblioDbName
            // 
            this.label_biblioDbName.AutoSize = true;
            this.label_biblioDbName.Location = new System.Drawing.Point(14, 63);
            this.label_biblioDbName.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_biblioDbName.Name = "label_biblioDbName";
            this.label_biblioDbName.Size = new System.Drawing.Size(117, 21);
            this.label_biblioDbName.TabIndex = 2;
            this.label_biblioDbName.Text = "书目库(&D):";
            this.label_biblioDbName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkedComboBox_biblioDbNames
            // 
            this.checkedComboBox_biblioDbNames.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_biblioDbNames.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_biblioDbNames.Location = new System.Drawing.Point(168, 58);
            this.checkedComboBox_biblioDbNames.Margin = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_biblioDbNames.Name = "checkedComboBox_biblioDbNames";
            this.checkedComboBox_biblioDbNames.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_biblioDbNames.ReadOnly = false;
            this.checkedComboBox_biblioDbNames.Size = new System.Drawing.Size(313, 32);
            this.checkedComboBox_biblioDbNames.TabIndex = 3;
            this.checkedComboBox_biblioDbNames.DropDown += new System.EventHandler(this.checkedComboBox_biblioDbNames_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 104);
            this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "检索途径(&F):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_matchStyle
            // 
            this.label_matchStyle.AutoSize = true;
            this.label_matchStyle.Location = new System.Drawing.Point(16, 186);
            this.label_matchStyle.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label_matchStyle.Name = "label_matchStyle";
            this.label_matchStyle.Size = new System.Drawing.Size(138, 21);
            this.label_matchStyle.TabIndex = 8;
            this.label_matchStyle.Text = "匹配方式(&M):";
            this.label_matchStyle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_matchStyle
            // 
            this.comboBox_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_matchStyle.FormattingEnabled = true;
            this.comboBox_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致",
            "空值"});
            this.comboBox_matchStyle.Location = new System.Drawing.Point(168, 183);
            this.comboBox_matchStyle.Margin = new System.Windows.Forms.Padding(7);
            this.comboBox_matchStyle.Name = "comboBox_matchStyle";
            this.comboBox_matchStyle.Size = new System.Drawing.Size(313, 29);
            this.comboBox_matchStyle.TabIndex = 9;
            this.comboBox_matchStyle.Text = "前方一致";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 229);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "馆藏地(&L):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_location
            // 
            this.comboBox_location.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_location.FormattingEnabled = true;
            this.comboBox_location.Location = new System.Drawing.Point(168, 226);
            this.comboBox_location.Margin = new System.Windows.Forms.Padding(7);
            this.comboBox_location.Name = "comboBox_location";
            this.comboBox_location.Size = new System.Drawing.Size(313, 29);
            this.comboBox_location.TabIndex = 11;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(392, 436);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 14;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(542, 436);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 15;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // textBox_from
            // 
            this.textBox_from.Location = new System.Drawing.Point(168, 101);
            this.textBox_from.Name = "textBox_from";
            this.textBox_from.ReadOnly = true;
            this.textBox_from.Size = new System.Drawing.Size(313, 31);
            this.textBox_from.TabIndex = 5;
            // 
            // checkBox_dontUseBatch
            // 
            this.checkBox_dontUseBatch.AutoSize = true;
            this.checkBox_dontUseBatch.Location = new System.Drawing.Point(18, 307);
            this.checkBox_dontUseBatch.Name = "checkBox_dontUseBatch";
            this.checkBox_dontUseBatch.Size = new System.Drawing.Size(206, 25);
            this.checkBox_dontUseBatch.TabIndex = 13;
            this.checkBox_dontUseBatch.Text = "不使用批检索 API";
            this.checkBox_dontUseBatch.UseVisualStyleBackColor = true;
            // 
            // checkBox_detect
            // 
            this.checkBox_detect.AutoSize = true;
            this.checkBox_detect.Location = new System.Drawing.Point(18, 276);
            this.checkBox_detect.Name = "checkBox_detect";
            this.checkBox_detect.Size = new System.Drawing.Size(153, 25);
            this.checkBox_detect.TabIndex = 12;
            this.checkBox_detect.Text = "探测检索(&D)";
            this.checkBox_detect.UseVisualStyleBackColor = true;
            // 
            // textBox_combinations
            // 
            this.textBox_combinations.Location = new System.Drawing.Point(168, 138);
            this.textBox_combinations.Name = "textBox_combinations";
            this.textBox_combinations.ReadOnly = true;
            this.textBox_combinations.Size = new System.Drawing.Size(313, 31);
            this.textBox_combinations.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 141);
            this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "过滤途径(&I):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(16, 357);
            this.label36.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(264, 21);
            this.label36.TabIndex = 16;
            this.label36.Text = "单检索词最大命中条数(&M):";
            // 
            // numericUpDown_maxBiblioResultCount
            // 
            this.numericUpDown_maxBiblioResultCount.Location = new System.Drawing.Point(20, 381);
            this.numericUpDown_maxBiblioResultCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_maxBiblioResultCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_maxBiblioResultCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_maxBiblioResultCount.Name = "numericUpDown_maxBiblioResultCount";
            this.numericUpDown_maxBiblioResultCount.Size = new System.Drawing.Size(151, 31);
            this.numericUpDown_maxBiblioResultCount.TabIndex = 17;
            // 
            // FilterSearchDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(695, 490);
            this.Controls.Add(this.label36);
            this.Controls.Add(this.numericUpDown_maxBiblioResultCount);
            this.Controls.Add(this.textBox_combinations);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox_detect);
            this.Controls.Add(this.checkBox_dontUseBatch);
            this.Controls.Add(this.textBox_from);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox_location);
            this.Controls.Add(this.label_matchStyle);
            this.Controls.Add(this.comboBox_matchStyle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_biblioDbName);
            this.Controls.Add(this.textBox_queryWord);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkedComboBox_biblioDbNames);
            this.Name = "FilterSearchDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "启动组合检索";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FilterSearchDialog_FormClosed);
            this.Load += new System.EventHandler(this.FilterSearchDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxBiblioResultCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_biblioDbName;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_biblioDbNames;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_from;
        private System.Windows.Forms.Label label_matchStyle;
        private System.Windows.Forms.ComboBox comboBox_matchStyle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_location;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_from;
        private System.Windows.Forms.CheckBox checkBox_dontUseBatch;
        private System.Windows.Forms.CheckBox checkBox_detect;
        private System.Windows.Forms.TextBox textBox_combinations;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.NumericUpDown numericUpDown_maxBiblioResultCount;
    }
}