namespace DigitalPlatform
{
    partial class SerialCodeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialCodeForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_originCode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serialCode = new System.Windows.Forms.TextBox();
            this.label_message = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_copyNicInfomation = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "特征字符串(&O):";
            // 
            // textBox_originCode
            // 
            this.textBox_originCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_originCode.Location = new System.Drawing.Point(9, 22);
            this.textBox_originCode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_originCode.Multiline = true;
            this.textBox_originCode.Name = "textBox_originCode";
            this.textBox_originCode.ReadOnly = true;
            this.textBox_originCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_originCode.Size = new System.Drawing.Size(324, 64);
            this.textBox_originCode.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 158);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "序列号(&S):";
            // 
            // textBox_serialCode
            // 
            this.textBox_serialCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serialCode.Location = new System.Drawing.Point(9, 173);
            this.textBox_serialCode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_serialCode.Multiline = true;
            this.textBox_serialCode.Name = "textBox_serialCode";
            this.textBox_serialCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_serialCode.Size = new System.Drawing.Size(324, 64);
            this.textBox_serialCode.TabIndex = 3;
            // 
            // label_message
            // 
            this.label_message.Location = new System.Drawing.Point(9, 97);
            this.label_message.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(322, 51);
            this.label_message.TabIndex = 4;
            this.label_message.Text = "1) 请将上面的特征字符串完整复制后通过 email 或其他方式发送给数字平台，以获得序列号。\r\n\r\n2) 然后将序列号粘贴到下面的文本框中即可完成设置。";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(214, 253);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(275, 253);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(56, 22);
            this.button_cancel.TabIndex = 6;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_copyNicInfomation
            // 
            this.button_copyNicInfomation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_copyNicInfomation.Location = new System.Drawing.Point(12, 252);
            this.button_copyNicInfomation.Name = "button_copyNicInfomation";
            this.button_copyNicInfomation.Size = new System.Drawing.Size(171, 23);
            this.button_copyNicInfomation.TabIndex = 7;
            this.button_copyNicInfomation.Text = "Copy NIC Information(&C)";
            this.button_copyNicInfomation.UseVisualStyleBackColor = true;
            this.button_copyNicInfomation.Click += new System.EventHandler(this.button_copyNicInfomation_Click);
            // 
            // SerialCodeForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 285);
            this.Controls.Add(this.button_copyNicInfomation);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.textBox_serialCode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_originCode);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SerialCodeForm";
            this.ShowInTaskbar = false;
            this.Text = "设置序列号";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_originCode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_serialCode;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_copyNicInfomation;
    }
}