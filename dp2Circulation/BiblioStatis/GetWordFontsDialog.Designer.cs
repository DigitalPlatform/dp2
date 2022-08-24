namespace dp2Circulation
{
    partial class GetWordFontsDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_ascii = new System.Windows.Forms.ComboBox();
            this.comboBox_hAnsi = new System.Windows.Forms.ComboBox();
            this.comboBox_eastAsia = new System.Windows.Forms.ComboBox();
            this.comboBox_cs = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(670, 401);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(118, 37);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(546, 401);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(118, 37);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "ascii:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 21);
            this.label2.TabIndex = 5;
            this.label2.Text = "hAnsi:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "eastAsia:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 207);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "cs:";
            // 
            // comboBox_ascII
            // 
            this.comboBox_ascii.FormattingEnabled = true;
            this.comboBox_ascii.Location = new System.Drawing.Point(197, 47);
            this.comboBox_ascii.Name = "comboBox_ascII";
            this.comboBox_ascii.Size = new System.Drawing.Size(411, 29);
            this.comboBox_ascii.TabIndex = 8;
            // 
            // comboBox_hAnsi
            // 
            this.comboBox_hAnsi.FormattingEnabled = true;
            this.comboBox_hAnsi.Location = new System.Drawing.Point(197, 98);
            this.comboBox_hAnsi.Name = "comboBox_hAnsi";
            this.comboBox_hAnsi.Size = new System.Drawing.Size(411, 29);
            this.comboBox_hAnsi.TabIndex = 9;
            // 
            // comboBox_eastAsia
            // 
            this.comboBox_eastAsia.FormattingEnabled = true;
            this.comboBox_eastAsia.Location = new System.Drawing.Point(197, 147);
            this.comboBox_eastAsia.Name = "comboBox_eastAsia";
            this.comboBox_eastAsia.Size = new System.Drawing.Size(411, 29);
            this.comboBox_eastAsia.TabIndex = 10;
            // 
            // comboBox_cs
            // 
            this.comboBox_cs.FormattingEnabled = true;
            this.comboBox_cs.Location = new System.Drawing.Point(197, 204);
            this.comboBox_cs.Name = "comboBox_cs";
            this.comboBox_cs.Size = new System.Drawing.Size(411, 29);
            this.comboBox_cs.TabIndex = 11;
            // 
            // GetWordFontsDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.comboBox_cs);
            this.Controls.Add(this.comboBox_eastAsia);
            this.Controls.Add(this.comboBox_hAnsi);
            this.Controls.Add(this.comboBox_ascii);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "GetWordFontsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "指定字体";
            this.Load += new System.EventHandler(this.GetWordFontsDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_ascii;
        private System.Windows.Forms.ComboBox comboBox_hAnsi;
        private System.Windows.Forms.ComboBox comboBox_eastAsia;
        private System.Windows.Forms.ComboBox comboBox_cs;
    }
}