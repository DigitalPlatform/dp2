namespace dp2Installer
{
    partial class SetupMongoDbDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_dataDir = new System.Windows.Forms.TextBox();
            this.button_findDataDir = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_findBinDir = new System.Windows.Forms.Button();
            this.textBox_binDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据目录:";
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dataDir.Location = new System.Drawing.Point(12, 100);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(329, 21);
            this.textBox_dataDir.TabIndex = 1;
            // 
            // button_findDataDir
            // 
            this.button_findDataDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findDataDir.Location = new System.Drawing.Point(348, 100);
            this.button_findDataDir.Name = "button_findDataDir";
            this.button_findDataDir.Size = new System.Drawing.Size(51, 23);
            this.button_findDataDir.TabIndex = 2;
            this.button_findDataDir.Text = "...";
            this.button_findDataDir.UseVisualStyleBackColor = true;
            this.button_findDataDir.Click += new System.EventHandler(this.button_findDataDir_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(246, 255);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(327, 255);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_findBinDir
            // 
            this.button_findBinDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findBinDir.Location = new System.Drawing.Point(347, 27);
            this.button_findBinDir.Name = "button_findBinDir";
            this.button_findBinDir.Size = new System.Drawing.Size(51, 23);
            this.button_findBinDir.TabIndex = 7;
            this.button_findBinDir.Text = "...";
            this.button_findBinDir.UseVisualStyleBackColor = true;
            this.button_findBinDir.Click += new System.EventHandler(this.button_findBinDir_Click);
            // 
            // textBox_binDir
            // 
            this.textBox_binDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_binDir.Location = new System.Drawing.Point(12, 27);
            this.textBox_binDir.Name = "textBox_binDir";
            this.textBox_binDir.Size = new System.Drawing.Size(329, 21);
            this.textBox_binDir.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "mongod.exe 所在目录:";
            // 
            // SetupMongoDbDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(414, 290);
            this.Controls.Add(this.button_findBinDir);
            this.Controls.Add(this.textBox_binDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_findDataDir);
            this.Controls.Add(this.textBox_dataDir);
            this.Controls.Add(this.label1);
            this.Name = "SetupMongoDbDialog";
            this.Text = "SetupMongoDbDialog";
            this.Load += new System.EventHandler(this.SetupMongoDbDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dataDir;
        private System.Windows.Forms.Button button_findDataDir;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_findBinDir;
        private System.Windows.Forms.TextBox textBox_binDir;
        private System.Windows.Forms.Label label2;
    }
}