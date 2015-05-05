namespace dp2Circulation
{
    partial class IssuePublishTimeFoundDupDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IssuePublishTimeFoundDupDlg));
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.webBrowser_issue = new System.Windows.Forms.WebBrowser();
            this.webBrowser_biblio = new System.Windows.Forms.WebBrowser();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(495, 362);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "È·¶¨";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(12, 12);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(557, 80);
            this.textBox_message.TabIndex = 5;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(12, 111);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.webBrowser_issue);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser_biblio);
            this.splitContainer_main.Size = new System.Drawing.Size(557, 244);
            this.splitContainer_main.SplitterDistance = 206;
            this.splitContainer_main.SplitterWidth = 5;
            this.splitContainer_main.TabIndex = 4;
            // 
            // webBrowser_issue
            // 
            this.webBrowser_issue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_issue.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_issue.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_issue.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser_issue.Name = "webBrowser_issue";
            this.webBrowser_issue.Size = new System.Drawing.Size(206, 244);
            this.webBrowser_issue.TabIndex = 0;
            // 
            // webBrowser_biblio
            // 
            this.webBrowser_biblio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_biblio.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_biblio.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_biblio.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser_biblio.Name = "webBrowser_biblio";
            this.webBrowser_biblio.Size = new System.Drawing.Size(346, 244);
            this.webBrowser_biblio.TabIndex = 0;
            // 
            // IssuePublishTimeFoundDupDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 403);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IssuePublishTimeFoundDupDlg";
            this.ShowInTaskbar = false;
            this.Text = "IssuePublishTimeFoundDupDlg";
            this.Load += new System.EventHandler(this.IssuePublishTimeFoundDupDlg_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser_issue;
        private System.Windows.Forms.WebBrowser webBrowser_biblio;
    }
}