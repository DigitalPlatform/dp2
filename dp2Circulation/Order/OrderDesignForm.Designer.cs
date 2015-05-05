namespace dp2Circulation
{
    partial class OrderDesignForm
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
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.orderDesignControl1 = new DigitalPlatform.CommonControl.OrderDesignControl();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(198, 196);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(259, 196);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // orderDesignControl1
            // 
            this.orderDesignControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.orderDesignControl1.BackColor = System.Drawing.SystemColors.Window;
            this.orderDesignControl1.Changed = true;
            this.orderDesignControl1.Location = new System.Drawing.Point(10, 10);
            this.orderDesignControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.orderDesignControl1.Name = "orderDesignControl1";
            this.orderDesignControl1.NewlyOrderTotalCopy = 0;
            this.orderDesignControl1.Size = new System.Drawing.Size(305, 181);
            this.orderDesignControl1.TabIndex = 0;
            this.orderDesignControl1.TargetRecPath = "";
            // 
            // OrderDesignForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 228);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.orderDesignControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "OrderDesignForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "订购";
            this.Load += new System.EventHandler(this.OrderDesignForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.CommonControl.OrderDesignControl orderDesignControl1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}