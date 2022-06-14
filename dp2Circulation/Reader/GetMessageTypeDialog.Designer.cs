
namespace dp2Circulation
{
    partial class GetMessageTypeDialog
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
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_mq = new System.Windows.Forms.CheckBox();
            this.checkBox_dpmail = new System.Windows.Forms.CheckBox();
            this.checkBox_sms = new System.Windows.Forms.CheckBox();
            this.checkBox_email = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(647, 396);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(138, 40);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(497, 396);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_mq
            // 
            this.checkBox_mq.AutoSize = true;
            this.checkBox_mq.Location = new System.Drawing.Point(13, 55);
            this.checkBox_mq.Name = "checkBox_mq";
            this.checkBox_mq.Size = new System.Drawing.Size(120, 25);
            this.checkBox_mq.TabIndex = 5;
            this.checkBox_mq.Text = "消息队列";
            this.checkBox_mq.UseVisualStyleBackColor = true;
            // 
            // checkBox_dpmail
            // 
            this.checkBox_dpmail.AutoSize = true;
            this.checkBox_dpmail.Location = new System.Drawing.Point(13, 86);
            this.checkBox_dpmail.Name = "checkBox_dpmail";
            this.checkBox_dpmail.Size = new System.Drawing.Size(102, 25);
            this.checkBox_dpmail.TabIndex = 6;
            this.checkBox_dpmail.Text = "dpMail";
            this.checkBox_dpmail.UseVisualStyleBackColor = true;
            // 
            // checkBox_sms
            // 
            this.checkBox_sms.AutoSize = true;
            this.checkBox_sms.Location = new System.Drawing.Point(13, 117);
            this.checkBox_sms.Name = "checkBox_sms";
            this.checkBox_sms.Size = new System.Drawing.Size(120, 25);
            this.checkBox_sms.TabIndex = 7;
            this.checkBox_sms.Text = "手机短信";
            this.checkBox_sms.UseVisualStyleBackColor = true;
            // 
            // checkBox_email
            // 
            this.checkBox_email.AutoSize = true;
            this.checkBox_email.Location = new System.Drawing.Point(13, 148);
            this.checkBox_email.Name = "checkBox_email";
            this.checkBox_email.Size = new System.Drawing.Size(91, 25);
            this.checkBox_email.TabIndex = 8;
            this.checkBox_email.Text = "Email";
            this.checkBox_email.UseVisualStyleBackColor = true;
            // 
            // GetMessageTypeDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.checkBox_email);
            this.Controls.Add(this.checkBox_sms);
            this.Controls.Add(this.checkBox_dpmail);
            this.Controls.Add(this.checkBox_mq);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "GetMessageTypeDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请选择消息类型";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_mq;
        private System.Windows.Forms.CheckBox checkBox_dpmail;
        private System.Windows.Forms.CheckBox checkBox_sms;
        private System.Windows.Forms.CheckBox checkBox_email;
    }
}