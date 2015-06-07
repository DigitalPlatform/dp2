namespace dp2Circulation
{
    partial class LineLayerForm
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
            this.SuspendLayout();
            // 
            // LineLayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 269);
            this.DoubleBuffered = true;
            this.Name = "LineLayerForm";
            this.Opacity = 0.45D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "LineLayerForm";
            this.Activated += new System.EventHandler(this.LineLayerForm_Activated);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LineLayerForm_Paint);
            this.ResumeLayout(false);

        }

        #endregion
    }
}