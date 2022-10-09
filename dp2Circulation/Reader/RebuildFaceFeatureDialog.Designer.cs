namespace dp2Circulation.Reader
{
    partial class RebuildFaceFeatureDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_onlyRebuildExistingFaceElement = new System.Windows.Forms.CheckBox();
            this.checkBox_searchDup = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.Location = new System.Drawing.Point(674, 385);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(114, 53);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(557, 385);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(114, 53);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_onlyRebuildExistingFaceElement
            // 
            this.checkBox_onlyRebuildExistingFaceElement.AutoSize = true;
            this.checkBox_onlyRebuildExistingFaceElement.Location = new System.Drawing.Point(13, 40);
            this.checkBox_onlyRebuildExistingFaceElement.Name = "checkBox_onlyRebuildExistingFaceElement";
            this.checkBox_onlyRebuildExistingFaceElement.Size = new System.Drawing.Size(312, 25);
            this.checkBox_onlyRebuildExistingFaceElement.TabIndex = 7;
            this.checkBox_onlyRebuildExistingFaceElement.Text = "仅重建已经存在的 face 元素";
            this.checkBox_onlyRebuildExistingFaceElement.UseVisualStyleBackColor = true;
            // 
            // checkBox_searchDup
            // 
            this.checkBox_searchDup.AutoSize = true;
            this.checkBox_searchDup.Location = new System.Drawing.Point(13, 71);
            this.checkBox_searchDup.Name = "checkBox_searchDup";
            this.checkBox_searchDup.Size = new System.Drawing.Size(183, 25);
            this.checkBox_searchDup.TabIndex = 8;
            this.checkBox_searchDup.Text = "对人脸特征查重";
            this.checkBox_searchDup.UseVisualStyleBackColor = true;
            // 
            // RebuildFaceFeatureDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.checkBox_searchDup);
            this.Controls.Add(this.checkBox_onlyRebuildExistingFaceElement);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "RebuildFaceFeatureDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "重建人脸特征";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RebuildFaceFeatureDialog_FormClosed);
            this.Load += new System.EventHandler(this.RebuildFaceFeatureDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_onlyRebuildExistingFaceElement;
        private System.Windows.Forms.CheckBox checkBox_searchDup;
    }
}