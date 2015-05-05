namespace dp2Circulation
{
    partial class ChargingInfoDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChargingInfoDlg));
            this.label_colorBar = new System.Windows.Forms.Label();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_fastInputText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer_transparent = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label_colorBar
            // 
            this.label_colorBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_colorBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label_colorBar.Location = new System.Drawing.Point(0, 0);
            this.label_colorBar.Name = "label_colorBar";
            this.label_colorBar.Size = new System.Drawing.Size(585, 60);
            this.label_colorBar.TabIndex = 0;
            this.label_colorBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_colorBar_MouseDown);
            this.label_colorBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_colorBar_MouseUp);
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.Font = new System.Drawing.Font("SimSun", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(3, 102);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(10);
            this.textBox_message.MaxLength = 0;
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(582, 124);
            this.textBox_message.TabIndex = 1;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_OK.Location = new System.Drawing.Point(510, 239);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_fastInputText
            // 
            this.textBox_fastInputText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_fastInputText.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_fastInputText.Location = new System.Drawing.Point(105, 242);
            this.textBox_fastInputText.Name = "textBox_fastInputText";
            this.textBox_fastInputText.Size = new System.Drawing.Size(399, 25);
            this.textBox_fastInputText.TabIndex = 3;
            this.textBox_fastInputText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_fastInputText_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 245);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "便捷输入(&F):";
            // 
            // timer_transparent
            // 
            this.timer_transparent.Interval = 1500;
            this.timer_transparent.Tick += new System.EventHandler(this.timer_transparent_Tick);
            // 
            // ChargingInfoDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 279);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_fastInputText);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.label_colorBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChargingInfoDlg";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "操作信息";
            this.Load += new System.EventHandler(this.ChargingInfoDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_colorBar;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_fastInputText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer_transparent;
    }
}