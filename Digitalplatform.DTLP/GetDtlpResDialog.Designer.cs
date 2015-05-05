namespace DigitalPlatform.DTLP
{
    partial class GetDtlpResDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetDtlpResDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dtlpResDirControl1 = new DigitalPlatform.DTLP.DtlpResDirControl();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(311, 241);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(87, 28);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(311, 211);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(87, 28);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_path
            // 
            this.textBox_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_path.Location = new System.Drawing.Point(100, 214);
            this.textBox_path.Name = "textBox_path";
            this.textBox_path.Size = new System.Drawing.Size(205, 25);
            this.textBox_path.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 218);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "名称(&N):";
            // 
            // dtlpResDirControl1
            // 
            this.dtlpResDirControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtlpResDirControl1.HideSelection = false;
            this.dtlpResDirControl1.ImageIndex = 0;
            this.dtlpResDirControl1.Location = new System.Drawing.Point(12, 13);
            this.dtlpResDirControl1.Name = "dtlpResDirControl1";
            this.dtlpResDirControl1.SelectedImageIndex = 0;
            this.dtlpResDirControl1.SelectedPath1 = null;
            this.dtlpResDirControl1.Size = new System.Drawing.Size(386, 192);
            this.dtlpResDirControl1.TabIndex = 9;
            this.dtlpResDirControl1.ItemSelected += new DigitalPlatform.DTLP.ItemSelectedEventHandle(this.dtlpResDirControl1_ItemSelected);
            this.dtlpResDirControl1.GetItemTextStyle += new DigitalPlatform.DTLP.GetItemTextStyleEventHandle(this.dtlpResDirControl1_GetItemTextStyle);
            // 
            // GetDtlpResDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(410, 281);
            this.Controls.Add(this.dtlpResDirControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_path);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GetDtlpResDialog";
            this.ShowInTaskbar = false;
            this.Text = "GetDtlpResDialog";
            this.Load += new System.EventHandler(this.GetDtlpResDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_path;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.DTLP.DtlpResDirControl dtlpResDirControl1;
    }
}