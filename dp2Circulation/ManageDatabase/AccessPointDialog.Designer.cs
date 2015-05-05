namespace dp2Circulation
{
    partial class AccessPointDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccessPointDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_weight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.comboBox_fromName = new System.Windows.Forms.ComboBox();
            this.tabComboBox_searchStyle = new DigitalPlatform.CommonControl.TabComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "检索途径(&F):";
            // 
            // textBox_weight
            // 
            this.textBox_weight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_weight.Location = new System.Drawing.Point(93, 33);
            this.textBox_weight.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_weight.Name = "textBox_weight";
            this.textBox_weight.Size = new System.Drawing.Size(118, 21);
            this.textBox_weight.TabIndex = 3;
            this.textBox_weight.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_weight_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 35);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "权值(&W):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 60);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "检索方式(&S):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(93, 132);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(154, 132);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // comboBox_fromName
            // 
            this.comboBox_fromName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_fromName.FormattingEnabled = true;
            this.comboBox_fromName.Items.AddRange(new object[] {
            "exact",
            "left",
            "middle",
            "right"});
            this.comboBox_fromName.Location = new System.Drawing.Point(93, 10);
            this.comboBox_fromName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_fromName.Name = "comboBox_fromName";
            this.comboBox_fromName.Size = new System.Drawing.Size(118, 20);
            this.comboBox_fromName.TabIndex = 1;
            // 
            // tabComboBox_searchStyle
            // 
            this.tabComboBox_searchStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabComboBox_searchStyle.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_searchStyle.FormattingEnabled = true;
            this.tabComboBox_searchStyle.Items.AddRange(new object[] {
            "exact\t精确一致",
            "left\t前方一致",
            "middle\t中间一致",
            "right\t后方一致"});
            this.tabComboBox_searchStyle.Location = new System.Drawing.Point(93, 57);
            this.tabComboBox_searchStyle.Name = "tabComboBox_searchStyle";
            this.tabComboBox_searchStyle.Size = new System.Drawing.Size(118, 22);
            this.tabComboBox_searchStyle.TabIndex = 5;
            // 
            // AccessPointDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(219, 164);
            this.Controls.Add(this.tabComboBox_searchStyle);
            this.Controls.Add(this.comboBox_fromName);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_weight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "AccessPointDialog";
            this.ShowInTaskbar = false;
            this.Text = "检索点事项";
            this.Load += new System.EventHandler(this.AccessPointDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_weight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ComboBox comboBox_fromName;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_searchStyle;
    }
}