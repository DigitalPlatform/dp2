namespace DigitalPlatform.Drawing
{
    partial class QrRecognitionControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            DisposeResource(disposing);

            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cmbDevice = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel_camera = new System.Windows.Forms.Panel();
            this.progressBar1 = new DigitalPlatform.GUI.VeritalProgressBar();
            this.label_message = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel_camera.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(318, 287);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // cmbDevice
            // 
            this.cmbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDevice.FormattingEnabled = true;
            this.cmbDevice.Location = new System.Drawing.Point(66, 0);
            this.cmbDevice.MaximumSize = new System.Drawing.Size(200, 0);
            this.cmbDevice.Name = "cmbDevice";
            this.cmbDevice.Size = new System.Drawing.Size(200, 20);
            this.cmbDevice.TabIndex = 4;
            this.cmbDevice.SelectedIndexChanged += new System.EventHandler(this.cmbDevice_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "摄像头(&C):";
            // 
            // panel_camera
            // 
            this.panel_camera.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_camera.Controls.Add(this.progressBar1);
            this.panel_camera.Controls.Add(this.cmbDevice);
            this.panel_camera.Controls.Add(this.label1);
            this.panel_camera.Location = new System.Drawing.Point(3, 4);
            this.panel_camera.Name = "panel_camera";
            this.panel_camera.Size = new System.Drawing.Size(318, 23);
            this.panel_camera.TabIndex = 6;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(272, 0);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(14, 20);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // label_message
            // 
            this.label_message.BackColor = System.Drawing.Color.Transparent;
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label_message.Location = new System.Drawing.Point(0, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(318, 287);
            this.label_message.TabIndex = 7;
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_message.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.label_message);
            this.panel1.Location = new System.Drawing.Point(3, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(318, 287);
            this.panel1.TabIndex = 8;
            // 
            // QrRecognitionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel_camera);
            this.Name = "QrRecognitionControl";
            this.Size = new System.Drawing.Size(324, 317);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel_camera.ResumeLayout(false);
            this.panel_camera.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox cmbDevice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel_camera;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Panel panel1;
        private DigitalPlatform.GUI.VeritalProgressBar progressBar1;
    }
}
