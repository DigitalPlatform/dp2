
namespace RfidTool
{
    partial class FloatingErrorDialog
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
            this.label_text = new System.Windows.Forms.Label();
            this.button_close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_text
            // 
            this.label_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_text.BackColor = System.Drawing.Color.DarkRed;
            this.label_text.Font = new System.Drawing.Font("微软雅黑", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_text.ForeColor = System.Drawing.Color.White;
            this.label_text.Location = new System.Drawing.Point(12, 13);
            this.label_text.Name = "label_text";
            this.label_text.Size = new System.Drawing.Size(776, 361);
            this.label_text.TabIndex = 3;
            this.label_text.Text = "...";
            this.label_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_close
            // 
            this.button_close.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_close.Location = new System.Drawing.Point(308, 387);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(185, 51);
            this.button_close.TabIndex = 2;
            this.button_close.Text = "关闭";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // FloatingErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label_text);
            this.Controls.Add(this.button_close);
            this.MinimizeBox = false;
            this.Name = "FloatingErrorDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "错误信息";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_text;
        private System.Windows.Forms.Button button_close;
    }
}