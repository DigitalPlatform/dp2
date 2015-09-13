namespace DigitalPlatform.CommonControl
{
    partial class WindowsUpdateDialog
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
            this.button_begin = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.button_testprogress = new System.Windows.Forms.Button();
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
            this.webBrowser1.Size = new System.Drawing.Size(520, 269);
            this.webBrowser1.TabIndex = 0;
            // 
            // button_begin
            // 
            this.button_begin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_begin.Location = new System.Drawing.Point(376, 287);
            this.button_begin.Name = "button_begin";
            this.button_begin.Size = new System.Drawing.Size(75, 23);
            this.button_begin.TabIndex = 1;
            this.button_begin.Text = "开始更新";
            this.button_begin.UseVisualStyleBackColor = true;
            this.button_begin.Click += new System.EventHandler(this.button_begin_Click);
            // 
            // button_close
            // 
            this.button_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_close.Location = new System.Drawing.Point(457, 287);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(75, 23);
            this.button_close.TabIndex = 2;
            this.button_close.Text = "关闭";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // button_testprogress
            // 
            this.button_testprogress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_testprogress.Location = new System.Drawing.Point(12, 287);
            this.button_testprogress.Name = "button_testprogress";
            this.button_testprogress.Size = new System.Drawing.Size(75, 23);
            this.button_testprogress.TabIndex = 3;
            this.button_testprogress.Text = "test progress";
            this.button_testprogress.UseVisualStyleBackColor = true;
            this.button_testprogress.Visible = false;
            this.button_testprogress.Click += new System.EventHandler(this.button_testprogress_Click);
            // 
            // WindowsUpdateDialog
            // 
            this.AcceptButton = this.button_begin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_close;
            this.ClientSize = new System.Drawing.Size(544, 322);
            this.Controls.Add(this.button_testprogress);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.button_begin);
            this.Controls.Add(this.webBrowser1);
            this.Name = "WindowsUpdateDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "安装 Windows 更新";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WindowsUpdateDialog_FormClosing);
            this.Load += new System.EventHandler(this.WindowsUpdateDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Button button_testprogress;
    }
}