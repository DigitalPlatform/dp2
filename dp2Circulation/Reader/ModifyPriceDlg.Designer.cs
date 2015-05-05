namespace dp2Circulation
{
    partial class ModifyPriceDlg
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyPriceDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_id = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_price = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_appendComment = new System.Windows.Forms.TextBox();
            this.button_newComment_insertDateTime = new System.Windows.Forms.Button();
            this.toolTip_usage = new System.Windows.Forms.ToolTip(this.components);
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_oldComment_insertDateTime = new System.Windows.Forms.Button();
            this.splitContainer_comment = new System.Windows.Forms.SplitContainer();
            this.label_comment = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_comment)).BeginInit();
            this.splitContainer_comment.Panel1.SuspendLayout();
            this.splitContainer_comment.Panel2.SuspendLayout();
            this.splitContainer_comment.SuspendLayout();
            this.SuspendLayout();
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
            // textBox_id
            // 
            this.textBox_id.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_id.Location = new System.Drawing.Point(86, 10);
            this.textBox_id.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_id.Name = "textBox_id";
            this.textBox_id.ReadOnly = true;
            this.textBox_id.Size = new System.Drawing.Size(309, 21);
            this.textBox_id.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "金额(&A):";
            // 
            // textBox_price
            // 
            this.textBox_price.Location = new System.Drawing.Point(86, 34);
            this.textBox_price.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_price.Name = "textBox_price";
            this.textBox_price.Size = new System.Drawing.Size(157, 21);
            this.textBox_price.TabIndex = 3;
            this.textBox_price.TextChanged += new System.EventHandler(this.textBox_price_TextChanged);
            this.textBox_price.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_price_Validating);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(278, 294);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(338, 294);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-2, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "新注释(&C):";
            // 
            // textBox_appendComment
            // 
            this.textBox_appendComment.AcceptsReturn = true;
            this.textBox_appendComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_appendComment.Location = new System.Drawing.Point(76, 0);
            this.textBox_appendComment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_appendComment.MaxLength = 0;
            this.textBox_appendComment.Multiline = true;
            this.textBox_appendComment.Name = "textBox_appendComment";
            this.textBox_appendComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_appendComment.Size = new System.Drawing.Size(281, 79);
            this.textBox_appendComment.TabIndex = 1;
            this.textBox_appendComment.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            this.textBox_appendComment.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_appendComment_Validating);
            this.textBox_appendComment.Validated += new System.EventHandler(this.textBox_appendComment_Validated);
            // 
            // button_newComment_insertDateTime
            // 
            this.button_newComment_insertDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_newComment_insertDateTime.Image = ((System.Drawing.Image)(resources.GetObject("button_newComment_insertDateTime.Image")));
            this.button_newComment_insertDateTime.Location = new System.Drawing.Point(361, 0);
            this.button_newComment_insertDateTime.Margin = new System.Windows.Forms.Padding(2);
            this.button_newComment_insertDateTime.Name = "button_newComment_insertDateTime";
            this.button_newComment_insertDateTime.Size = new System.Drawing.Size(24, 22);
            this.button_newComment_insertDateTime.TabIndex = 2;
            this.button_newComment_insertDateTime.UseVisualStyleBackColor = true;
            this.button_newComment_insertDateTime.Click += new System.EventHandler(this.button_insertDateTime_Click);
            this.button_newComment_insertDateTime.MouseHover += new System.EventHandler(this.button_insertDateTime_MouseHover);
            // 
            // toolTip_usage
            // 
            this.toolTip_usage.ToolTipTitle = "用途";
            // 
            // textBox_comment
            // 
            this.textBox_comment.AcceptsReturn = true;
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(76, 0);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_comment.MaxLength = 0;
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_comment.Size = new System.Drawing.Size(281, 85);
            this.textBox_comment.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "旧注释(&O):";
            // 
            // button_oldComment_insertDateTime
            // 
            this.button_oldComment_insertDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_oldComment_insertDateTime.Enabled = false;
            this.button_oldComment_insertDateTime.Image = ((System.Drawing.Image)(resources.GetObject("button_oldComment_insertDateTime.Image")));
            this.button_oldComment_insertDateTime.Location = new System.Drawing.Point(361, 0);
            this.button_oldComment_insertDateTime.Margin = new System.Windows.Forms.Padding(2);
            this.button_oldComment_insertDateTime.Name = "button_oldComment_insertDateTime";
            this.button_oldComment_insertDateTime.Size = new System.Drawing.Size(24, 22);
            this.button_oldComment_insertDateTime.TabIndex = 2;
            this.button_oldComment_insertDateTime.UseVisualStyleBackColor = true;
            // 
            // splitContainer_comment
            // 
            this.splitContainer_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_comment.Location = new System.Drawing.Point(10, 59);
            this.splitContainer_comment.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_comment.Name = "splitContainer_comment";
            this.splitContainer_comment.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_comment.Panel1
            // 
            this.splitContainer_comment.Panel1.Controls.Add(this.textBox_comment);
            this.splitContainer_comment.Panel1.Controls.Add(this.label4);
            this.splitContainer_comment.Panel1.Controls.Add(this.button_oldComment_insertDateTime);
            // 
            // splitContainer_comment.Panel2
            // 
            this.splitContainer_comment.Panel2.Controls.Add(this.label3);
            this.splitContainer_comment.Panel2.Controls.Add(this.button_newComment_insertDateTime);
            this.splitContainer_comment.Panel2.Controls.Add(this.textBox_appendComment);
            this.splitContainer_comment.Size = new System.Drawing.Size(385, 177);
            this.splitContainer_comment.SplitterDistance = 86;
            this.splitContainer_comment.SplitterWidth = 6;
            this.splitContainer_comment.TabIndex = 12;
            // 
            // label_comment
            // 
            this.label_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_comment.BackColor = System.Drawing.SystemColors.Info;
            this.label_comment.Location = new System.Drawing.Point(10, 242);
            this.label_comment.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_comment.Name = "label_comment";
            this.label_comment.Padding = new System.Windows.Forms.Padding(2);
            this.label_comment.Size = new System.Drawing.Size(385, 50);
            this.label_comment.TabIndex = 4;
            this.label_comment.Text = "注: 当最后提交到服务器的时候，新注释将自动追加在旧注释的后面。";
            // 
            // ModifyPriceDlg
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
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ModifyPriceDlg";
            this.ShowInTaskbar = false;
            this.Text = "变更金额";
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

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_id;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_price;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_appendComment;
        private System.Windows.Forms.Button button_newComment_insertDateTime;
        private System.Windows.Forms.ToolTip toolTip_usage;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_oldComment_insertDateTime;
        private System.Windows.Forms.SplitContainer splitContainer_comment;
        private System.Windows.Forms.Label label_comment;
    }
}