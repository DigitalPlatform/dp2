namespace dp2Circulation
{
    partial class PrintColumnDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintColumnDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_columnName = new System.Windows.Forms.ComboBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_caption = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_maxChars = new System.Windows.Forms.NumericUpDown();
            this.textBox_eval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown_widthChars = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxChars)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_widthChars)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "栏目名(&N):";
            // 
            // comboBox_columnName
            // 
            this.comboBox_columnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_columnName.FormattingEnabled = true;
            this.comboBox_columnName.Items.AddRange(new object[] {
            "no -- 序号",
            "barcode -- 册条码号",
            "summary -- 摘要",
            "accessNo -- 索取号",
            "state -- 状态",
            "location -- 馆藏地点",
            "price -- 册价格",
            "bookType -- 册类型",
            "registerNo -- 登录号",
            "comment -- 注释",
            "mergeComment -- 合并注释",
            "batchNo -- 批次号",
            "borrower -- 借阅者",
            "borrowDate -- 借阅日期",
            "borrowPeriod -- 借阅期限",
            "recpath -- 册记录路径",
            "biblioRecpath -- 种记录路径",
            "biblioPrice -- 种价格"});
            this.comboBox_columnName.Location = new System.Drawing.Point(99, 10);
            this.comboBox_columnName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_columnName.Name = "comboBox_columnName";
            this.comboBox_columnName.Size = new System.Drawing.Size(179, 20);
            this.comboBox_columnName.TabIndex = 1;
            this.comboBox_columnName.SelectedIndexChanged += new System.EventHandler(this.comboBox_columnName_SelectedIndexChanged);
            this.comboBox_columnName.DropDownClosed += new System.EventHandler(this.comboBox_columnName_DropDownClosed);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(160, 232);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(220, 232);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 35);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "标题文字(&C):";
            // 
            // textBox_caption
            // 
            this.textBox_caption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_caption.Location = new System.Drawing.Point(99, 33);
            this.textBox_caption.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_caption.Name = "textBox_caption";
            this.textBox_caption.Size = new System.Drawing.Size(179, 21);
            this.textBox_caption.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 95);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "截断字符数(&M):";
            // 
            // numericUpDown_maxChars
            // 
            this.numericUpDown_maxChars.Location = new System.Drawing.Point(99, 94);
            this.numericUpDown_maxChars.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_maxChars.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.numericUpDown_maxChars.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_maxChars.Name = "numericUpDown_maxChars";
            this.numericUpDown_maxChars.Size = new System.Drawing.Size(90, 21);
            this.numericUpDown_maxChars.TabIndex = 7;
            this.numericUpDown_maxChars.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // textBox_eval
            // 
            this.textBox_eval.AcceptsReturn = true;
            this.textBox_eval.AcceptsTab = true;
            this.textBox_eval.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_eval.Location = new System.Drawing.Point(99, 120);
            this.textBox_eval.Multiline = true;
            this.textBox_eval.Name = "textBox_eval";
            this.textBox_eval.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_eval.Size = new System.Drawing.Size(175, 92);
            this.textBox_eval.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "脚本(&E):";
            // 
            // numericUpDown_widthChars
            // 
            this.numericUpDown_widthChars.Location = new System.Drawing.Point(99, 58);
            this.numericUpDown_widthChars.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown_widthChars.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.numericUpDown_widthChars.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_widthChars.Name = "numericUpDown_widthChars";
            this.numericUpDown_widthChars.Size = new System.Drawing.Size(90, 21);
            this.numericUpDown_widthChars.TabIndex = 5;
            this.numericUpDown_widthChars.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 59);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "栏宽字符数(&W):";
            // 
            // PrintColumnDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(286, 264);
            this.Controls.Add(this.numericUpDown_widthChars);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_eval);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown_maxChars);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_caption);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_columnName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "PrintColumnDlg";
            this.ShowInTaskbar = false;
            this.Text = "一个栏目";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxChars)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_widthChars)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_columnName;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_caption;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_maxChars;
        private System.Windows.Forms.TextBox textBox_eval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown_widthChars;
        private System.Windows.Forms.Label label5;
    }
}