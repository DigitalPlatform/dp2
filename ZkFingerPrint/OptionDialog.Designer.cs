namespace ZkFingerprint
{
    partial class OptionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionDialog));
            this.checkBox_displayFingerprintImage = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_threshold = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_threshold)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBox_displayFingerprintImage
            // 
            this.checkBox_displayFingerprintImage.AutoSize = true;
            this.checkBox_displayFingerprintImage.Location = new System.Drawing.Point(15, 18);
            this.checkBox_displayFingerprintImage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.checkBox_displayFingerprintImage.Name = "checkBox_displayFingerprintImage";
            this.checkBox_displayFingerprintImage.Size = new System.Drawing.Size(116, 21);
            this.checkBox_displayFingerprintImage.TabIndex = 0;
            this.checkBox_displayFingerprintImage.Text = "显示指纹图像(&D)";
            this.checkBox_displayFingerprintImage.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(192, 144);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(87, 33);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(286, 144);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(87, 33);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "指纹识别阈值(&S):";
            // 
            // numericUpDown_threshold
            // 
            this.numericUpDown_threshold.Location = new System.Drawing.Point(126, 56);
            this.numericUpDown_threshold.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_threshold.Name = "numericUpDown_threshold";
            this.numericUpDown_threshold.Size = new System.Drawing.Size(73, 23);
            this.numericUpDown_threshold.TabIndex = 4;
            this.numericUpDown_threshold.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // OptionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(388, 194);
            this.Controls.Add(this.numericUpDown_threshold);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_displayFingerprintImage);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "OptionDialog";
            this.ShowInTaskbar = false;
            this.Text = "选项";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_threshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_displayFingerprintImage;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown_threshold;
    }
}