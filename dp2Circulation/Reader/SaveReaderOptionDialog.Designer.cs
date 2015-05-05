namespace dp2Circulation.Reader
{
    partial class SaveReaderOptionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveReaderOptionDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_clearOutOfReservationCount = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(309, 231);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(248, 231);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            // 
            // checkBox_clearOutOfReservationCount
            // 
            this.checkBox_clearOutOfReservationCount.AutoSize = true;
            this.checkBox_clearOutOfReservationCount.Location = new System.Drawing.Point(12, 12);
            this.checkBox_clearOutOfReservationCount.Name = "checkBox_clearOutOfReservationCount";
            this.checkBox_clearOutOfReservationCount.Size = new System.Drawing.Size(162, 16);
            this.checkBox_clearOutOfReservationCount.TabIndex = 9;
            this.checkBox_clearOutOfReservationCount.Text = "清除 预约到书后未取计数";
            this.checkBox_clearOutOfReservationCount.UseVisualStyleBackColor = true;
            // 
            // SaveReaderOptionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(376, 264);
            this.Controls.Add(this.checkBox_clearOutOfReservationCount);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SaveReaderOptionDialog";
            this.ShowInTaskbar = false;
            this.Text = "保存时选项";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_clearOutOfReservationCount;
    }
}