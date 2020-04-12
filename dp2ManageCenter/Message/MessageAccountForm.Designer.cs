namespace dp2ManageCenter.Message
{
    partial class MessageAccountForm
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
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_accounts = new System.Windows.Forms.ListView();
            this.columnHeader_serverUrl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(686, 470);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(112, 40);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(566, 470);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 40);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_accounts
            // 
            this.listView_accounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_serverUrl,
            this.columnHeader_userName});
            this.listView_accounts.FullRowSelect = true;
            this.listView_accounts.HideSelection = false;
            this.listView_accounts.Location = new System.Drawing.Point(12, 12);
            this.listView_accounts.Name = "listView_accounts";
            this.listView_accounts.Size = new System.Drawing.Size(787, 452);
            this.listView_accounts.TabIndex = 4;
            this.listView_accounts.UseCompatibleStateImageBehavior = false;
            this.listView_accounts.View = System.Windows.Forms.View.Details;
            this.listView_accounts.DoubleClick += new System.EventHandler(this.listView_accounts_DoubleClick);
            this.listView_accounts.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_accounts_MouseUp);
            // 
            // columnHeader_serverUrl
            // 
            this.columnHeader_serverUrl.Text = "消息服务器 URL";
            this.columnHeader_serverUrl.Width = 313;
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 233;
            // 
            // MessageAccountForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(811, 522);
            this.Controls.Add(this.listView_accounts);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "MessageAccountForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置消息账户";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MessageAccountForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MessageAccountForm_FormClosed);
            this.Load += new System.EventHandler(this.MessageAccountForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ListView listView_accounts;
        private System.Windows.Forms.ColumnHeader columnHeader_serverUrl;
        private System.Windows.Forms.ColumnHeader columnHeader_userName;
    }
}