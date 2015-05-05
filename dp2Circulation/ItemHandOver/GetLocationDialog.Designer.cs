namespace dp2Circulation
{
    partial class GetLocationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetLocationDialog));
            this.label_location = new System.Windows.Forms.Label();
            this.comboBox_location = new System.Windows.Forms.ComboBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_location
            // 
            this.label_location.AutoSize = true;
            this.label_location.Location = new System.Drawing.Point(11, 14);
            this.label_location.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_location.Name = "label_location";
            this.label_location.Size = new System.Drawing.Size(77, 12);
            this.label_location.TabIndex = 2;
            this.label_location.Text = "馆藏地点(&L):";
            // 
            // comboBox_location
            // 
            this.comboBox_location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_location.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_location.FormattingEnabled = true;
            this.comboBox_location.Location = new System.Drawing.Point(100, 11);
            this.comboBox_location.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_location.Name = "comboBox_location";
            this.comboBox_location.Size = new System.Drawing.Size(210, 20);
            this.comboBox_location.TabIndex = 3;
            this.comboBox_location.DropDown += new System.EventHandler(this.comboBox_location_DropDown);
            this.comboBox_location.SizeChanged += new System.EventHandler(this.comboBox_location_SizeChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(235, 54);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(156, 54);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // GetLocationDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(324, 87);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_location);
            this.Controls.Add(this.label_location);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetLocationDialog";
            this.ShowInTaskbar = false;
            this.Text = "请指定要修改成的馆藏地点";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SearchByBatchnoForm_FormClosed);
            this.Load += new System.EventHandler(this.SearchByBatchnoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_location;
        private System.Windows.Forms.ComboBox comboBox_location;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}