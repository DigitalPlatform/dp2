namespace dp2Circulation
{
    partial class OrderOutputItemDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderOutputItemDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_seller = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_findOutputFormat = new System.Windows.Forms.Button();
            this.comboBox_outputFormat = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(297, 90);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(216, 90);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "渠道(&S):";
            // 
            // comboBox_seller
            // 
            this.comboBox_seller.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_seller.FormattingEnabled = true;
            this.comboBox_seller.Location = new System.Drawing.Point(123, 12);
            this.comboBox_seller.Name = "comboBox_seller";
            this.comboBox_seller.Size = new System.Drawing.Size(200, 23);
            this.comboBox_seller.TabIndex = 1;
            this.comboBox_seller.DropDown += new System.EventHandler(this.comboBox_seller_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "输出格式(&O):";
            // 
            // button_findOutputFormat
            // 
            this.button_findOutputFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findOutputFormat.Location = new System.Drawing.Point(329, 37);
            this.button_findOutputFormat.Name = "button_findOutputFormat";
            this.button_findOutputFormat.Size = new System.Drawing.Size(43, 28);
            this.button_findOutputFormat.TabIndex = 4;
            this.button_findOutputFormat.Text = "...";
            this.button_findOutputFormat.UseVisualStyleBackColor = true;
            this.button_findOutputFormat.Click += new System.EventHandler(this.button_findOutputFormat_Click);
            // 
            // comboBox_outputFormat
            // 
            this.comboBox_outputFormat.FormattingEnabled = true;
            this.comboBox_outputFormat.Items.AddRange(new object[] {
            "<缺省>",
            "<选择一个定制格式...>"});
            this.comboBox_outputFormat.Location = new System.Drawing.Point(123, 41);
            this.comboBox_outputFormat.Name = "comboBox_outputFormat";
            this.comboBox_outputFormat.Size = new System.Drawing.Size(200, 23);
            this.comboBox_outputFormat.TabIndex = 7;
            this.comboBox_outputFormat.DropDownClosed += new System.EventHandler(this.comboBox_outputFormat_DropDownClosed);
            this.comboBox_outputFormat.TextChanged += new System.EventHandler(this.comboBox_outputFormat_TextChanged);
            // 
            // OrderOutputItemDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(384, 130);
            this.Controls.Add(this.comboBox_outputFormat);
            this.Controls.Add(this.button_findOutputFormat);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_seller);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OrderOutputItemDialog";
            this.ShowInTaskbar = false;
            this.Text = "订单输出特性";
            this.Load += new System.EventHandler(this.OrderOutputItemDialog_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OrderOutputItemDialog_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrderOutputItemDialog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_seller;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_findOutputFormat;
        private System.Windows.Forms.ComboBox comboBox_outputFormat;
    }
}