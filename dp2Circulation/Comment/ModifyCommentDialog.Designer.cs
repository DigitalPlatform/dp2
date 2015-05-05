namespace dp2Circulation
{
    partial class ModifyCommentDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyCommentDialog));
            this.label_comment = new System.Windows.Forms.Label();
            this.splitContainer_comment = new System.Windows.Forms.SplitContainer();
            this.radioButton_overwrite = new System.Windows.Forms.RadioButton();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.button_oldComment_insertDateTime = new System.Windows.Forms.Button();
            this.radioButton_append = new System.Windows.Forms.RadioButton();
            this.button_newComment_insertDateTime = new System.Windows.Forms.Button();
            this.textBox_appendComment = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_price = new System.Windows.Forms.TextBox();
            this.textBox_id = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip_usage = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_comment)).BeginInit();
            this.splitContainer_comment.Panel1.SuspendLayout();
            this.splitContainer_comment.Panel2.SuspendLayout();
            this.splitContainer_comment.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_comment
            // 
            this.label_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_comment.BackColor = System.Drawing.SystemColors.Info;
            this.label_comment.Location = new System.Drawing.Point(10, 242);
            this.label_comment.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_comment.Name = "label_comment";
            this.label_comment.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.label_comment.Size = new System.Drawing.Size(385, 50);
            this.label_comment.TabIndex = 4;
            this.label_comment.Text = "注: 当最后提交到服务器的时候，新注释将自动追加在旧注释的后面。";
            // 
            // splitContainer_comment
            // 
            this.splitContainer_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_comment.Location = new System.Drawing.Point(10, 59);
            this.splitContainer_comment.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer_comment.Name = "splitContainer_comment";
            this.splitContainer_comment.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_comment.Panel1
            // 
            this.splitContainer_comment.Panel1.Controls.Add(this.radioButton_overwrite);
            this.splitContainer_comment.Panel1.Controls.Add(this.textBox_comment);
            this.splitContainer_comment.Panel1.Controls.Add(this.button_oldComment_insertDateTime);
            // 
            // splitContainer_comment.Panel2
            // 
            this.splitContainer_comment.Panel2.Controls.Add(this.radioButton_append);
            this.splitContainer_comment.Panel2.Controls.Add(this.button_newComment_insertDateTime);
            this.splitContainer_comment.Panel2.Controls.Add(this.textBox_appendComment);
            this.splitContainer_comment.Size = new System.Drawing.Size(385, 177);
            this.splitContainer_comment.SplitterDistance = 86;
            this.splitContainer_comment.SplitterWidth = 6;
            this.splitContainer_comment.TabIndex = 20;
            // 
            // radioButton_overwrite
            // 
            this.radioButton_overwrite.AutoSize = true;
            this.radioButton_overwrite.Location = new System.Drawing.Point(0, 0);
            this.radioButton_overwrite.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_overwrite.Name = "radioButton_overwrite";
            this.radioButton_overwrite.Size = new System.Drawing.Size(107, 16);
            this.radioButton_overwrite.TabIndex = 0;
            this.radioButton_overwrite.TabStop = true;
            this.radioButton_overwrite.Text = "直接修改旧注释";
            this.radioButton_overwrite.UseVisualStyleBackColor = true;
            this.radioButton_overwrite.CheckedChanged += new System.EventHandler(this.radioButton_overwrite_CheckedChanged);
            // 
            // textBox_comment
            // 
            this.textBox_comment.AcceptsReturn = true;
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(76, 20);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(281, 65);
            this.textBox_comment.TabIndex = 1;
            this.textBox_comment.TextChanged += new System.EventHandler(this.textBox_oldComment_TextChanged);
            this.textBox_comment.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_comment_Validating);
            // 
            // button_oldComment_insertDateTime
            // 
            this.button_oldComment_insertDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_oldComment_insertDateTime.Enabled = false;
            this.button_oldComment_insertDateTime.Image = ((System.Drawing.Image)(resources.GetObject("button_oldComment_insertDateTime.Image")));
            this.button_oldComment_insertDateTime.Location = new System.Drawing.Point(361, 20);
            this.button_oldComment_insertDateTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_oldComment_insertDateTime.Name = "button_oldComment_insertDateTime";
            this.button_oldComment_insertDateTime.Size = new System.Drawing.Size(24, 22);
            this.button_oldComment_insertDateTime.TabIndex = 2;
            this.button_oldComment_insertDateTime.UseVisualStyleBackColor = true;
            this.button_oldComment_insertDateTime.Click += new System.EventHandler(this.button_oldComment_insertDateTime_Click);
            this.button_oldComment_insertDateTime.MouseHover += new System.EventHandler(this.button_oldComment_insertDateTime_MouseHover);
            // 
            // radioButton_append
            // 
            this.radioButton_append.AutoSize = true;
            this.radioButton_append.Checked = true;
            this.radioButton_append.Location = new System.Drawing.Point(0, 2);
            this.radioButton_append.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_append.Name = "radioButton_append";
            this.radioButton_append.Size = new System.Drawing.Size(83, 16);
            this.radioButton_append.TabIndex = 0;
            this.radioButton_append.TabStop = true;
            this.radioButton_append.Text = "追加新注释";
            this.radioButton_append.UseVisualStyleBackColor = true;
            this.radioButton_append.CheckedChanged += new System.EventHandler(this.radioButton_append_CheckedChanged);
            // 
            // button_newComment_insertDateTime
            // 
            this.button_newComment_insertDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_newComment_insertDateTime.Image = ((System.Drawing.Image)(resources.GetObject("button_newComment_insertDateTime.Image")));
            this.button_newComment_insertDateTime.Location = new System.Drawing.Point(361, 19);
            this.button_newComment_insertDateTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_newComment_insertDateTime.Name = "button_newComment_insertDateTime";
            this.button_newComment_insertDateTime.Size = new System.Drawing.Size(24, 22);
            this.button_newComment_insertDateTime.TabIndex = 2;
            this.button_newComment_insertDateTime.UseVisualStyleBackColor = true;
            this.button_newComment_insertDateTime.Click += new System.EventHandler(this.button_newComment_insertDateTime_Click);
            this.button_newComment_insertDateTime.MouseHover += new System.EventHandler(this.button_newComment_insertDateTime_MouseHover);
            // 
            // textBox_appendComment
            // 
            this.textBox_appendComment.AcceptsReturn = true;
            this.textBox_appendComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_appendComment.Location = new System.Drawing.Point(76, 19);
            this.textBox_appendComment.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_appendComment.Multiline = true;
            this.textBox_appendComment.Name = "textBox_appendComment";
            this.textBox_appendComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_appendComment.Size = new System.Drawing.Size(281, 64);
            this.textBox_appendComment.TabIndex = 1;
            this.textBox_appendComment.TextChanged += new System.EventHandler(this.textBox_newComment_TextChanged);
            this.textBox_appendComment.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_appendComment_Validating);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(338, 294);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(278, 294);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_price
            // 
            this.textBox_price.Location = new System.Drawing.Point(86, 34);
            this.textBox_price.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_price.Name = "textBox_price";
            this.textBox_price.ReadOnly = true;
            this.textBox_price.Size = new System.Drawing.Size(157, 21);
            this.textBox_price.TabIndex = 3;
            // 
            // textBox_id
            // 
            this.textBox_id.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_id.Location = new System.Drawing.Point(86, 10);
            this.textBox_id.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_id.Name = "textBox_id";
            this.textBox_id.ReadOnly = true;
            this.textBox_id.Size = new System.Drawing.Size(309, 21);
            this.textBox_id.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "价格(&P):";
            // 
            // toolTip_usage
            // 
            this.toolTip_usage.ToolTipTitle = "用途";
            // 
            // ModifyCommentDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(404, 326);
            this.Controls.Add(this.label_comment);
            this.Controls.Add(this.splitContainer_comment);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_price);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_id);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "ModifyCommentDialog";
            this.ShowInTaskbar = false;
            this.Text = "变更注释";
            this.Load += new System.EventHandler(this.ModifyCommentDialog_Load);
            this.splitContainer_comment.Panel1.ResumeLayout(false);
            this.splitContainer_comment.Panel1.PerformLayout();
            this.splitContainer_comment.Panel2.ResumeLayout(false);
            this.splitContainer_comment.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_comment)).EndInit();
            this.splitContainer_comment.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_comment;
        private System.Windows.Forms.SplitContainer splitContainer_comment;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Button button_oldComment_insertDateTime;
        private System.Windows.Forms.Button button_newComment_insertDateTime;
        private System.Windows.Forms.TextBox textBox_appendComment;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_price;
        private System.Windows.Forms.TextBox textBox_id;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButton_overwrite;
        private System.Windows.Forms.RadioButton radioButton_append;
        private System.Windows.Forms.ToolTip toolTip_usage;
    }
}