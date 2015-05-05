namespace DigitalPlatform.CommonControl
{
    partial class MarcTemplateControl
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
            this.tableLayoutPanel_content = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_content
            // 
            this.tableLayoutPanel_content.ColumnCount = 3;
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_content.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_content.Name = "tableLayoutPanel_content";
            this.tableLayoutPanel_content.RowCount = 2;
            this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_content.Size = new System.Drawing.Size(233, 244);
            this.tableLayoutPanel_content.TabIndex = 0;
            // 
            // MarcTemplateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel_content);
            this.Name = "MarcTemplateControl";
            this.Size = new System.Drawing.Size(199, 150);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_content;
    }
}
