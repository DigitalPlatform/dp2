namespace DigitalPlatform.DTLP
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
            this.textBox_fromName = new System.Windows.Forms.TextBox();
            this.textBox_weight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_searchStyle = new System.Windows.Forms.ComboBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "来源名(&F):";
            // 
            // textBox_fromName
            // 
            this.textBox_fromName.Location = new System.Drawing.Point(124, 12);
            this.textBox_fromName.Name = "textBox_fromName";
            this.textBox_fromName.Size = new System.Drawing.Size(127, 25);
            this.textBox_fromName.TabIndex = 1;
            // 
            // textBox_weight
            // 
            this.textBox_weight.Location = new System.Drawing.Point(124, 43);
            this.textBox_weight.Name = "textBox_weight";
            this.textBox_weight.Size = new System.Drawing.Size(91, 25);
            this.textBox_weight.TabIndex = 3;
            this.textBox_weight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_weight.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_weight_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "权值(&W):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "检索方式(&S):";
            // 
            // comboBox_searchStyle
            // 
            this.comboBox_searchStyle.FormattingEnabled = true;
            this.comboBox_searchStyle.Items.AddRange(new object[] {
            "Exact",
            "Left"});
            this.comboBox_searchStyle.Location = new System.Drawing.Point(124, 74);
            this.comboBox_searchStyle.Name = "comboBox_searchStyle";
            this.comboBox_searchStyle.Size = new System.Drawing.Size(131, 23);
            this.comboBox_searchStyle.TabIndex = 5;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(124, 165);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(205, 165);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // AccessPointDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(292, 205);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_searchStyle);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_weight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_fromName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AccessPointDialog";
            this.ShowInTaskbar = false;
            this.Text = "检索点事项";
            this.Load += new System.EventHandler(this.AccessPointDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_fromName;
        private System.Windows.Forms.TextBox textBox_weight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_searchStyle;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}