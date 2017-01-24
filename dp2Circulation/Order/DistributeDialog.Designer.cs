namespace dp2Circulation
{
    partial class DistributeDialog
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
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.locationEditControl1 = new DigitalPlatform.CommonControl.LocationEditControl();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_text
            // 
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.Location = new System.Drawing.Point(13, 13);
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.Size = new System.Drawing.Size(369, 21);
            this.textBox_text.TabIndex = 0;
            this.textBox_text.TextChanged += new System.EventHandler(this.textBox_text_TextChanged);
            this.textBox_text.Leave += new System.EventHandler(this.textBox_text_Leave);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.locationEditControl1);
            this.panel1.Location = new System.Drawing.Point(13, 41);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(369, 221);
            this.panel1.TabIndex = 2;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(307, 268);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 22;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(226, 268);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 21;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // locationEditControl1
            // 
            this.locationEditControl1.ArrivedCount = 0;
            this.locationEditControl1.Changed = false;
            this.locationEditControl1.Count = 0;
            this.locationEditControl1.HideSelection = true;
            this.locationEditControl1.Location = new System.Drawing.Point(0, 0);
            this.locationEditControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.locationEditControl1.MaximumSize = new System.Drawing.Size(0, 24);
            this.locationEditControl1.MinimumSize = new System.Drawing.Size(276, 30);
            this.locationEditControl1.Name = "locationEditControl1";
            this.locationEditControl1.ReadOnly = false;
            this.locationEditControl1.Size = new System.Drawing.Size(276, 30);
            this.locationEditControl1.TabIndex = 1;
            this.locationEditControl1.Value = "";
            this.locationEditControl1.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(this.locationEditControl1_ContentChanged);
            this.locationEditControl1.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(this.locationEditControl1_GetValueTable);
            this.locationEditControl1.Leave += new System.EventHandler(this.locationEditControl1_Leave);
            // 
            // DistributeDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(394, 303);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textBox_text);
            this.Name = "DistributeDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "去向";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_text;
        private DigitalPlatform.CommonControl.LocationEditControl locationEditControl1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}