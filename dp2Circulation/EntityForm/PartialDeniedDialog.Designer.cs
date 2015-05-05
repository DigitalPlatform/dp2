namespace dp2Circulation
{
    partial class PartialDeniedDialog
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_loadSaved = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(12, 12);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(486, 211);
            this.webBrowser1.TabIndex = 0;
            // 
            // button_loadSaved
            // 
            this.button_loadSaved.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_loadSaved.Location = new System.Drawing.Point(250, 229);
            this.button_loadSaved.Name = "button_loadSaved";
            this.button_loadSaved.Size = new System.Drawing.Size(167, 23);
            this.button_loadSaved.TabIndex = 1;
            this.button_loadSaved.Text = "装入实际保存后的记录";
            this.button_loadSaved.UseVisualStyleBackColor = true;
            this.button_loadSaved.Click += new System.EventHandler(this.button_loadSaved_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(423, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "返回";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // PartialDeniedDialog
            // 
            this.AcceptButton = this.button_loadSaved;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(510, 264);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_loadSaved);
            this.Controls.Add(this.webBrowser1);
            this.Name = "PartialDeniedDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "部分字段被拒绝";
            this.Load += new System.EventHandler(this.PartialDeniedDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button_loadSaved;
        private System.Windows.Forms.Button button_Cancel;
    }
}