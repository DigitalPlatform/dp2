namespace DigitalPlatform.LibraryServer
{
    partial class LibraryXmlDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_serverReplication = new System.Windows.Forms.TabPage();
            this.button_editMasterServer = new System.Windows.Forms.Button();
            this.textBox_masterServer = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_serverReplication.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(399, 291);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 19;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(318, 291);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 18;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_serverReplication);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(461, 272);
            this.tabControl1.TabIndex = 20;
            // 
            // tabPage_serverReplication
            // 
            this.tabPage_serverReplication.Controls.Add(this.button_editMasterServer);
            this.tabPage_serverReplication.Controls.Add(this.textBox_masterServer);
            this.tabPage_serverReplication.Controls.Add(this.label4);
            this.tabPage_serverReplication.Location = new System.Drawing.Point(4, 22);
            this.tabPage_serverReplication.Name = "tabPage_serverReplication";
            this.tabPage_serverReplication.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_serverReplication.Size = new System.Drawing.Size(453, 246);
            this.tabPage_serverReplication.TabIndex = 0;
            this.tabPage_serverReplication.Text = "服务器复制";
            this.tabPage_serverReplication.UseVisualStyleBackColor = true;
            // 
            // button_editMasterServer
            // 
            this.button_editMasterServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editMasterServer.Location = new System.Drawing.Point(402, 6);
            this.button_editMasterServer.Name = "button_editMasterServer";
            this.button_editMasterServer.Size = new System.Drawing.Size(45, 23);
            this.button_editMasterServer.TabIndex = 9;
            this.button_editMasterServer.Text = "...";
            this.button_editMasterServer.UseVisualStyleBackColor = true;
            this.button_editMasterServer.Click += new System.EventHandler(this.button_editMasterServer_Click);
            // 
            // textBox_masterServer
            // 
            this.textBox_masterServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_masterServer.Location = new System.Drawing.Point(102, 6);
            this.textBox_masterServer.Name = "textBox_masterServer";
            this.textBox_masterServer.ReadOnly = true;
            this.textBox_masterServer.Size = new System.Drawing.Size(294, 21);
            this.textBox_masterServer.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "主服务器(&M):";
            // 
            // LibraryXmlDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(486, 326);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "LibraryXmlDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "LibraryXmlDialog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LibraryXmlDialog_FormClosing);
            this.Load += new System.EventHandler(this.LibraryXmlDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_serverReplication.ResumeLayout(false);
            this.tabPage_serverReplication.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_serverReplication;
        private System.Windows.Forms.Button button_editMasterServer;
        private System.Windows.Forms.TextBox textBox_masterServer;
        private System.Windows.Forms.Label label4;
    }
}