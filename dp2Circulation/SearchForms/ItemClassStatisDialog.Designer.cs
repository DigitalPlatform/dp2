namespace dp2Circulation
{
    partial class ItemClassStatisDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.button_getOutputExcelFileName = new System.Windows.Forms.Button();
            this.textBox_outputExcelFileName = new System.Windows.Forms.TextBox();
            this.comboBox_classType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_price = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Excel 文件名(&F):";
            // 
            // button_getOutputExcelFileName
            // 
            this.button_getOutputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getOutputExcelFileName.Location = new System.Drawing.Point(351, 22);
            this.button_getOutputExcelFileName.Name = "button_getOutputExcelFileName";
            this.button_getOutputExcelFileName.Size = new System.Drawing.Size(44, 23);
            this.button_getOutputExcelFileName.TabIndex = 5;
            this.button_getOutputExcelFileName.Text = "...";
            this.button_getOutputExcelFileName.UseVisualStyleBackColor = true;
            this.button_getOutputExcelFileName.Click += new System.EventHandler(this.button_getOutputExcelFileName_Click);
            // 
            // textBox_outputExcelFileName
            // 
            this.textBox_outputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputExcelFileName.Location = new System.Drawing.Point(12, 24);
            this.textBox_outputExcelFileName.Name = "textBox_outputExcelFileName";
            this.textBox_outputExcelFileName.Size = new System.Drawing.Size(333, 21);
            this.textBox_outputExcelFileName.TabIndex = 4;
            // 
            // comboBox_classType
            // 
            this.comboBox_classType.FormattingEnabled = true;
            this.comboBox_classType.Items.AddRange(new object[] {
            "中图法",
            "科图法",
            "人大法",
            "石头汤分类法",
            "DDC",
            "UDC",
            "LCC",
            "其它"});
            this.comboBox_classType.Location = new System.Drawing.Point(12, 95);
            this.comboBox_classType.Name = "comboBox_classType";
            this.comboBox_classType.Size = new System.Drawing.Size(201, 20);
            this.comboBox_classType.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "分类法(&C):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(320, 282);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(241, 282);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_price
            // 
            this.checkBox_price.AutoSize = true;
            this.checkBox_price.Location = new System.Drawing.Point(12, 162);
            this.checkBox_price.Name = "checkBox_price";
            this.checkBox_price.Size = new System.Drawing.Size(78, 16);
            this.checkBox_price.TabIndex = 10;
            this.checkBox_price.Text = "价格列(&P)";
            this.checkBox_price.UseVisualStyleBackColor = true;
            // 
            // ItemClassStatisDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(406, 316);
            this.Controls.Add(this.checkBox_price);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_classType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_getOutputExcelFileName);
            this.Controls.Add(this.textBox_outputExcelFileName);
            this.Name = "ItemClassStatisDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "册分类统计";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getOutputExcelFileName;
        private System.Windows.Forms.TextBox textBox_outputExcelFileName;
        private System.Windows.Forms.ComboBox comboBox_classType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_price;
    }
}