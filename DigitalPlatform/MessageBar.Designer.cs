namespace DigitalPlatform
{
    partial class MessageBar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBar));
            this.label_message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_message
            // 
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Location = new System.Drawing.Point(0, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(502, 84);
            this.label_message.TabIndex = 0;
            this.label_message.Text = "ÇëµÈ´ý...";
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessageBar
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 84);
            this.ControlBox = false;
            this.Controls.Add(this.label_message);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageBar";
            this.ShowInTaskbar = false;
            this.Text = "MessageBar";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_message;
    }
}