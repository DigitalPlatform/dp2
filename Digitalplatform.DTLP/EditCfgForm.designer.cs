namespace DigitalPlatform.DTLP
{
    partial class EditCfgForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCfgForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_cfgPath = new System.Windows.Forms.TextBox();
            this.textBox_content = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件路径(&P):";
            // 
            // textBox_cfgPath
            // 
            this.textBox_cfgPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cfgPath.Location = new System.Drawing.Point(117, 12);
            this.textBox_cfgPath.Name = "textBox_cfgPath";
            this.textBox_cfgPath.Size = new System.Drawing.Size(468, 25);
            this.textBox_cfgPath.TabIndex = 1;
            this.textBox_cfgPath.TextChanged += new System.EventHandler(this.textBox_cfgPath_TextChanged);
            // 
            // textBox_content
            // 
            this.textBox_content.AcceptsReturn = true;
            this.textBox_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_content.HideSelection = false;
            this.textBox_content.Location = new System.Drawing.Point(12, 67);
            this.textBox_content.Multiline = true;
            this.textBox_content.Name = "textBox_content";
            this.textBox_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_content.Size = new System.Drawing.Size(573, 221);
            this.textBox_content.TabIndex = 2;
            this.textBox_content.TextChanged += new System.EventHandler(this.textBox_content_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "内容(&C):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(429, 294);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(510, 294);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // EditCfgForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(597, 334);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_content);
            this.Controls.Add(this.textBox_cfgPath);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditCfgForm";
            this.ShowInTaskbar = false;
            this.Text = "编辑配置文件";
            this.Load += new System.EventHandler(this.EditCfgForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_cfgPath;
        private System.Windows.Forms.TextBox textBox_content;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}