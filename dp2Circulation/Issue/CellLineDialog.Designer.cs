namespace dp2Circulation
{
    partial class CellLineDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CellLineDialog));
            this.textBox_caption = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.comboBox_fieldName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_caption
            // 
            this.textBox_caption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_caption.Location = new System.Drawing.Point(132, 41);
            this.textBox_caption.Name = "textBox_caption";
            this.textBox_caption.Size = new System.Drawing.Size(237, 25);
            this.textBox_caption.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "标题文字(&C):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(294, 172);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 15;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(213, 172);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 14;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // comboBox_fieldName
            // 
            this.comboBox_fieldName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_fieldName.FormattingEnabled = true;
            this.comboBox_fieldName.Items.AddRange(new object[] {
            "location -- 馆藏地点",
            "intact -- 完好率",
            "state -- 册状态",
            "refID -- 参考ID",
            "publishTime -- 出版时间",
            "barcode -- 册条码号",
            "regitserNo -- 册登录号",
            "source -- 经费来源",
            "seller -- 订购渠道",
            "accessNo -- 索取号",
            "bookType -- 册类型",
            "price -- 册价格",
            "volumn -- 卷期号",
            "comment -- 注释",
            "batchNo -- 批次号",
            "binding -- 合订信息",
            "recpath -- 记录路径",
            "mergeComment -- 合并注释",
            "borrower -- 借阅者",
            "borrowDate -- 借阅日期",
            "borrowPeriod -- 借阅期限"});
            this.comboBox_fieldName.Location = new System.Drawing.Point(132, 12);
            this.comboBox_fieldName.Name = "comboBox_fieldName";
            this.comboBox_fieldName.Size = new System.Drawing.Size(237, 23);
            this.comboBox_fieldName.TabIndex = 11;
            this.comboBox_fieldName.SelectedIndexChanged += new System.EventHandler(this.comboBox_fieldName_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "字段名(&F):";
            // 
            // CellLineDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(381, 212);
            this.Controls.Add(this.textBox_caption);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_fieldName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CellLineDialog";
            this.Text = "一个行";
            this.Load += new System.EventHandler(this.CellLineDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_caption;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ComboBox comboBox_fieldName;
        private System.Windows.Forms.Label label1;
    }
}