namespace dp2Catalog.DTLP
{
    partial class GenerateBatchFileDialog
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
            this.label_startPath = new System.Windows.Forms.Label();
            this.textBox_startPath = new System.Windows.Forms.TextBox();
            this.textBox_endPath = new System.Windows.Forms.TextBox();
            this.label_endPath = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(647, 391);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 10;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(647, 340);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label_startPath
            // 
            this.label_startPath.AutoSize = true;
            this.label_startPath.Location = new System.Drawing.Point(13, 30);
            this.label_startPath.Name = "label_startPath";
            this.label_startPath.Size = new System.Drawing.Size(105, 21);
            this.label_startPath.TabIndex = 11;
            this.label_startPath.Text = "起始路径:";
            // 
            // textBox_startPath
            // 
            this.textBox_startPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startPath.Location = new System.Drawing.Point(17, 54);
            this.textBox_startPath.Name = "textBox_startPath";
            this.textBox_startPath.Size = new System.Drawing.Size(768, 31);
            this.textBox_startPath.TabIndex = 12;
            // 
            // textBox_endPath
            // 
            this.textBox_endPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_endPath.Location = new System.Drawing.Point(17, 143);
            this.textBox_endPath.Name = "textBox_endPath";
            this.textBox_endPath.Size = new System.Drawing.Size(768, 31);
            this.textBox_endPath.TabIndex = 14;
            // 
            // label_endPath
            // 
            this.label_endPath.AutoSize = true;
            this.label_endPath.Location = new System.Drawing.Point(13, 119);
            this.label_endPath.Name = "label_endPath";
            this.label_endPath.Size = new System.Drawing.Size(105, 21);
            this.label_endPath.TabIndex = 13;
            this.label_endPath.Text = "结束路径:";
            // 
            // GenerateBatchFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox_endPath);
            this.Controls.Add(this.label_endPath);
            this.Controls.Add(this.textBox_startPath);
            this.Controls.Add(this.label_startPath);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "GenerateBatchFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "发生批控文件";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label_startPath;
        private System.Windows.Forms.TextBox textBox_startPath;
        private System.Windows.Forms.TextBox textBox_endPath;
        private System.Windows.Forms.Label label_endPath;
    }
}