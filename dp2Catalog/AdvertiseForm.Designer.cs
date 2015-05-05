namespace dp2Catalog
{
    partial class AdvertiseForm
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
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(350, 264);
            this.webBrowser1.TabIndex = 0;
            // 
            // AdvertiseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 264);
            this.Controls.Add(this.webBrowser1);
            this.Name = "AdvertiseForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "广告";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdvertiseForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AdvertiseForm_FormClosed);
            this.Load += new System.EventHandler(this.AdvertiseForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}