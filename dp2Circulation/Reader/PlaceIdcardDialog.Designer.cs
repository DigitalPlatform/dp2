namespace dp2Circulation
{
    partial class PlaceIdcardDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlaceIdcardDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.button_retry = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.checkBox_autoRetry = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(345, 41);
            this.label1.TabIndex = 0;
            this.label1.Text = "请把身份证放到读卡器上，并保持到操作完成...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // button_retry
            // 
            this.button_retry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_retry.Location = new System.Drawing.Point(200, 255);
            this.button_retry.Name = "button_retry";
            this.button_retry.Size = new System.Drawing.Size(75, 23);
            this.button_retry.TabIndex = 1;
            this.button_retry.Text = "重试";
            this.button_retry.UseVisualStyleBackColor = true;
            this.button_retry.Click += new System.EventHandler(this.button_retry_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(283, 255);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 2;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // checkBox_autoRetry
            // 
            this.checkBox_autoRetry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoRetry.AutoSize = true;
            this.checkBox_autoRetry.Location = new System.Drawing.Point(12, 262);
            this.checkBox_autoRetry.Name = "checkBox_autoRetry";
            this.checkBox_autoRetry.Size = new System.Drawing.Size(90, 16);
            this.checkBox_autoRetry.TabIndex = 3;
            this.checkBox_autoRetry.Text = "自动重试(&A)";
            this.checkBox_autoRetry.UseVisualStyleBackColor = true;
            this.checkBox_autoRetry.CheckedChanged += new System.EventHandler(this.checkBox_autoRetry_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(12, 53);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(345, 196);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 3000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // PlaceIdcardDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(369, 290);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_retry);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox_autoRetry);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PlaceIdcardDialog";
            this.ShowInTaskbar = false;
            this.Text = "从身份证导入信息";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PlaceIdcardDialog_FormClosed);
            this.Load += new System.EventHandler(this.PlaceIdcardDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_retry;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.CheckBox checkBox_autoRetry;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;

    }
}