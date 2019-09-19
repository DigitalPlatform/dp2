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
            this.SuspendLayout();
            // 
            // button_faceRecognition1
            // 
            this.button_faceRecognition1.Location = new System.Drawing.Point(13, 81);
            this.button_faceRecognition1.Name = "button_faceRecognition1";
            this.button_faceRecognition1.Size = new System.Drawing.Size(545, 45);
            this.button_faceRecognition1.TabIndex = 0;
            this.button_faceRecognition1.Text = "人脸识别1(利用 FaceCenter 窗口)";
            this.button_faceRecognition1.UseVisualStyleBackColor = true;
            this.button_faceRecognition1.Click += new System.EventHandler(this.button_faceRecognition1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_faceRecognition1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_faceRecognition1;
    }
}

