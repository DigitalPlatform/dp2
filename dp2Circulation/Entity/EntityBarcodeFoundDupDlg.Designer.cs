namespace dp2Circulation
{
    partial class EntityBarcodeFoundDupDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityBarcodeFoundDupDlg));
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.webBrowser_item = new System.Windows.Forms.WebBrowser();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(10, 90);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.webBrowser_item);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser_biblio);
            this.splitContainer_main.Size = new System.Drawing.Size(418, 195);
            this.splitContainer_main.SplitterDistance = 154;
            this.splitContainer_main.TabIndex = 1;
            // 
            // webBrowser_item
            // 
            this.webBrowser_item.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_item.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_item.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_item.Name = "webBrowser_item";
            this.webBrowser_item.Size = new System.Drawing.Size(154, 195);
            this.webBrowser_item.TabIndex = 0;
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(260, 195);
            this.webBrowser_biblio.TabIndex = 0;
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(10, 10);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(418, 64);
            this.textBox_message.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(372, 290);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // EntityBarcodeFoundDupDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 322);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "EntityBarcodeFoundDupDlg";
            this.ShowInTaskbar = false;
            this.Text = "册条码号发现有重复";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EntityBarcodeFoundDupDlg_FormClosed);
            this.Load += new System.EventHandler(this.EntityBarcodeFoundDupDlg_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser_item;
        private System.Windows.Forms.WebBrowser webBrowser_biblio;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_OK;
    }
}