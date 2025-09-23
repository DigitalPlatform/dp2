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
            this.textBox_resthttpComment = new System.Windows.Forms.TextBox();
            this.textBox_resthttpUrl = new System.Windows.Forms.TextBox();
            this.checkBox_resthttp = new System.Windows.Forms.CheckBox();
            this.panel_main = new System.Windows.Forms.Panel();
            this.checkBox_others = new System.Windows.Forms.CheckBox();
            this.textBox_othersComment = new System.Windows.Forms.TextBox();
            this.textBox_othersUrl = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox_basichttpComment = new System.Windows.Forms.TextBox();
            this.textBox_basichttpUrl = new System.Windows.Forms.TextBox();
            this.checkBox_basichttp = new System.Windows.Forms.CheckBox();
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
            this.textBox_netpipeUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
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
            this.textBox_httpUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
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
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
            // textBox_resthttpComment
            // 
            this.textBox_resthttpComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_resthttpComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_resthttpComment.Location = new System.Drawing.Point(174, 464);
            this.textBox_resthttpComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_resthttpComment.Multiline = true;
            this.textBox_resthttpComment.Name = "textBox_resthttpComment";
            this.textBox_resthttpComment.ReadOnly = true;
            this.textBox_resthttpComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_resthttpComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_resthttpComment.TabIndex = 11;
            this.textBox_resthttpComment.Text = "轻量级WebService接口。适用于Intranet和Internet。主要供外部访问。";
            // 
            // textBox_resthttpUrl
            // 
            this.textBox_resthttpUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_resthttpUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_resthttpUrl.Location = new System.Drawing.Point(174, 416);
            this.textBox_resthttpUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_resthttpUrl.Name = "textBox_resthttpUrl";
            this.textBox_resthttpUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_resthttpUrl.TabIndex = 10;
            // 
            // checkBox_resthttp
            // 
            this.checkBox_resthttp.AutoSize = true;
            this.checkBox_resthttp.Checked = true;
            this.checkBox_resthttp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_resthttp.Location = new System.Drawing.Point(0, 419);
            this.checkBox_resthttp.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_resthttp.Name = "checkBox_resthttp";
            this.checkBox_resthttp.Size = new System.Drawing.Size(135, 25);
            this.checkBox_resthttp.TabIndex = 9;
            this.checkBox_resthttp.Text = "REST.HTTP";
            this.checkBox_resthttp.UseVisualStyleBackColor = true;
            this.checkBox_resthttp.CheckedChanged += new System.EventHandler(this.checkBox_rest_CheckedChanged);
            // 
            // panel_main
            // 
            this.panel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_main.AutoScroll = true;
            this.panel_main.Controls.Add(this.checkBox_others);
            this.panel_main.Controls.Add(this.textBox_othersComment);
            this.panel_main.Controls.Add(this.textBox_othersUrl);
            this.panel_main.Controls.Add(this.panel1);
            this.panel_main.Controls.Add(this.textBox_basichttpComment);
            this.panel_main.Controls.Add(this.textBox_basichttpUrl);
            this.panel_main.Controls.Add(this.checkBox_basichttp);
            this.panel_main.Controls.Add(this.textBox_netpipeComment);
            this.panel_main.Controls.Add(this.textBox_resthttpComment);
            this.panel_main.Controls.Add(this.checkBox_nettcp);
            this.panel_main.Controls.Add(this.textBox_resthttpUrl);
            this.panel_main.Controls.Add(this.textBox_nettcpUrl);
            this.panel_main.Controls.Add(this.checkBox_resthttp);
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
            // checkBox_others
            // 
            this.checkBox_others.AutoSize = true;
            this.checkBox_others.Checked = true;
            this.checkBox_others.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_others.Location = new System.Drawing.Point(0, 713);
            this.checkBox_others.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_others.Name = "checkBox_others";
            this.checkBox_others.Size = new System.Drawing.Size(78, 25);
            this.checkBox_others.TabIndex = 16;
            this.checkBox_others.Text = "其它";
            this.checkBox_others.UseVisualStyleBackColor = true;
            this.checkBox_others.CheckedChanged += new System.EventHandler(this.checkBox_others_CheckedChanged);
            // 
            // textBox_othersComment
            // 
            this.textBox_othersComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_othersComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_othersComment.Location = new System.Drawing.Point(174, 757);
            this.textBox_othersComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_othersComment.Multiline = true;
            this.textBox_othersComment.Name = "textBox_othersComment";
            this.textBox_othersComment.ReadOnly = true;
            this.textBox_othersComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_othersComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_othersComment.TabIndex = 18;
            this.textBox_othersComment.Text = "其它。";
            // 
            // textBox_othersUrl
            // 
            this.textBox_othersUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_othersUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_othersUrl.Location = new System.Drawing.Point(174, 710);
            this.textBox_othersUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_othersUrl.Name = "textBox_othersUrl";
            this.textBox_othersUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_othersUrl.TabIndex = 17;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(541, 852);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(235, 48);
            this.panel1.TabIndex = 15;
            // 
            // textBox_basichttpComment
            // 
            this.textBox_basichttpComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_basichttpComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_basichttpComment.Location = new System.Drawing.Point(174, 607);
            this.textBox_basichttpComment.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_basichttpComment.Multiline = true;
            this.textBox_basichttpComment.Name = "textBox_basichttpComment";
            this.textBox_basichttpComment.ReadOnly = true;
            this.textBox_basichttpComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_basichttpComment.Size = new System.Drawing.Size(604, 77);
            this.textBox_basichttpComment.TabIndex = 14;
            this.textBox_basichttpComment.Text = "轻量级WebService接口。适用于Intranet和Internet。主要供外部访问。";
            // 
            // textBox_basichttpUrl
            // 
            this.textBox_basichttpUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_basichttpUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_basichttpUrl.Location = new System.Drawing.Point(174, 560);
            this.textBox_basichttpUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_basichttpUrl.Name = "textBox_basichttpUrl";
            this.textBox_basichttpUrl.Size = new System.Drawing.Size(600, 31);
            this.textBox_basichttpUrl.TabIndex = 13;
            // 
            // checkBox_basichttp
            // 
            this.checkBox_basichttp.AutoSize = true;
            this.checkBox_basichttp.Checked = true;
            this.checkBox_basichttp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_basichttp.Location = new System.Drawing.Point(0, 562);
            this.checkBox_basichttp.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_basichttp.Name = "checkBox_basichttp";
            this.checkBox_basichttp.Size = new System.Drawing.Size(146, 25);
            this.checkBox_basichttp.TabIndex = 12;
            this.checkBox_basichttp.Text = "BASIC.HTTP";
            this.checkBox_basichttp.UseVisualStyleBackColor = true;
            this.checkBox_basichttp.CheckedChanged += new System.EventHandler(this.checkBox_basic_CheckedChanged);
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
        private System.Windows.Forms.TextBox textBox_resthttpComment;
        private System.Windows.Forms.TextBox textBox_resthttpUrl;
        private System.Windows.Forms.CheckBox checkBox_resthttp;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.TextBox textBox_basichttpComment;
        private System.Windows.Forms.TextBox textBox_basichttpUrl;
        private System.Windows.Forms.CheckBox checkBox_basichttp;
        private System.Windows.Forms.CheckBox checkBox_others;
        private System.Windows.Forms.TextBox textBox_othersComment;
        private System.Windows.Forms.TextBox textBox_othersUrl;
        private System.Windows.Forms.Panel panel1;
    }
}