namespace dp2Circulation
{
    partial class OutputAcountBookTextFileDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputAcountBookTextFileDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.checkBox_truncate = new System.Windows.Forms.CheckBox();
            this.checkBox_outputStatisPart = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(270, 131);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 14;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(209, 131);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 13;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label_message
            // 
            this.label_message.AutoSize = true;
            this.label_message.Location = new System.Drawing.Point(8, 7);
            this.label_message.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(161, 12);
            this.label_message.TabIndex = 15;
            this.label_message.Text = "请指定文本文件输出的特性。";
            // 
            // checkBox_truncate
            // 
            this.checkBox_truncate.AutoSize = true;
            this.checkBox_truncate.Location = new System.Drawing.Point(10, 53);
            this.checkBox_truncate.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_truncate.Name = "checkBox_truncate";
            this.checkBox_truncate.Size = new System.Drawing.Size(150, 16);
            this.checkBox_truncate.TabIndex = 16;
            this.checkBox_truncate.Text = "按列配置截断长文字(&T)";
            this.checkBox_truncate.UseVisualStyleBackColor = true;
            // 
            // checkBox_outputStatisPart
            // 
            this.checkBox_outputStatisPart.AutoSize = true;
            this.checkBox_outputStatisPart.Location = new System.Drawing.Point(10, 73);
            this.checkBox_outputStatisPart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_outputStatisPart.Name = "checkBox_outputStatisPart";
            this.checkBox_outputStatisPart.Size = new System.Drawing.Size(114, 16);
            this.checkBox_outputStatisPart.TabIndex = 17;
            this.checkBox_outputStatisPart.Text = "输出统计部分(&T)";
            this.checkBox_outputStatisPart.UseVisualStyleBackColor = true;
            // 
            // OutputAcountBookTextFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(335, 163);
            this.Controls.Add(this.checkBox_outputStatisPart);
            this.Controls.Add(this.checkBox_truncate);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "OutputAcountBookTextFileDialog";
            this.ShowInTaskbar = false;
            this.Text = "财产帐簿输出到文本文件";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.CheckBox checkBox_truncate;
        private System.Windows.Forms.CheckBox checkBox_outputStatisPart;
    }
}