
namespace TestMySQL
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
            this.button_testConnectionString = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_testConnectionString
            // 
            this.button_testConnectionString.Location = new System.Drawing.Point(44, 189);
            this.button_testConnectionString.Name = "button_testConnectionString";
            this.button_testConnectionString.Size = new System.Drawing.Size(370, 48);
            this.button_testConnectionString.TabIndex = 0;
            this.button_testConnectionString.Text = "Test Connection String";
            this.button_testConnectionString.UseVisualStyleBackColor = true;
            this.button_testConnectionString.Click += new System.EventHandler(this.button_testConnectionString_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_testConnectionString);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_testConnectionString;
    }
}

