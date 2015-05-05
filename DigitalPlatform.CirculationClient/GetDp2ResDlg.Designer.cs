namespace DigitalPlatform.CirculationClient
{
    partial class GetDp2ResDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetDp2ResDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_path = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.dp2ResTree1 = new DigitalPlatform.CirculationClient.dp2ResTree();
            this.checkBox_notAsk = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 172);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "名称(&N):";
            // 
            // textBox_path
            // 
            this.textBox_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_path.Location = new System.Drawing.Point(75, 170);
            this.textBox_path.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_path.Name = "textBox_path";
            this.textBox_path.Size = new System.Drawing.Size(155, 21);
            this.textBox_path.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(233, 169);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(65, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(233, 193);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(65, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // dp2ResTree1
            // 
            this.dp2ResTree1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dp2ResTree1.ImageIndex = 0;
            this.dp2ResTree1.Location = new System.Drawing.Point(10, 10);
            this.dp2ResTree1.Margin = new System.Windows.Forms.Padding(2);
            this.dp2ResTree1.Name = "dp2ResTree1";
            this.dp2ResTree1.SelectedImageIndex = 0;
            this.dp2ResTree1.Size = new System.Drawing.Size(290, 154);
            this.dp2ResTree1.TabIndex = 0;
            this.dp2ResTree1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dp2ResTree1_AfterSelect);
            // 
            // checkBox_notAsk
            // 
            this.checkBox_notAsk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_notAsk.AutoSize = true;
            this.checkBox_notAsk.Enabled = false;
            this.checkBox_notAsk.Location = new System.Drawing.Point(11, 199);
            this.checkBox_notAsk.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_notAsk.Name = "checkBox_notAsk";
            this.checkBox_notAsk.Size = new System.Drawing.Size(144, 16);
            this.checkBox_notAsk.TabIndex = 3;
            this.checkBox_notAsk.Text = "下次不再出现此对话框";
            // 
            // GetDp2ResDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(308, 225);
            this.Controls.Add(this.checkBox_notAsk);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_path);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dp2ResTree1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "GetDp2ResDlg";
            this.ShowInTaskbar = false;
            this.Text = "GetDp2ResDlg";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetDp2ResDlg_FormClosed);
            this.Load += new System.EventHandler(this.GetDp2ResDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private dp2ResTree dp2ResTree1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_path;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_notAsk;
    }
}