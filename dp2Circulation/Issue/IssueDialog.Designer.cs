namespace dp2Circulation
{
    partial class IssueDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IssueDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_publishTime = new System.Windows.Forms.TextBox();
            this.textBox_issue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_zong = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_volume = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_editComment = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "出版时间(&P):";
            // 
            // textBox_publishTime
            // 
            this.textBox_publishTime.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_publishTime.Location = new System.Drawing.Point(137, 12);
            this.textBox_publishTime.Name = "textBox_publishTime";
            this.textBox_publishTime.Size = new System.Drawing.Size(206, 25);
            this.textBox_publishTime.TabIndex = 1;
            // 
            // textBox_issue
            // 
            this.textBox_issue.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_issue.Location = new System.Drawing.Point(137, 91);
            this.textBox_issue.Name = "textBox_issue";
            this.textBox_issue.Size = new System.Drawing.Size(118, 25);
            this.textBox_issue.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "期号(&I):";
            // 
            // textBox_zong
            // 
            this.textBox_zong.Location = new System.Drawing.Point(137, 122);
            this.textBox_zong.Name = "textBox_zong";
            this.textBox_zong.Size = new System.Drawing.Size(118, 25);
            this.textBox_zong.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "总期号(&N):";
            // 
            // textBox_volume
            // 
            this.textBox_volume.Location = new System.Drawing.Point(137, 153);
            this.textBox_volume.Name = "textBox_volume";
            this.textBox_volume.Size = new System.Drawing.Size(118, 25);
            this.textBox_volume.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "卷号(&V):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(310, 376);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 13;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(229, 376);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 12;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_editComment
            // 
            this.textBox_editComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_editComment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_editComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_editComment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_editComment.Location = new System.Drawing.Point(15, 270);
            this.textBox_editComment.Multiline = true;
            this.textBox_editComment.Name = "textBox_editComment";
            this.textBox_editComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_editComment.Size = new System.Drawing.Size(370, 100);
            this.textBox_editComment.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(137, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(248, 44);
            this.label5.TabIndex = 2;
            this.label5.Text = "注：出版时间为8个数字的形态，例如：20080101";
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(137, 183);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(248, 81);
            this.textBox_comment.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 186);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "注释(&C):";
            // 
            // IssueDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(397, 416);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_editComment);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_volume);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_zong);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_issue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_publishTime);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IssueDialog";
            this.ShowInTaskbar = false;
            this.Text = "期信息";
            this.Load += new System.EventHandler(this.IssueDialog_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IssueDialog_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IssueDialog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_publishTime;
        private System.Windows.Forms.TextBox textBox_issue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_zong;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_volume;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_editComment;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Label label6;
    }
}