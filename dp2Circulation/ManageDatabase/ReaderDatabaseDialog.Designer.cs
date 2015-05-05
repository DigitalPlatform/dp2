namespace dp2Circulation
{
    partial class ReaderDatabaseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderDatabaseDialog));
            this.textBox_readerDbName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_inCirculation = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_libraryCode = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // textBox_readerDbName
            // 
            this.textBox_readerDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_readerDbName.Location = new System.Drawing.Point(104, 11);
            this.textBox_readerDbName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_readerDbName.Name = "textBox_readerDbName";
            this.textBox_readerDbName.Size = new System.Drawing.Size(182, 21);
            this.textBox_readerDbName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者库名(&R):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(212, 103);
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
            this.button_OK.Location = new System.Drawing.Point(133, 103);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_inCirculation
            // 
            this.checkBox_inCirculation.AutoSize = true;
            this.checkBox_inCirculation.Checked = true;
            this.checkBox_inCirculation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_inCirculation.Location = new System.Drawing.Point(11, 70);
            this.checkBox_inCirculation.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_inCirculation.Name = "checkBox_inCirculation";
            this.checkBox_inCirculation.Size = new System.Drawing.Size(90, 16);
            this.checkBox_inCirculation.TabIndex = 4;
            this.checkBox_inCirculation.Text = "参与流通(&C)";
            this.checkBox_inCirculation.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "图书馆代码(&L):";
            // 
            // comboBox_libraryCode
            // 
            this.comboBox_libraryCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_libraryCode.FormattingEnabled = true;
            this.comboBox_libraryCode.Location = new System.Drawing.Point(104, 38);
            this.comboBox_libraryCode.Name = "comboBox_libraryCode";
            this.comboBox_libraryCode.Size = new System.Drawing.Size(181, 20);
            this.comboBox_libraryCode.TabIndex = 3;
            // 
            // ReaderDatabaseDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(298, 135);
            this.Controls.Add(this.comboBox_libraryCode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBox_inCirculation);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_readerDbName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ReaderDatabaseDialog";
            this.ShowInTaskbar = false;
            this.Text = "读者库";
            this.Load += new System.EventHandler(this.ReaderDatabaseDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_readerDbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_inCirculation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_libraryCode;
    }
}