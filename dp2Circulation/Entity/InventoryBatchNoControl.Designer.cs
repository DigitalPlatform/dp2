namespace dp2Circulation
{
    partial class InventoryBatchNoControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox_libraryCode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_number = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // comboBox_libraryCode
            // 
            this.comboBox_libraryCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_libraryCode.FormattingEnabled = true;
            this.comboBox_libraryCode.Location = new System.Drawing.Point(0, 1);
            this.comboBox_libraryCode.Margin = new System.Windows.Forms.Padding(6);
            this.comboBox_libraryCode.Name = "comboBox_libraryCode";
            this.comboBox_libraryCode.Size = new System.Drawing.Size(172, 20);
            this.comboBox_libraryCode.TabIndex = 0;
            this.comboBox_libraryCode.TextChanged += new System.EventHandler(this.comboBox_libraryCode_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(177, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "-";
            // 
            // textBox_number
            // 
            this.textBox_number.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_number.Location = new System.Drawing.Point(200, 1);
            this.textBox_number.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_number.Name = "textBox_number";
            this.textBox_number.Size = new System.Drawing.Size(71, 21);
            this.textBox_number.TabIndex = 2;
            this.textBox_number.TextChanged += new System.EventHandler(this.textBox_number_TextChanged);
            // 
            // InventoryBatchNoControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.textBox_number);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_libraryCode);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "InventoryBatchNoControl";
            this.Size = new System.Drawing.Size(277, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_libraryCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_number;
    }
}
