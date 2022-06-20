
namespace dp2Catalog
{
    partial class SelectMarcXmlDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_unimarc = new System.Windows.Forms.ComboBox();
            this.comboBox_marc21 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(647, 396);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 10;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(647, 346);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 21);
            this.label1.TabIndex = 11;
            this.label1.Text = "UNIMARC 名字空间:";
            // 
            // comboBox_unimarc
            // 
            this.comboBox_unimarc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_unimarc.FormattingEnabled = true;
            this.comboBox_unimarc.Items.AddRange(new object[] {
            "dp2003 UNIMARC -- http://dp2003.com/UNIMARC",
            "MarcXChange -- info:lc/xmlns/marcxchange-v1"});
            this.comboBox_unimarc.Location = new System.Drawing.Point(12, 46);
            this.comboBox_unimarc.Name = "comboBox_unimarc";
            this.comboBox_unimarc.Size = new System.Drawing.Size(570, 29);
            this.comboBox_unimarc.TabIndex = 12;
            // 
            // comboBox_marc21
            // 
            this.comboBox_marc21.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_marc21.FormattingEnabled = true;
            this.comboBox_marc21.Items.AddRange(new object[] {
            "LOC slim -- http://www.loc.gov/MARC21/slim",
            "MarcXChange -- info:lc/xmlns/marcxchange-v1"});
            this.comboBox_marc21.Location = new System.Drawing.Point(12, 118);
            this.comboBox_marc21.Name = "comboBox_marc21";
            this.comboBox_marc21.Size = new System.Drawing.Size(570, 29);
            this.comboBox_marc21.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(182, 21);
            this.label2.TabIndex = 13;
            this.label2.Text = "MARC21 名字空间:";
            // 
            // SelectMarcXmlDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.comboBox_marc21);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_unimarc);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SelectMarcXmlDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请选择 MARCXML 格式";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_unimarc;
        private System.Windows.Forms.ComboBox comboBox_marc21;
        private System.Windows.Forms.Label label2;
    }
}