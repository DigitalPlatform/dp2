namespace dp2Circulation
{
    partial class ZhongcihaoGroupDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZhongcihaoGroupDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_groupName = new System.Windows.Forms.TextBox();
            this.textBox_zhongcihaoDbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_getZhongcihaoDbName = new System.Windows.Forms.Button();
            this.textBox_comment = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "组名(&N):";
            // 
            // textBox_groupName
            // 
            this.textBox_groupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_groupName.Location = new System.Drawing.Point(151, 13);
            this.textBox_groupName.Name = "textBox_groupName";
            this.textBox_groupName.Size = new System.Drawing.Size(204, 25);
            this.textBox_groupName.TabIndex = 1;
            // 
            // textBox_zhongcihaoDbName
            // 
            this.textBox_zhongcihaoDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_zhongcihaoDbName.Location = new System.Drawing.Point(151, 44);
            this.textBox_zhongcihaoDbName.Name = "textBox_zhongcihaoDbName";
            this.textBox_zhongcihaoDbName.Size = new System.Drawing.Size(204, 25);
            this.textBox_zhongcihaoDbName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "种次号库名(&Z):";
            // 
            // button_getZhongcihaoDbName
            // 
            this.button_getZhongcihaoDbName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getZhongcihaoDbName.Location = new System.Drawing.Point(361, 44);
            this.button_getZhongcihaoDbName.Name = "button_getZhongcihaoDbName";
            this.button_getZhongcihaoDbName.Size = new System.Drawing.Size(55, 28);
            this.button_getZhongcihaoDbName.TabIndex = 4;
            this.button_getZhongcihaoDbName.Text = "...";
            this.button_getZhongcihaoDbName.UseVisualStyleBackColor = true;
            this.button_getZhongcihaoDbName.Click += new System.EventHandler(this.button_getZhongcihaoDbName_Click);
            // 
            // textBox_script_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_comment.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_comment.HideSelection = false;
            this.textBox_comment.Location = new System.Drawing.Point(12, 79);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_script_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(404, 138);
            this.textBox_comment.TabIndex = 11;
            this.textBox_comment.Text = "注：一个组由共享同一种次号库的若干书目库构成。这些书目库都在一个物理书库空间中进行排架。";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(341, 227);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 21;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(260, 227);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 20;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // ZhongcihaoGroupDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(428, 267);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.button_getZhongcihaoDbName);
            this.Controls.Add(this.textBox_zhongcihaoDbName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_groupName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ZhongcihaoGroupDialog";
            this.ShowInTaskbar = false;
            this.Text = "种次号 组";
            this.Load += new System.EventHandler(this.ZhongcihaoGroupDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_groupName;
        private System.Windows.Forms.TextBox textBox_zhongcihaoDbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getZhongcihaoDbName;
        private DigitalPlatform.GUI.NoHasSelTextBox textBox_comment;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}