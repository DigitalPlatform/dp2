namespace dp2Catalog
{
    partial class AmazonSimpleQueryControl
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_word = new System.Windows.Forms.Label();
            this.label_from = new System.Windows.Forms.Label();
            this.comboBox_from = new DigitalPlatform.CommonControl.TabComboBox();
            this.label_searchParameters = new System.Windows.Forms.Label();
            this.tabComboBox_match = new DigitalPlatform.CommonControl.TabComboBox();
            this.textBox_word = new System.Windows.Forms.TextBox();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label_word, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_from, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBox_from, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label_searchParameters, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tabComboBox_match, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBox_word, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBox_comment, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(311, 173);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // label_word
            // 
            this.label_word.AutoSize = true;
            this.label_word.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_word.Location = new System.Drawing.Point(3, 0);
            this.label_word.Name = "label_word";
            this.label_word.Size = new System.Drawing.Size(53, 27);
            this.label_word.TabIndex = 0;
            this.label_word.Text = "检索词";
            this.label_word.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_from
            // 
            this.label_from.AutoSize = true;
            this.label_from.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_from.Location = new System.Drawing.Point(3, 27);
            this.label_from.Name = "label_from";
            this.label_from.Size = new System.Drawing.Size(53, 28);
            this.label_from.TabIndex = 2;
            this.label_from.Text = "检索途径";
            this.label_from.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox_from
            // 
            this.comboBox_from.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_from.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.Location = new System.Drawing.Point(62, 30);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(246, 22);
            this.comboBox_from.TabIndex = 3;
            this.comboBox_from.SelectedIndexChanged += new System.EventHandler(this.comboBox_from_SelectedIndexChanged);
            // 
            // label_searchParameters
            // 
            this.label_searchParameters.AutoSize = true;
            this.label_searchParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_searchParameters.Location = new System.Drawing.Point(3, 55);
            this.label_searchParameters.Name = "label_searchParameters";
            this.label_searchParameters.Size = new System.Drawing.Size(53, 28);
            this.label_searchParameters.TabIndex = 4;
            this.label_searchParameters.Text = "匹配方式";
            this.label_searchParameters.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabComboBox_match
            // 
            this.tabComboBox_match.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabComboBox_match.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_match.FormattingEnabled = true;
            this.tabComboBox_match.Location = new System.Drawing.Point(62, 58);
            this.tabComboBox_match.Name = "tabComboBox_match";
            this.tabComboBox_match.Size = new System.Drawing.Size(246, 22);
            this.tabComboBox_match.TabIndex = 5;
            // 
            // textBox_word
            // 
            this.textBox_word.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_word.Location = new System.Drawing.Point(62, 3);
            this.textBox_word.Name = "textBox_word";
            this.textBox_word.Size = new System.Drawing.Size(246, 21);
            this.textBox_word.TabIndex = 1;
            // 
            // textBox_comment
            // 
            this.textBox_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_comment.Location = new System.Drawing.Point(62, 86);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.Size = new System.Drawing.Size(246, 84);
            this.textBox_comment.TabIndex = 6;
            // 
            // AmazonSimpleQueryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AmazonSimpleQueryControl";
            this.Size = new System.Drawing.Size(311, 173);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label_from;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_from;
        private System.Windows.Forms.Label label_searchParameters;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_match;
        private System.Windows.Forms.TextBox textBox_word;
        private System.Windows.Forms.Label label_word;
        private System.Windows.Forms.TextBox textBox_comment;
    }
}
