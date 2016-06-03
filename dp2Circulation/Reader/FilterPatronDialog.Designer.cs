namespace dp2Circulation
{
    partial class FilterPatronDialog
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
            this.checkBox_range_inPeriod = new System.Windows.Forms.CheckBox();
            this.checkBox_range_noBorrowAndOverdueItem = new System.Windows.Forms.CheckBox();
            this.checkBox_range_outofPeriod = new System.Windows.Forms.CheckBox();
            this.checkBox_range_hasOverdueItem = new System.Windows.Forms.CheckBox();
            this.checkBox_range_hasBorrowItem = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBox_range_inPeriod
            // 
            this.checkBox_range_inPeriod.AutoSize = true;
            this.checkBox_range_inPeriod.Location = new System.Drawing.Point(26, 90);
            this.checkBox_range_inPeriod.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_range_inPeriod.Name = "checkBox_range_inPeriod";
            this.checkBox_range_inPeriod.Size = new System.Drawing.Size(60, 16);
            this.checkBox_range_inPeriod.TabIndex = 11;
            this.checkBox_range_inPeriod.Text = "未超期";
            this.checkBox_range_inPeriod.UseVisualStyleBackColor = true;
            this.checkBox_range_inPeriod.CheckedChanged += new System.EventHandler(this.checkBox_range_inPeriod_CheckedChanged);
            // 
            // checkBox_range_noBorrowAndOverdueItem
            // 
            this.checkBox_range_noBorrowAndOverdueItem.AutoSize = true;
            this.checkBox_range_noBorrowAndOverdueItem.Location = new System.Drawing.Point(11, 49);
            this.checkBox_range_noBorrowAndOverdueItem.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_range_noBorrowAndOverdueItem.Name = "checkBox_range_noBorrowAndOverdueItem";
            this.checkBox_range_noBorrowAndOverdueItem.Size = new System.Drawing.Size(144, 16);
            this.checkBox_range_noBorrowAndOverdueItem.TabIndex = 7;
            this.checkBox_range_noBorrowAndOverdueItem.Text = "无 在借册和违约金 的";
            this.checkBox_range_noBorrowAndOverdueItem.UseVisualStyleBackColor = true;
            // 
            // checkBox_range_outofPeriod
            // 
            this.checkBox_range_outofPeriod.AutoSize = true;
            this.checkBox_range_outofPeriod.Location = new System.Drawing.Point(26, 110);
            this.checkBox_range_outofPeriod.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_range_outofPeriod.Name = "checkBox_range_outofPeriod";
            this.checkBox_range_outofPeriod.Size = new System.Drawing.Size(60, 16);
            this.checkBox_range_outofPeriod.TabIndex = 10;
            this.checkBox_range_outofPeriod.Text = "已超期";
            this.checkBox_range_outofPeriod.UseVisualStyleBackColor = true;
            this.checkBox_range_outofPeriod.CheckedChanged += new System.EventHandler(this.checkBox_range_outofPeriod_CheckedChanged);
            // 
            // checkBox_range_hasOverdueItem
            // 
            this.checkBox_range_hasOverdueItem.AutoSize = true;
            this.checkBox_range_hasOverdueItem.Location = new System.Drawing.Point(11, 130);
            this.checkBox_range_hasOverdueItem.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_range_hasOverdueItem.Name = "checkBox_range_hasOverdueItem";
            this.checkBox_range_hasOverdueItem.Size = new System.Drawing.Size(84, 16);
            this.checkBox_range_hasOverdueItem.TabIndex = 9;
            this.checkBox_range_hasOverdueItem.Text = "有违约金的";
            this.checkBox_range_hasOverdueItem.UseVisualStyleBackColor = true;
            // 
            // checkBox_range_hasBorrowItem
            // 
            this.checkBox_range_hasBorrowItem.AutoSize = true;
            this.checkBox_range_hasBorrowItem.Location = new System.Drawing.Point(11, 70);
            this.checkBox_range_hasBorrowItem.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_range_hasBorrowItem.Name = "checkBox_range_hasBorrowItem";
            this.checkBox_range_hasBorrowItem.Size = new System.Drawing.Size(84, 16);
            this.checkBox_range_hasBorrowItem.TabIndex = 8;
            this.checkBox_range_hasBorrowItem.Text = "有在借册的";
            this.checkBox_range_hasBorrowItem.UseVisualStyleBackColor = true;
            this.checkBox_range_hasBorrowItem.CheckedChanged += new System.EventHandler(this.checkBox_range_hasBorrowItem_CheckedChanged);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(170, 208);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 12;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(251, 208);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 13;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // FilterPatronDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 243);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_range_inPeriod);
            this.Controls.Add(this.checkBox_range_noBorrowAndOverdueItem);
            this.Controls.Add(this.checkBox_range_outofPeriod);
            this.Controls.Add(this.checkBox_range_hasOverdueItem);
            this.Controls.Add(this.checkBox_range_hasBorrowItem);
            this.Name = "FilterPatronDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "如何筛选读者记录";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_range_inPeriod;
        private System.Windows.Forms.CheckBox checkBox_range_noBorrowAndOverdueItem;
        private System.Windows.Forms.CheckBox checkBox_range_outofPeriod;
        private System.Windows.Forms.CheckBox checkBox_range_hasOverdueItem;
        private System.Windows.Forms.CheckBox checkBox_range_hasBorrowItem;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}