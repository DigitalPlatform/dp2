namespace DigitalPlatform.CommonControl
{
    partial class W3cDtfDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(W3cDtfDialog));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label_label = new System.Windows.Forms.Label();
            this.w3cDtfControl1 = new DigitalPlatform.CommonControl.W3cDtfControl();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(258, 75);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(339, 75);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label_label
            // 
            this.label_label.AutoSize = true;
            this.label_label.Location = new System.Drawing.Point(8, 28);
            this.label_label.Name = "label_label";
            this.label_label.Size = new System.Drawing.Size(45, 15);
            this.label_label.TabIndex = 3;
            this.label_label.Text = "时间:";
            // 
            // w3cDtfControl1
            // 
            this.w3cDtfControl1.AutoSize = true;
            this.w3cDtfControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.w3cDtfControl1.Location = new System.Drawing.Point(11, 46);
            this.w3cDtfControl1.Name = "w3cDtfControl1";
            this.w3cDtfControl1.Size = new System.Drawing.Size(403, 23);
            this.w3cDtfControl1.TabIndex = 0;
            this.w3cDtfControl1.ValueString = "";
            // 
            // W3cDtfDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 113);
            this.ControlBox = false;
            this.Controls.Add(this.label_label);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.w3cDtfControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "W3cDtfDialog";
            this.ShowInTaskbar = false;
            this.Text = "编辑时间";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private W3cDtfControl w3cDtfControl1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label_label;
    }
}