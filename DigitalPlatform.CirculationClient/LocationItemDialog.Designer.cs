namespace DigitalPlatform.CirculationClient
{
    partial class LocationItemDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocationItemDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_location = new System.Windows.Forms.TextBox();
            this.checkBox_canBorrow = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_libraryCode = new System.Windows.Forms.ComboBox();
            this.checkBox_itemBarcodeNullable = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 39);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "馆藏地名称(&N):";
            // 
            // textBox_location
            // 
            this.textBox_location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_location.Location = new System.Drawing.Point(119, 36);
            this.textBox_location.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_location.Name = "textBox_location";
            this.textBox_location.Size = new System.Drawing.Size(169, 21);
            this.textBox_location.TabIndex = 3;
            // 
            // checkBox_canBorrow
            // 
            this.checkBox_canBorrow.AutoSize = true;
            this.checkBox_canBorrow.Location = new System.Drawing.Point(13, 103);
            this.checkBox_canBorrow.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_canBorrow.Name = "checkBox_canBorrow";
            this.checkBox_canBorrow.Size = new System.Drawing.Size(90, 16);
            this.checkBox_canBorrow.TabIndex = 5;
            this.checkBox_canBorrow.Text = "允许外借(&B)";
            this.checkBox_canBorrow.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(232, 142);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(172, 142);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 15);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "馆代码(&L):";
            // 
            // comboBox_libraryCode
            // 
            this.comboBox_libraryCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_libraryCode.FormattingEnabled = true;
            this.comboBox_libraryCode.Location = new System.Drawing.Point(119, 12);
            this.comboBox_libraryCode.Name = "comboBox_libraryCode";
            this.comboBox_libraryCode.Size = new System.Drawing.Size(169, 20);
            this.comboBox_libraryCode.TabIndex = 1;
            // 
            // checkBox_itemBarcodeNullable
            // 
            this.checkBox_itemBarcodeNullable.AutoSize = true;
            this.checkBox_itemBarcodeNullable.Location = new System.Drawing.Point(13, 74);
            this.checkBox_itemBarcodeNullable.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_itemBarcodeNullable.Name = "checkBox_itemBarcodeNullable";
            this.checkBox_itemBarcodeNullable.Size = new System.Drawing.Size(126, 16);
            this.checkBox_itemBarcodeNullable.TabIndex = 4;
            this.checkBox_itemBarcodeNullable.Text = "册条码号可为空(&N)";
            this.checkBox_itemBarcodeNullable.UseVisualStyleBackColor = true;
            // 
            // LocationItemDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(299, 175);
            this.Controls.Add(this.checkBox_itemBarcodeNullable);
            this.Controls.Add(this.comboBox_libraryCode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_canBorrow);
            this.Controls.Add(this.textBox_location);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LocationItemDialog";
            this.ShowInTaskbar = false;
            this.Text = "馆藏地";
            this.Load += new System.EventHandler(this.LocationItemDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_location;
        private System.Windows.Forms.CheckBox checkBox_canBorrow;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_libraryCode;
        private System.Windows.Forms.CheckBox checkBox_itemBarcodeNullable;
    }
}