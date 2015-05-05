namespace DigitalPlatform.dp2.Statis
{
    partial class HtmlInputDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HtmlInputDialog));
            this.extendedWebBrowser1 = new DigitalPlatform.dp2.Statis.ExtendedWebBrowser();
            this.SuspendLayout();
            // 
            // extendedWebBrowser1
            // 
            this.extendedWebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extendedWebBrowser1.Location = new System.Drawing.Point(0, 0);
            this.extendedWebBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.extendedWebBrowser1.Name = "extendedWebBrowser1";
            this.extendedWebBrowser1.Size = new System.Drawing.Size(292, 272);
            this.extendedWebBrowser1.TabIndex = 0;
            this.extendedWebBrowser1.BeforeNavigate += new DigitalPlatform.dp2.Statis.BeforeNavigateEventHandler(this.extendedWebBrowser1_BeforeNavigate);
            // 
            // HtmlInputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 272);
            this.Controls.Add(this.extendedWebBrowser1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HtmlInputDialog";
            this.ShowInTaskbar = false;
            this.Text = "HtmlInputDialog";
            this.Load += new System.EventHandler(this.HtmlInputDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ExtendedWebBrowser extendedWebBrowser1;
    }
}