namespace DigitalPlatform.Install
{
    partial class WcfBindingDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WcfBindingDlg));
            this.checkBox_nettcp = new System.Windows.Forms.CheckBox();
            this.textBox_nettcpUrl = new System.Windows.Forms.TextBox();
            this.textBox_netpipeUrl = new System.Windows.Forms.TextBox();
            this.checkBox_netpipe = new System.Windows.Forms.CheckBox();
            this.textBox_httpUrl = new System.Windows.Forms.TextBox();
            this.checkBox_http = new System.Windows.Forms.CheckBox();
            this.textBox_nettcpComment = new System.Windows.Forms.TextBox();
            this.textBox_netpipeComment = new System.Windows.Forms.TextBox();
            this.textBox_httpComment = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.textBox_restComment = new System.Windows.Forms.TextBox();
            this.textBox_restUrl = new System.Windows.Forms.TextBox();
            this.checkBox_rest = new System.Windows.Forms.CheckBox();
            this.panel_main = new System.Windows.Forms.Panel();
            this.textBox_basicComment = new System.Windows.Forms.TextBox();
            this.textBox_basicUrl = new System.Windows.Forms.TextBox();
            this.checkBox_basic = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox_https = new System.Windows.Forms.CheckBox();
            this.textBox_httpsComment = new System.Windows.Forms.TextBox();
            this.textBox_httpsUrl = new System.Windows.Forms.TextBox();
            this.panel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_nettcp
            // 
            this.checkBox_nettcp.AutoSize = true;
            this.checkBox_nettcp.Checked = true;
            this.checkBox_nettcp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_nettcp.Location = new System.Drawing.Point(0, 7);
            this.checkBox_nettcp.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_nettcp.Name = "checkBox_nettcp";
            this.checkBox_nettcp.Size = new System.Drawing.Size(113, 25);
            this.checkBox_nettcp.TabIndex = 0;
            this.checkBox_nettcp.Text = "NET.TCP";
            this.checkBox_nettcp.UseVisualStyleBackColor = true;
            this.checkBox_nettcp.CheckedChanged += new System.EventHandler(this.checkBox_nettcp_CheckedChanged);
            // 
            // textBox_nettcpUrl
            // 
            this.textBox_nettcpUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_nettcpUrl.Location = new System.Drawing.Point(174, 5);
            this.textBox_nettcpUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_nettcpUrl.Name = "textBox_nettcpUrl";
            this.textBox_nettcpUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_nettcpUrl.TabIndex = 1;
            // 
            // textBox_netpipeUrl
            // 
            this.textBox_netpipeUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_netpipeUrl.Location = new System.Drawing.Point(174, 136);
            this.textBox_netpipeUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_netpipeUrl.Name = "textBox_netpipeUrl";
            this.textBox_netpipeUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_netpipeUrl.TabIndex = 4;
            // 
            // checkBox_netpipe
            // 
            this.checkBox_netpipe.AutoSize = true;
            this.checkBox_netpipe.Checked = true;
            this.checkBox_netpipe.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_netpipe.Location = new System.Drawing.Point(0, 139);
            this.checkBox_netpipe.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_netpipe.Name = "checkBox_netpipe";
            this.checkBox_netpipe.Size = new System.Drawing.Size(124, 25);
            this.checkBox_netpipe.TabIndex = 3;
            this.checkBox_netpipe.Text = "NET.PIPE";
            this.checkBox_netpipe.UseVisualStyleBackColor = true;
            this.checkBox_netpipe.CheckedChanged += new System.EventHandler(this.checkBox_netpipe_CheckedChanged);
            // 
            // textBox_httpUrl
            // 
            this.textBox_httpUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_httpUrl.Location = new System.Drawing.Point(174, 275);
            this.textBox_httpUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_httpUrl.Name = "textBox_httpUrl";
            this.textBox_httpUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_httpUrl.TabIndex = 7;
            // 
            // checkBox_http
            // 
            this.checkBox_http.AutoSize = true;
            this.checkBox_http.Checked = true;
            this.checkBox_http.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_http.Location = new System.Drawing.Point(0, 278);
            this.checkBox_http.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_http.Name = "checkBox_http";
            this.checkBox_http.Size = new System.Drawing.Size(154, 25);
            this.checkBox_http.TabIndex = 6;
            this.checkBox_http.Text = "加密的 HTTP";
            this.checkBox_http.UseVisualStyleBackColor = true;
            this.checkBox_http.CheckedChanged += new System.EventHandler(this.checkBox_http_CheckedChanged);
            // 
            // textBox_nettcpComment
            // 
            this.textBox_nettcpComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_nettcpComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_nettcpComment.Location = new System.Drawing.Point(174, 45);
            this.textBox_nettcpComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_nettcpComment.Multiline = true;
            this.textBox_nettcpComment.Name = "textBox_nettcpComment";
            this.textBox_nettcpComment.ReadOnly = true;
            this.textBox_nettcpComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_nettcpComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_nettcpComment.TabIndex = 2;
            this.textBox_nettcpComment.Text = "速度较快。适用于Intranet和Internet";
            // 
            // textBox_netpipeComment
            // 
            this.textBox_netpipeComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_netpipeComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_netpipeComment.Location = new System.Drawing.Point(174, 184);
            this.textBox_netpipeComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_netpipeComment.Multiline = true;
            this.textBox_netpipeComment.Name = "textBox_netpipeComment";
            this.textBox_netpipeComment.ReadOnly = true;
            this.textBox_netpipeComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_netpipeComment.Size = new System.Drawing.Size(598, 77);
            this.textBox_netpipeComment.TabIndex = 5;
            this.textBox_netpipeComment.Text = "速度最快。但只能在本机内使用\r\n注意不要指定端口号";
            // 
            // textBox_httpComment
            // 
            this.textBox_httpComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_httpComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_httpComment.Location = new System.Drawing.Point(174, 322);
            this.textBox_httpComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_httpComment.Multiline = true;
            this.textBox_httpComment.Name = "textBox_httpComment";
            this.textBox_httpComment.ReadOnly = true;
            this.textBox_httpComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_httpComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_httpComment.TabIndex = 8;
            this.textBox_httpComment.Text = "速度最慢。适用于Intranet和Internet。利用证书对消息加密";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(546, 605);
            this.button_OK.Margin = new System.Windows.Forms.Padding(5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(137, 40);
            this.button_OK.TabIndex = 12;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(694, 605);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(137, 40);
            this.button_Cancel.TabIndex = 13;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // textBox_restComment
            // 
            this.textBox_restComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_restComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_restComment.Location = new System.Drawing.Point(174, 464);
            this.textBox_restComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_restComment.Multiline = true;
            this.textBox_restComment.Name = "textBox_restComment";
            this.textBox_restComment.ReadOnly = true;
            this.textBox_restComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_restComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_restComment.TabIndex = 11;
            this.textBox_restComment.Text = "轻量级WebService接口。适用于Intranet和Internet。主要供外部访问。";
            // 
            // textBox_restUrl
            // 
            this.textBox_restUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_restUrl.Location = new System.Drawing.Point(174, 416);
            this.textBox_restUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_restUrl.Name = "textBox_restUrl";
            this.textBox_restUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_restUrl.TabIndex = 10;
            // 
            // checkBox_rest
            // 
            this.checkBox_rest.AutoSize = true;
            this.checkBox_rest.Checked = true;
            this.checkBox_rest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_rest.Location = new System.Drawing.Point(0, 419);
            this.checkBox_rest.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_rest.Name = "checkBox_rest";
            this.checkBox_rest.Size = new System.Drawing.Size(135, 25);
            this.checkBox_rest.TabIndex = 9;
            this.checkBox_rest.Text = "REST.HTTP";
            this.checkBox_rest.UseVisualStyleBackColor = true;
            this.checkBox_rest.CheckedChanged += new System.EventHandler(this.checkBox_rest_CheckedChanged);
            // 
            // panel_main
            // 
            this.panel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_main.AutoScroll = true;
            this.panel_main.Controls.Add(this.checkBox_https);
            this.panel_main.Controls.Add(this.textBox_httpsComment);
            this.panel_main.Controls.Add(this.textBox_httpsUrl);
            this.panel_main.Controls.Add(this.panel1);
            this.panel_main.Controls.Add(this.textBox_basicComment);
            this.panel_main.Controls.Add(this.textBox_basicUrl);
            this.panel_main.Controls.Add(this.checkBox_basic);
            this.panel_main.Controls.Add(this.textBox_netpipeComment);
            this.panel_main.Controls.Add(this.textBox_restComment);
            this.panel_main.Controls.Add(this.checkBox_nettcp);
            this.panel_main.Controls.Add(this.textBox_restUrl);
            this.panel_main.Controls.Add(this.textBox_nettcpUrl);
            this.panel_main.Controls.Add(this.checkBox_rest);
            this.panel_main.Controls.Add(this.checkBox_netpipe);
            this.panel_main.Controls.Add(this.textBox_netpipeUrl);
            this.panel_main.Controls.Add(this.checkBox_http);
            this.panel_main.Controls.Add(this.textBox_httpComment);
            this.panel_main.Controls.Add(this.textBox_httpUrl);
            this.panel_main.Controls.Add(this.textBox_nettcpComment);
            this.panel_main.Location = new System.Drawing.Point(24, 21);
            this.panel_main.Margin = new System.Windows.Forms.Padding(5);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(809, 565);
            this.panel_main.TabIndex = 14;
            // 
            // textBox_basicComment
            // 
            this.textBox_basicComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_basicComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_basicComment.Location = new System.Drawing.Point(174, 607);
            this.textBox_basicComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_basicComment.Multiline = true;
            this.textBox_basicComment.Name = "textBox_basicComment";
            this.textBox_basicComment.ReadOnly = true;
            this.textBox_basicComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_basicComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_basicComment.TabIndex = 14;
            this.textBox_basicComment.Text = "轻量级WebService接口。适用于Intranet和Internet。主要供外部访问。";
            // 
            // textBox_basicUrl
            // 
            this.textBox_basicUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_basicUrl.Location = new System.Drawing.Point(174, 560);
            this.textBox_basicUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_basicUrl.Name = "textBox_basicUrl";
            this.textBox_basicUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_basicUrl.TabIndex = 13;
            // 
            // checkBox_basic
            // 
            this.checkBox_basic.AutoSize = true;
            this.checkBox_basic.Checked = true;
            this.checkBox_basic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_basic.Location = new System.Drawing.Point(0, 562);
            this.checkBox_basic.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_basic.Name = "checkBox_basic";
            this.checkBox_basic.Size = new System.Drawing.Size(146, 25);
            this.checkBox_basic.TabIndex = 12;
            this.checkBox_basic.Text = "BASIC.HTTP";
            this.checkBox_basic.UseVisualStyleBackColor = true;
            this.checkBox_basic.CheckedChanged += new System.EventHandler(this.checkBox_basic_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(541, 852);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(235, 48);
            this.panel1.TabIndex = 15;
            // 
            // checkBox_https
            // 
            this.checkBox_https.AutoSize = true;
            this.checkBox_https.Checked = true;
            this.checkBox_https.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_https.Location = new System.Drawing.Point(0, 713);
            this.checkBox_https.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_https.Name = "checkBox_https";
            this.checkBox_https.Size = new System.Drawing.Size(91, 25);
            this.checkBox_https.TabIndex = 16;
            this.checkBox_https.Text = "HTTPS";
            this.checkBox_https.UseVisualStyleBackColor = true;
            this.checkBox_https.CheckedChanged += new System.EventHandler(this.checkBox_https_CheckedChanged);
            // 
            // textBox_httpsComment
            // 
            this.textBox_httpsComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_httpsComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_httpsComment.Location = new System.Drawing.Point(174, 757);
            this.textBox_httpsComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_httpsComment.Multiline = true;
            this.textBox_httpsComment.Name = "textBox_httpsComment";
            this.textBox_httpsComment.ReadOnly = true;
            this.textBox_httpsComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_httpsComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_httpsComment.TabIndex = 18;
            this.textBox_httpsComment.Text = "适用于 Internet。利用 SSL 对 HTTP 传输加密";
            // 
            // textBox_httpsUrl
            // 
            this.textBox_httpsUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_httpsUrl.Location = new System.Drawing.Point(174, 710);
            this.textBox_httpsUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_httpsUrl.Name = "textBox_httpsUrl";
            this.textBox_httpsUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_httpsUrl.TabIndex = 17;
            // 
            // WcfBindingDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(854, 665);
            this.Controls.Add(this.panel_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "WcfBindingDlg";
            this.Text = "请选择服务器通讯协议";
            this.Load += new System.EventHandler(this.WcfBindingDlg_Load);
            this.panel_main.ResumeLayout(false);
            this.panel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_nettcp;
        private System.Windows.Forms.TextBox textBox_nettcpUrl;
        private System.Windows.Forms.TextBox textBox_netpipeUrl;
        private System.Windows.Forms.CheckBox checkBox_netpipe;
        private System.Windows.Forms.TextBox textBox_httpUrl;
        private System.Windows.Forms.CheckBox checkBox_http;
        private System.Windows.Forms.TextBox textBox_nettcpComment;
        private System.Windows.Forms.TextBox textBox_netpipeComment;
        private System.Windows.Forms.TextBox textBox_httpComment;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_restComment;
        private System.Windows.Forms.TextBox textBox_restUrl;
        private System.Windows.Forms.CheckBox checkBox_rest;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.TextBox textBox_basicComment;
        private System.Windows.Forms.TextBox textBox_basicUrl;
        private System.Windows.Forms.CheckBox checkBox_basic;
        private System.Windows.Forms.CheckBox checkBox_https;
        private System.Windows.Forms.TextBox textBox_httpsComment;
        private System.Windows.Forms.TextBox textBox_httpsUrl;
        private System.Windows.Forms.Panel panel1;
    }
}