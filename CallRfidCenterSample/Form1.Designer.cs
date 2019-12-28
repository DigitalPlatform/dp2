namespace CallRfidCenterSample
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
            this.button_tagListForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_tagListForm
            // 
            this.button_tagListForm.Location = new System.Drawing.Point(13, 13);
            this.button_tagListForm.Name = "button_tagListForm";
            this.button_tagListForm.Size = new System.Drawing.Size(422, 40);
            this.button_tagListForm.TabIndex = 0;
            this.button_tagListForm.Text = "打开 TagListForm";
            this.button_tagListForm.UseVisualStyleBackColor = true;
            this.button_tagListForm.Click += new System.EventHandler(this.button_tagListForm_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_tagListForm);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_tagListForm;
    }
}

