namespace DigitalPlatform.LibraryServer
{
    partial class CertificateDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CertificateDialog));
            this.button_viewCurrentCert = new System.Windows.Forms.Button();
            this.button_selectCert = new System.Windows.Forms.Button();
            this.button_clearSelection = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_viewCurrentCert
            // 
            this.button_viewCurrentCert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.button_viewCurrentCert.Location = new System.Drawing.Point(12, 13);
            this.button_viewCurrentCert.Name = "button_viewCurrentCert";
            this.button_viewCurrentCert.Size = new System.Drawing.Size(297, 23);
            this.button_viewCurrentCert.TabIndex = 0;
            this.button_viewCurrentCert.Text = "察看当前已选证书...";
            this.button_viewCurrentCert.UseVisualStyleBackColor = true;
            this.button_viewCurrentCert.Click += new System.EventHandler(this.button_viewCurrentCert_Click);
            // 
            // button_selectCert
            // 
            this.button_selectCert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.button_selectCert.Location = new System.Drawing.Point(12, 42);
            this.button_selectCert.Name = "button_selectCert";
            this.button_selectCert.Size = new System.Drawing.Size(297, 23);
            this.button_selectCert.TabIndex = 1;
            this.button_selectCert.Text = "选择一个证书...";
            this.button_selectCert.UseVisualStyleBackColor = true;
            this.button_selectCert.Click += new System.EventHandler(this.button_selectCert_Click);
            // 
            // button_clearSelection
            // 
            this.button_clearSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.button_clearSelection.Location = new System.Drawing.Point(12, 71);
            this.button_clearSelection.Name = "button_clearSelection";
            this.button_clearSelection.Size = new System.Drawing.Size(297, 23);
            this.button_clearSelection.TabIndex = 2;
            this.button_clearSelection.Text = "清除选择";
            this.button_clearSelection.UseVisualStyleBackColor = true;
            this.button_clearSelection.Click += new System.EventHandler(this.button_clearSelection_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(234, 142);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 15;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(153, 142);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 14;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // CertificateDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(321, 177);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_clearSelection);
            this.Controls.Add(this.button_selectCert);
            this.Controls.Add(this.button_viewCurrentCert);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CertificateDialog";
            this.ShowInTaskbar = false;
            this.Text = "证书";
            this.Load += new System.EventHandler(this.CertificateDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_viewCurrentCert;
        private System.Windows.Forms.Button button_selectCert;
        private System.Windows.Forms.Button button_clearSelection;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}