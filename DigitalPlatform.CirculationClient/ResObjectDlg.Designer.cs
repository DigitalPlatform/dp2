namespace DigitalPlatform.CirculationClient
{
    partial class ResObjectDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResObjectDlg));
            this.textBox_timestamp = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_mime = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_size = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_findLocalPath = new System.Windows.Forms.Button();
            this.textBox_localPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_state = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serverName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_usage = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_timestamp
            // 
            this.textBox_timestamp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_timestamp.Location = new System.Drawing.Point(112, 229);
            this.textBox_timestamp.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_timestamp.Name = "textBox_timestamp";
            this.textBox_timestamp.ReadOnly = true;
            this.textBox_timestamp.Size = new System.Drawing.Size(188, 21);
            this.textBox_timestamp.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 231);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "时间戳(&T):";
            // 
            // textBox_mime
            // 
            this.textBox_mime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_mime.Location = new System.Drawing.Point(112, 204);
            this.textBox_mime.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_mime.Name = "textBox_mime";
            this.textBox_mime.Size = new System.Drawing.Size(188, 21);
            this.textBox_mime.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 206);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "媒体类型(&M):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(331, 229);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 16;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(331, 205);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 15;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_size
            // 
            this.textBox_size.Location = new System.Drawing.Point(112, 179);
            this.textBox_size.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_size.Name = "textBox_size";
            this.textBox_size.ReadOnly = true;
            this.textBox_size.Size = new System.Drawing.Size(102, 21);
            this.textBox_size.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 181);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "尺寸(&S):";
            // 
            // button_findLocalPath
            // 
            this.button_findLocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findLocalPath.Location = new System.Drawing.Point(379, 96);
            this.button_findLocalPath.Margin = new System.Windows.Forms.Padding(2);
            this.button_findLocalPath.Name = "button_findLocalPath";
            this.button_findLocalPath.Size = new System.Drawing.Size(32, 22);
            this.button_findLocalPath.TabIndex = 6;
            this.button_findLocalPath.Text = "...";
            this.button_findLocalPath.Click += new System.EventHandler(this.button_findLocalPath_Click);
            // 
            // textBox_localPath
            // 
            this.textBox_localPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_localPath.Location = new System.Drawing.Point(9, 96);
            this.textBox_localPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_localPath.Name = "textBox_localPath";
            this.textBox_localPath.Size = new System.Drawing.Size(366, 21);
            this.textBox_localPath.TabIndex = 5;
            this.textBox_localPath.TextChanged += new System.EventHandler(this.textBox_localPath_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 82);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "本地物理路径(&P):";
            // 
            // textBox_state
            // 
            this.textBox_state.Location = new System.Drawing.Point(112, 34);
            this.textBox_state.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_state.Name = "textBox_state";
            this.textBox_state.ReadOnly = true;
            this.textBox_state.Size = new System.Drawing.Size(102, 21);
            this.textBox_state.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "状态(&S):";
            // 
            // textBox_serverName
            // 
            this.textBox_serverName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox_serverName.Location = new System.Drawing.Point(112, 10);
            this.textBox_serverName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_serverName.Name = "textBox_serverName";
            this.textBox_serverName.ReadOnly = true;
            this.textBox_serverName.Size = new System.Drawing.Size(300, 21);
            this.textBox_serverName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "资源ID (&I):";
            // 
            // textBox_usage
            // 
            this.textBox_usage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_usage.Location = new System.Drawing.Point(112, 139);
            this.textBox_usage.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_usage.Name = "textBox_usage";
            this.textBox_usage.Size = new System.Drawing.Size(188, 21);
            this.textBox_usage.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 141);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "用途(&U):";
            // 
            // ResObjectDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 261);
            this.Controls.Add(this.textBox_usage);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_timestamp);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_mime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_size);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_findLocalPath);
            this.Controls.Add(this.textBox_localPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_state);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_serverName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ResObjectDlg";
            this.ShowInTaskbar = false;
            this.Text = "资源对象";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox textBox_timestamp;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox textBox_mime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        public System.Windows.Forms.TextBox textBox_size;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_findLocalPath;
        public System.Windows.Forms.TextBox textBox_localPath;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox_state;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox_serverName;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBox_usage;
        private System.Windows.Forms.Label label7;
    }
}