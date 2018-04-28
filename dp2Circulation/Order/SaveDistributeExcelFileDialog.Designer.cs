namespace dp2Circulation.Order
{
    partial class SaveDistributeExcelFileDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_seller = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_getOutputFileName = new System.Windows.Forms.Button();
            this.textBox_outputFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "书商(&S):";
            // 
            // comboBox_seller
            // 
            this.comboBox_seller.BackColor = System.Drawing.SystemColors.Window;
            this.comboBox_seller.Location = new System.Drawing.Point(167, 12);
            this.comboBox_seller.Margin = new System.Windows.Forms.Padding(0);
            this.comboBox_seller.Name = "comboBox_seller";
            this.comboBox_seller.Size = new System.Drawing.Size(308, 21);
            this.comboBox_seller.TabIndex = 1;
            this.comboBox_seller.DropDown += new System.EventHandler(this.comboBox_seller_DropDown);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(507, 329);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(112, 34);
            this.button_Cancel.TabIndex = 24;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(386, 329);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 34);
            this.button_OK.TabIndex = 23;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_getOutputFileName
            // 
            this.button_getOutputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getOutputFileName.Location = new System.Drawing.Point(547, 187);
            this.button_getOutputFileName.Margin = new System.Windows.Forms.Padding(4);
            this.button_getOutputFileName.Name = "button_getOutputFileName";
            this.button_getOutputFileName.Size = new System.Drawing.Size(72, 34);
            this.button_getOutputFileName.TabIndex = 27;
            this.button_getOutputFileName.Text = "...";
            this.button_getOutputFileName.UseVisualStyleBackColor = true;
            this.button_getOutputFileName.Click += new System.EventHandler(this.button_getOutputFileName_Click);
            // 
            // textBox_outputFileName
            // 
            this.textBox_outputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputFileName.Location = new System.Drawing.Point(15, 187);
            this.textBox_outputFileName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_outputFileName.Name = "textBox_outputFileName";
            this.textBox_outputFileName.Size = new System.Drawing.Size(524, 28);
            this.textBox_outputFileName.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 165);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 18);
            this.label2.TabIndex = 25;
            this.label2.Text = "Excel 文件名(&F):";
            // 
            // SaveDistributeExcelFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(632, 376);
            this.Controls.Add(this.button_getOutputFileName);
            this.Controls.Add(this.textBox_outputFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_seller);
            this.Controls.Add(this.label1);
            this.Name = "SaveDistributeExcelFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "保存到订购去向 Excel 文件";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.CheckedComboBox comboBox_seller;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_getOutputFileName;
        private System.Windows.Forms.TextBox textBox_outputFileName;
        private System.Windows.Forms.Label label2;
    }
}