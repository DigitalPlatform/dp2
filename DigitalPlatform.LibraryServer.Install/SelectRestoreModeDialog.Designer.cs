namespace DigitalPlatform.LibraryServer
{
    partial class SelectRestoreModeDialog
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
            this.button_fullRestore = new System.Windows.Forms.Button();
            this.button_blank = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_fullRestore
            // 
            this.button_fullRestore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_fullRestore.Location = new System.Drawing.Point(13, 13);
            this.button_fullRestore.Name = "button_fullRestore";
            this.button_fullRestore.Size = new System.Drawing.Size(391, 23);
            this.button_fullRestore.TabIndex = 0;
            this.button_fullRestore.Text = "全部恢复(服务器定义+全部数据)";
            this.button_fullRestore.UseVisualStyleBackColor = true;
            this.button_fullRestore.Click += new System.EventHandler(this.button_fullRestore_Click);
            // 
            // button_blank
            // 
            this.button_blank.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_blank.Location = new System.Drawing.Point(12, 42);
            this.button_blank.Name = "button_blank";
            this.button_blank.Size = new System.Drawing.Size(391, 23);
            this.button_blank.TabIndex = 1;
            this.button_blank.Text = "空服务器";
            this.button_blank.UseVisualStyleBackColor = true;
            this.button_blank.Click += new System.EventHandler(this.button_blank_Click);
            // 
            // SelectRestoreModeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 103);
            this.Controls.Add(this.button_blank);
            this.Controls.Add(this.button_fullRestore);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectRestoreModeDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请选择恢复方式";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_fullRestore;
        private System.Windows.Forms.Button button_blank;
    }
}