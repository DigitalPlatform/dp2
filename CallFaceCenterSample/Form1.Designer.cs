namespace CallFaceCenterSample
{
    partial class Form1
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
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button_faceRecognition1 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button_startVideo = new System.Windows.Forms.Button();
            this.button_stopVideo = new System.Windows.Forms.Button();
            this.button_faceRecognition2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_faceRecognition1
            // 
            this.button_faceRecognition1.Location = new System.Drawing.Point(12, 24);
            this.button_faceRecognition1.Name = "button_faceRecognition1";
            this.button_faceRecognition1.Size = new System.Drawing.Size(545, 45);
            this.button_faceRecognition1.TabIndex = 0;
            this.button_faceRecognition1.Text = "人脸识别1(利用 FaceCenter 窗口)";
            this.button_faceRecognition1.UseVisualStyleBackColor = true;
            this.button_faceRecognition1.Click += new System.EventHandler(this.button_faceRecognition1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(13, 168);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(328, 270);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // button_startVideo
            // 
            this.button_startVideo.Location = new System.Drawing.Point(371, 168);
            this.button_startVideo.Name = "button_startVideo";
            this.button_startVideo.Size = new System.Drawing.Size(144, 48);
            this.button_startVideo.TabIndex = 2;
            this.button_startVideo.Text = "开启视频";
            this.button_startVideo.UseVisualStyleBackColor = true;
            this.button_startVideo.Click += new System.EventHandler(this.button_startVideo_Click);
            // 
            // button_stopVideo
            // 
            this.button_stopVideo.Location = new System.Drawing.Point(371, 222);
            this.button_stopVideo.Name = "button_stopVideo";
            this.button_stopVideo.Size = new System.Drawing.Size(144, 48);
            this.button_stopVideo.TabIndex = 3;
            this.button_stopVideo.Text = "关闭视频";
            this.button_stopVideo.UseVisualStyleBackColor = true;
            this.button_stopVideo.Click += new System.EventHandler(this.button_stopVideo_Click);
            // 
            // button_faceRecognition2
            // 
            this.button_faceRecognition2.Location = new System.Drawing.Point(13, 75);
            this.button_faceRecognition2.Name = "button_faceRecognition2";
            this.button_faceRecognition2.Size = new System.Drawing.Size(545, 45);
            this.button_faceRecognition2.TabIndex = 4;
            this.button_faceRecognition2.Text = "人脸识别2(利用本程序窗口)";
            this.button_faceRecognition2.UseVisualStyleBackColor = true;
            this.button_faceRecognition2.Click += new System.EventHandler(this.button_faceRecognition2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_faceRecognition2);
            this.Controls.Add(this.button_stopVideo);
            this.Controls.Add(this.button_startVideo);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_faceRecognition1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_faceRecognition1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_startVideo;
        private System.Windows.Forms.Button button_stopVideo;
        private System.Windows.Forms.Button button_faceRecognition2;
    }
}

