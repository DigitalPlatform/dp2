namespace DigitalPlatform
{
    partial class FileDownloadDialog
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label_message = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_bandwidth = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(22, 66);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(878, 24);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 5;
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(24, 21);
            this.label_message.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(876, 40);
            this.label_message.TabIndex = 4;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(392, 114);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(138, 40);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label_bandwidth
            // 
            this.label_bandwidth.AutoSize = true;
            this.label_bandwidth.Location = new System.Drawing.Point(18, 95);
            this.label_bandwidth.Name = "label_bandwidth";
            this.label_bandwidth.Size = new System.Drawing.Size(0, 21);
            this.label_bandwidth.TabIndex = 6;
            // 
            // FileDownloadDialog
            // 
            this.AcceptButton = this.button_cancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 177);
            this.Controls.Add(this.label_bandwidth);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_cancel);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "FileDownloadDialog";
            this.ShowIcon = false;
            this.Text = "FileDownloadDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label_bandwidth;
    }
}