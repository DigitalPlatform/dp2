namespace dp2Circulation
{
    partial class ExportMarcHoldingDialog
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
            this.checkBox_905 = new System.Windows.Forms.CheckBox();
            this.comboBox_905_style = new System.Windows.Forms.ComboBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_removeOld905 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBox_905
            // 
            this.checkBox_905.AutoSize = true;
            this.checkBox_905.Location = new System.Drawing.Point(13, 13);
            this.checkBox_905.Name = "checkBox_905";
            this.checkBox_905.Size = new System.Drawing.Size(174, 16);
            this.checkBox_905.TabIndex = 0;
            this.checkBox_905.Text = "创建 905 字段[根据册记录]";
            this.checkBox_905.UseVisualStyleBackColor = true;
            this.checkBox_905.CheckedChanged += new System.EventHandler(this.checkBox_905_CheckedChanged);
            // 
            // comboBox_905_style
            // 
            this.comboBox_905_style.FormattingEnabled = true;
            this.comboBox_905_style.Items.AddRange(new object[] {
            "只创建单个 905 字段",
            "每册一个 905 字段"});
            this.comboBox_905_style.Location = new System.Drawing.Point(62, 35);
            this.comboBox_905_style.Name = "comboBox_905_style";
            this.comboBox_905_style.Size = new System.Drawing.Size(210, 20);
            this.comboBox_905_style.TabIndex = 1;
            this.comboBox_905_style.Visible = false;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(299, 151);
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
            this.button_OK.Location = new System.Drawing.Point(220, 151);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_removeOld905
            // 
            this.checkBox_removeOld905.AutoSize = true;
            this.checkBox_removeOld905.Location = new System.Drawing.Point(62, 61);
            this.checkBox_removeOld905.Name = "checkBox_removeOld905";
            this.checkBox_removeOld905.Size = new System.Drawing.Size(198, 16);
            this.checkBox_removeOld905.TabIndex = 10;
            this.checkBox_removeOld905.Text = "移除书目记录中原有的 905 字段";
            this.checkBox_removeOld905.UseVisualStyleBackColor = true;
            this.checkBox_removeOld905.Visible = false;
            // 
            // ExportMarcHoldingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(385, 185);
            this.Controls.Add(this.checkBox_removeOld905);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_905_style);
            this.Controls.Add(this.checkBox_905);
            this.Name = "ExportMarcHoldingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "指定 905 字段特性";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_905;
        private System.Windows.Forms.ComboBox comboBox_905_style;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_removeOld905;
    }
}